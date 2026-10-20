using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using Microsoft.ML.Data;
using FinanceTracker.Shared.Data;
using FinanceTracker.Shared.Models;

namespace FinanceTracker.Shared.Services
{
    public class FinancialInsightService
    {
        private readonly FinanceDbContext _dbContext;
        private readonly MLContext _mlContext;

        public FinancialInsightService(FinanceDbContext dbContext)
        {
            _dbContext = dbContext;
            _mlContext = new MLContext(seed: 0);
        }

        // Internal data transfer object for ML.NET
        private class CategorySpendData
        {
            public float Amount { get; set; }
        }

        private class SpikePrediction
        {
            [VectorType(3)]
            public double[] Prediction { get; set; } = Array.Empty<double>();
        }

        public async Task<List<FinancialInsight>> GenerateInsightsAsync(string selectedPeriod)
        {
            var insights = new List<FinancialInsight>();

            var parts = selectedPeriod.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out int year) || !int.TryParse(parts[1], out int month))
                return insights;

            var currentMonthStart = new DateTime(year, month, 1);
            var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
            var lookbackStart = currentMonthStart.AddMonths(-3);

            // GLOBAL FILTER: Exclude Splits, Transfers, AND Uncategorized
            var transactions = await _dbContext.Transactions
                .Where(t => t.Date >= lookbackStart
                         && t.Date <= currentMonthEnd
                         && !t.IsSplit
                         && t.Type == "Expense"
                         && !string.IsNullOrEmpty(t.Category)
                         && t.Category != "Uncategorized")
                .ToListAsync();

            var currentMonthTxns = transactions.Where(t => t.Date >= currentMonthStart).ToList();
            var priorTxns = transactions.Where(t => t.Date < currentMonthStart).ToList();

            // Fetch Income for Margin calculations
            var incomeTransactions = await _dbContext.Transactions
                .Where(t => t.Date >= lookbackStart && t.Date <= currentMonthEnd && t.Type == "Income")
                .ToListAsync();

            if (!priorTxns.Any())
                return insights; // Need historical data

            var fixedCategories = (await _dbContext.CategoryConfigs
                .Where(c => c.IsFixedBill)
                .Select(c => c.Name)
                .ToListAsync())
                .ToHashSet();

            // Helper: Discretionary Transactions Only
            var currentDiscTxns = currentMonthTxns.Where(t => !fixedCategories.Contains(t.Category)).ToList();
            var priorDiscTxns = priorTxns.Where(t => !fixedCategories.Contains(t.Category)).ToList();

            // -------------------------------------------------------------
            // 1. Category Spending Velocity Spikes
            // -------------------------------------------------------------
            var discretionaryCategories = currentDiscTxns.Select(t => t.Category).Distinct();
            foreach (var category in discretionaryCategories)
            {
                var priorCatMonths = priorTxns.Where(t => t.Category == category)
                    .GroupBy(t => $"{t.Date.Year}-{t.Date.Month:D2}")
                    .Select(g => g.Sum(t => t.Amount)).ToList();

                if (priorCatMonths.Count < 2) continue;

                decimal priorMonthlyAvg = priorCatMonths.Average();
                decimal currentSpend = currentMonthTxns.Where(t => t.Category == category).Sum(t => t.Amount);

                if (priorMonthlyAvg > 20m && currentSpend > priorMonthlyAvg * 1.35m)
                {
                    decimal delta = currentSpend - priorMonthlyAvg;
                    insights.Add(new FinancialInsight
                    {
                        Category = category,
                        Title = $"{category} Surge",
                        Message = $"Pacing {Math.Round((delta / priorMonthlyAvg) * 100)}% above your 2-month baseline ({currentSpend:C} vs {priorMonthlyAvg:C} avg).",
                        Severity = (delta / priorMonthlyAvg) > 0.6m ? InsightSeverity.Alert : InsightSeverity.Warning,
                        Icon = "bi-graph-up-arrow",
                        PotentialSavings = delta
                    });
                }
            }

            // -------------------------------------------------------------
            // 2. Subscription & Bill Drift (Price Creep)
            // -------------------------------------------------------------
            var currentMonthDistinctMerchants = currentMonthTxns
                .GroupBy(t => t.CleanedDescription)
                .Where(g => g.Count() == 1).Select(g => g.First()).ToList();

            foreach (var currentCharge in currentMonthDistinctMerchants)
            {
                var priorCharges = priorTxns.Where(t => t.CleanedDescription == currentCharge.CleanedDescription).OrderByDescending(t => t.Date).ToList();
                if (priorCharges.Any())
                {
                    var lastCharge = priorCharges.First();
                    if (currentCharge.Amount > lastCharge.Amount + 0.50m) // > $0.50 to ignore minor tax rounding
                    {
                        decimal driftAmount = currentCharge.Amount - lastCharge.Amount;
                        if ((driftAmount / lastCharge.Amount) < 1.0m) // Ignore massive 100%+ spikes (usually means it's not a recurring bill)
                        {
                            insights.Add(new FinancialInsight
                            {
                                Category = currentCharge.Category,
                                Title = "Bill Price Increase",
                                Message = $"{currentCharge.CleanedDescription} increased from {lastCharge.Amount:C} to {currentCharge.Amount:C}.",
                                Severity = InsightSeverity.Warning,
                                Icon = "bi-arrow-up-right-square"
                            });
                        }
                    }
                }
            }

            // -------------------------------------------------------------
            // 3. Micro-Transaction Frequency Spikes
            // -------------------------------------------------------------
            int currentMicroCount = currentMonthTxns.Count(t => t.Amount <= 20m);
            var priorMicroAvg = priorTxns.GroupBy(t => $"{t.Date.Year}-{t.Date.Month:D2}").Select(g => g.Count(t => t.Amount <= 20m)).DefaultIfEmpty(0).Average();

            if (currentMicroCount > priorMicroAvg * 1.5 && currentMicroCount >= 10)
            {
                decimal currentMicroTotal = currentMonthTxns.Where(t => t.Amount <= 20m).Sum(t => t.Amount);
                insights.Add(new FinancialInsight
                {
                    Category = "Discretionary", // We'll route this to MaxAmount=20 via the UI
                    Title = "Frequent Micro-Purchases",
                    Message = $"{currentMicroCount} swipes under $20 logged this month (vs {Math.Round(priorMicroAvg)} baseline avg), totaling {currentMicroTotal:C}.",
                    Severity = InsightSeverity.Warning,
                    Icon = "bi-receipt-cutoff",
                    PotentialSavings = currentMicroTotal * 0.3m
                });
            }

            // -------------------------------------------------------------
            // 4. Weekend Discretionary Leakage
            // -------------------------------------------------------------
            decimal currentWeekendSpend = currentDiscTxns.Where(t => t.Date.DayOfWeek == DayOfWeek.Saturday || t.Date.DayOfWeek == DayOfWeek.Sunday).Sum(t => t.Amount);
            decimal currentTotalDisc = currentDiscTxns.Sum(t => t.Amount);

            if (currentTotalDisc > 0)
            {
                decimal currentWeekendRatio = currentWeekendSpend / currentTotalDisc;
                var priorMonthsWeekendRatios = priorDiscTxns
                    .GroupBy(t => $"{t.Date.Year}-{t.Date.Month:D2}")
                    .Select(g => g.Where(t => t.Date.DayOfWeek == DayOfWeek.Saturday || t.Date.DayOfWeek == DayOfWeek.Sunday).Sum(t => t.Amount) / (g.Sum(t => t.Amount) == 0 ? 1 : g.Sum(t => t.Amount)))
                    .ToList();

                decimal priorWeekendRatioAvg = priorMonthsWeekendRatios.Any() ? priorMonthsWeekendRatios.Average() : 0;

                // Flag if weekend spend is > 50% of all discretionary AND it jumped 15% above historical avg
                if (currentWeekendRatio > 0.50m && currentWeekendRatio > priorWeekendRatioAvg + 0.15m)
                {
                    insights.Add(new FinancialInsight
                    {
                        Category = "Discretionary",
                        Title = "Weekend Burn Rate",
                        Message = $"{Math.Round(currentWeekendRatio * 100)}% of your discretionary spending ({currentWeekendSpend:C}) occurred on weekends, up from your {Math.Round(priorWeekendRatioAvg * 100)}% average.",
                        Severity = InsightSeverity.Info,
                        Icon = "bi-calendar-weekend"
                    });
                }
            }

            // -------------------------------------------------------------
            // 5. Budget Exhaustion Acceleration
            // -------------------------------------------------------------
            var currentBudgets = await _dbContext.Budgets.Where(b => b.Period == selectedPeriod).ToListAsync();
            foreach (var budget in currentBudgets)
            {
                // Find what day we hit 80% of this month's budget
                decimal limit80 = budget.MonthlyLimit * 0.8m;
                if (limit80 == 0) continue;

                var monthTxns = currentMonthTxns.Where(t => t.Category == budget.Category).OrderBy(t => t.Date).ToList();
                decimal runningTotal = 0;
                int? dayHit80 = null;

                foreach (var txn in monthTxns)
                {
                    runningTotal += txn.Amount;
                    if (runningTotal >= limit80)
                    {
                        dayHit80 = txn.Date.Day;
                        break;
                    }
                }

                if (dayHit80.HasValue && dayHit80.Value <= 20) // Only flag if hit early (before the 20th)
                {
                    insights.Add(new FinancialInsight
                    {
                        Category = budget.Category,
                        Title = "Rapid Budget Exhaustion",
                        Message = $"You reached 80% of your {budget.Category} budget by Day {dayHit80.Value}. Pace yourself for the rest of the month.",
                        Severity = InsightSeverity.Warning,
                        Icon = "bi-speedometer2"
                    });
                }
            }

            // -------------------------------------------------------------
            // 6. Fixed-to-Discretionary Ratio Shift (Margin Compression)
            // -------------------------------------------------------------
            decimal currentIncome = incomeTransactions.Where(t => t.Date >= currentMonthStart).Sum(t => t.Amount);
            var priorIncomeMonths = incomeTransactions.Where(t => t.Date < currentMonthStart)
                .GroupBy(t => $"{t.Date.Year}-{t.Date.Month:D2}").Select(g => g.Sum(t => t.Amount)).ToList();

            decimal priorIncomeAvg = priorIncomeMonths.Any() ? priorIncomeMonths.Average() : 0;
            decimal priorDiscAvg = priorDiscTxns.GroupBy(t => $"{t.Date.Year}-{t.Date.Month:D2}").Select(g => g.Sum(t => t.Amount)).DefaultIfEmpty(0).Average();

            if (currentIncome > 0 && priorIncomeAvg > 0)
            {
                decimal currentMargin = currentTotalDisc / currentIncome;
                decimal priorMargin = priorDiscAvg / priorIncomeAvg;

                // If discretionary spending took up 10% more of net income this month than the historical average
                if (currentMargin > priorMargin + 0.10m)
                {
                    insights.Add(new FinancialInsight
                    {
                        Category = "Discretionary",
                        Title = "Cash Flow Margin Narrowing",
                        Message = $"Discretionary purchases consumed {Math.Round(currentMargin * 100)}% of your income this month (up from {Math.Round(priorMargin * 100)}%), reducing your free cash flow.",
                        Severity = InsightSeverity.Alert,
                        Icon = "bi-pie-chart"
                    });
                }
            }

            // -------------------------------------------------------------
            // 7. Merchant Concentration (The "Amazon/Target" Factor)
            // -------------------------------------------------------------
            foreach (var category in discretionaryCategories)
            {
                var catTxns = currentDiscTxns.Where(t => t.Category == category).ToList();
                decimal catTotal = catTxns.Sum(t => t.Amount);

                // Only evaluate categories with meaningful spend (>$50) to avoid noisy math
                if (catTotal > 50m)
                {
                    var topMerchant = catTxns.GroupBy(t => t.CleanedDescription)
                        .Select(g => new { Name = g.Key, Total = g.Sum(t => t.Amount) })
                        .OrderByDescending(x => x.Total)
                        .FirstOrDefault();

                    // If one merchant is taking up more than 65% of the entire category
                    if (topMerchant != null && (topMerchant.Total / catTotal) > 0.65m)
                    {
                        insights.Add(new FinancialInsight
                        {
                            Category = category,
                            Title = "High Vendor Concentration",
                            Message = $"{topMerchant.Name} accounted for {Math.Round((topMerchant.Total / catTotal) * 100)}% of your {category} spending ({topMerchant.Total:C} of {catTotal:C}).",
                            Severity = InsightSeverity.Info,
                            Icon = "bi-shop"
                        });
                    }
                }
            }

            // -------------------------------------------------------------
            // 8. Income Variance / Missing Deposit Pacing
            // -------------------------------------------------------------
            var now = DateTime.Now;
            // Calculate exactly what day of the month we should be pacing against
            int targetDay = (year == now.Year && month == now.Month) ? now.Day : DateTime.DaysInMonth(year, month);

            decimal currentMTDIncome = incomeTransactions
                .Where(t => t.Date >= currentMonthStart && t.Date.Day <= targetDay)
                .Sum(t => t.Amount);

            var priorMTDIncomes = incomeTransactions
                .Where(t => t.Date < currentMonthStart && t.Date.Day <= targetDay)
                .GroupBy(t => $"{t.Date.Year}-{t.Date.Month:D2}")
                .Select(g => g.Sum(t => t.Amount))
                .ToList();

            decimal priorMTDAvg = priorMTDIncomes.Any() ? priorMTDIncomes.Average() : 0;

            // If income is 15% or more behind the historical pace for this exact day of the month
            if (priorMTDAvg > 0 && currentMTDIncome < priorMTDAvg * 0.85m)
            {
                insights.Add(new FinancialInsight
                {
                    Category = "Income",
                    Title = "Income Pacing Behind",
                    Message = $"Your deposited income ({currentMTDIncome:C}) is pacing {Math.Round((1 - (currentMTDIncome / priorMTDAvg)) * 100)}% lower than your historical average for this point in the month.",
                    Severity = InsightSeverity.Warning,
                    Icon = "bi-cash-stack"
                });
            }

            // -------------------------------------------------------------
            // 9. Irregular / Annual Bill Shock
            // -------------------------------------------------------------
            // Look for individual expenses over $100
            var largeCurrentTxns = currentMonthTxns.Where(t => t.Amount >= 100m).ToList();
            foreach (var txn in largeCurrentTxns)
            {
                // Check if we've seen this merchant AT ALL in the previous 90 days
                bool existsInPrior = priorTxns.Any(t => t.CleanedDescription == txn.CleanedDescription);
                if (!existsInPrior)
                {
                    insights.Add(new FinancialInsight
                    {
                        Category = txn.Category ?? "Uncategorized",
                        Title = "Irregular Expense Hit",
                        Message = $"A {txn.Amount:C} charge from {txn.CleanedDescription} appeared. This merchant hasn't been seen in the last 90 days.",
                        Severity = InsightSeverity.Warning,
                        Icon = "bi-exclamation-triangle"
                    });
                }
            }

            // -------------------------------------------------------------
            // 10. Cash Flow Timing Squeeze
            // -------------------------------------------------------------
            var fixedBills = currentMonthTxns.Where(t => fixedCategories.Contains(t.Category)).ToList();
            decimal totalFixed = fixedBills.Sum(t => t.Amount);

            decimal fixedDays1to10 = fixedBills.Where(t => t.Date.Day <= 10).Sum(t => t.Amount);
            decimal incomeDays1to10 = incomeTransactions.Where(t => t.Date >= currentMonthStart && t.Date.Day <= 10).Sum(t => t.Amount);

            // Guess expected total income if we are currently mid-month
            decimal expectedTotalIncome = (year == now.Year && month == now.Month) && priorIncomeAvg > 0
                ? priorIncomeAvg
                : incomeTransactions.Where(t => t.Date >= currentMonthStart).Sum(t => t.Amount);

            if (totalFixed > 0 && expectedTotalIncome > 0)
            {
                // If >70% of bills hit early, but <40% of income has arrived
                if ((fixedDays1to10 / totalFixed) > 0.70m && (incomeDays1to10 / expectedTotalIncome) < 0.40m)
                {
                    insights.Add(new FinancialInsight
                    {
                        Category = "Income", // Routes to Income tab so you can verify paychecks
                        Title = "Liquidity Squeeze",
                        Message = $"{Math.Round((fixedDays1to10 / totalFixed) * 100)}% of your fixed bills hit before Day 10, but historically only {Math.Round((incomeDays1to10 / expectedTotalIncome) * 100)}% of your income arrives by then.",
                        Severity = InsightSeverity.Info,
                        Icon = "bi-calendar2-minus"
                    });
                }
            }
            // -------------------------------------------------------------
            // 11. Savings Rate Compression
            // -------------------------------------------------------------
            decimal currentNetCashFlow = currentMonthTxns.Where(t => t.Type == "Income").Sum(t => t.Amount) - currentMonthTxns.Where(t => t.Type == "Expense").Sum(t => t.Amount);
            decimal currentIncomeTotal = incomeTransactions.Where(t => t.Date >= currentMonthStart).Sum(t => t.Amount);

            if (currentIncomeTotal > 0)
            {
                decimal currentSavingsRate = currentNetCashFlow / currentIncomeTotal;

                var priorSavingsRates = new List<decimal>();
                var priorMonthKeys = priorTxns.Select(t => $"{t.Date.Year}-{t.Date.Month:D2}").Distinct();

                foreach (var mKey in priorMonthKeys)
                {
                    var mParts = mKey.Split('-');
                    int mYear = int.Parse(mParts[0]);
                    int mMonth = int.Parse(mParts[1]);

                    decimal mIncome = incomeTransactions.Where(t => t.Date.Year == mYear && t.Date.Month == mMonth).Sum(t => t.Amount);
                    decimal mExpense = priorTxns.Where(t => t.Date.Year == mYear && t.Date.Month == mMonth).Sum(t => t.Amount);

                    if (mIncome > 0)
                    {
                        priorSavingsRates.Add((mIncome - mExpense) / mIncome);
                    }
                }

                decimal priorSavingsRateAvg = priorSavingsRates.Any() ? priorSavingsRates.Average() : 0;

                if (priorSavingsRateAvg > 0 && currentSavingsRate < priorSavingsRateAvg - 0.15m)
                {
                    insights.Add(new FinancialInsight
                    {
                        Category = "Income",
                        Title = "Savings Rate Compression",
                        Message = $"Your net savings rate dropped to {Math.Round(currentSavingsRate * 100)}% this month compared to your {Math.Round(priorSavingsRateAvg * 100)}% historical average.",
                        Severity = InsightSeverity.Warning,
                        Icon = "bi-piggy-bank"
                    });
                }
            }

            // -------------------------------------------------------------
            // 12. Mid-Week vs Thursday/Friday Stress Spend Imbalance
            // -------------------------------------------------------------
            decimal earlyWeekDailyAvg = currentDiscTxns
                .Where(t => t.Date.DayOfWeek >= DayOfWeek.Monday && t.Date.DayOfWeek <= DayOfWeek.Wednesday)
                .Sum(t => t.Amount) / 3.0m;

            decimal lateWeekDailyAvg = currentDiscTxns
                .Where(t => t.Date.DayOfWeek == DayOfWeek.Thursday || t.Date.DayOfWeek == DayOfWeek.Friday)
                .Sum(t => t.Amount) / 2.0m;

            if (earlyWeekDailyAvg > 10m && lateWeekDailyAvg > earlyWeekDailyAvg * 2.2m)
            {
                insights.Add(new FinancialInsight
                {
                    Category = "Discretionary",
                    Title = "End-of-Week Spending Spike",
                    Message = $"Your average daily spend on Thursdays and Fridays ({lateWeekDailyAvg:C}/day) is over 2x higher than the beginning of the week.",
                    Severity = InsightSeverity.Info,
                    Icon = "bi-calendar-event"
                });
            }

            // -------------------------------------------------------------
            // 13. The "Dormant Subscription" Trap
            // -------------------------------------------------------------
            // Check recurring-style charges ($5 to $40) where no other related activity exists in that category
            var lowCostMonthlyCharges = currentMonthTxns
                .Where(t => t.Amount >= 5m && t.Amount <= 40m)
                .GroupBy(t => t.CleanedDescription)
                .Where(g => g.Count() == 1)
                .Select(g => g.First())
                .ToList();

            foreach (var charge in lowCostMonthlyCharges)
            {
                int categoryActivityCount = transactions.Count(t => t.Category == charge.Category && t.CleanedDescription != charge.CleanedDescription);
                if (categoryActivityCount == 0 && !fixedCategories.Contains(charge.Category))
                {
                    insights.Add(new FinancialInsight
                    {
                        Category = charge.Category ?? "Uncategorized",
                        Title = "Potentially Unused Subscription",
                        Message = $"{charge.CleanedDescription} charged {charge.Amount:C}, but zero other activity has been logged in {charge.Category} for 90 days. Check if this is still active.",
                        Severity = InsightSeverity.Warning,
                        Icon = "bi-question-circle",
                        PotentialSavings = charge.Amount
                    });
                }
            }

            // -------------------------------------------------------------
            // 14. Low Cash Buffer Warning
            // -------------------------------------------------------------
            // Track running cash balance over the month
            decimal runningBalanceBuffer = currentIncomeTotal;
            decimal lowestBuffer = runningBalanceBuffer;

            var chronologicalMonthTxns = currentMonthTxns.OrderBy(t => t.Date).ToList();
            foreach (var txn in chronologicalMonthTxns)
            {
                if (txn.Type == "Income") runningBalanceBuffer += txn.Amount;
                else runningBalanceBuffer -= txn.Amount;

                if (runningBalanceBuffer < lowestBuffer) lowestBuffer = runningBalanceBuffer;
            }

            if (currentIncomeTotal > 0 && lowestBuffer < (currentIncomeTotal * 0.10m) && lowestBuffer < 300m)
            {
                insights.Add(new FinancialInsight
                {
                    Category = "Income",
                    Title = "Low Cash Buffer Warning",
                    Message = $"Your cash flow cushion dipped to a low point of {lowestBuffer:C} during this month's cycle before incoming deposits cleared.",
                    Severity = InsightSeverity.Alert,
                    Icon = "bi-shield-exclamation"
                });
            }

            // -------------------------------------------------------------
            // 15. Service Fee & Surcharge Accumulation
            // -------------------------------------------------------------
            var feeKeywords = new[] { "fee", "surcharge", "tip", "delivery", "service charge" };
            var feeTxns = currentMonthTxns.Where(t => feeKeywords.Any(kw => t.OriginalDescription.Contains(kw, StringComparison.OrdinalIgnoreCase))).ToList();

            if (feeTxns.Count >= 3)
            {
                decimal totalFees = feeTxns.Sum(t => t.Amount);
                insights.Add(new FinancialInsight
                {
                    Category = "Discretionary",
                    Title = "Service Fee Accumulation",
                    Message = $"You accumulated {totalFees:C} in delivery, service, or surcharge fees across {feeTxns.Count} separate transactions this month.",
                    Severity = InsightSeverity.Info,
                    Icon = "bi-receipt",
                    PotentialSavings = totalFees
                });
            }

            return insights.OrderByDescending(i => i.Severity).ThenByDescending(i => i.PotentialSavings).ToList();
        }
    }
}