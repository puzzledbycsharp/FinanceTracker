using Bogus;
using FinanceTracker.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace FinanceTracker.DataGenerator
{
    public static class DemoDataSeeder
    {
        public static List<Shared.Models.Transaction> Generate(int months = 6)
        {
            var transactions = new List<Shared.Models.Transaction>();
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddMonths(-months);

            // Shared Faker instance
            var f = new Faker();

            // ==========================================
            // 1. FIXED & RECURRING BILLS (The Baseline)
            // ==========================================
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Income: Bi-weekly UMB Payroll (Fridays)
                if (date.DayOfWeek == DayOfWeek.Friday && (date.DayOfYear % 14 == 0))
                {
                    // Direct deposits usually hit right at midnight/early AM
                    transactions.Add(CreateTx(date.AddHours(2), 3450.00m, "Income", "UMB - PAYROLL DIR DEP", "Income"));
                }

                // 1st of the Month: Housing, Loans, and Childcare
                if (date.Day == 1)
                {
                    transactions.Add(CreateTx(date.AddHours(1), -2450.00m, "Expense", "SEVERN SAVINGS BANK MORTGAGE", "Utilities & Bills"));
                    transactions.Add(CreateTx(date.AddHours(8), -1850.00m, "Expense", "GODDARD SCHOOL TUITION", "Health & Fitness")); // Childcare
                    transactions.Add(CreateTx(date.AddHours(9), -350.00m, "Expense", "MOHELA STUDENT LOAN", "Utilities & Bills"));
                }

                // 15th of the Month: Utilities & Insurance
                if (date.Day == 15)
                {
                    transactions.Add(CreateTx(date.AddHours(3), -210.00m, "Expense", "BGE ACH PMT", "Utilities & Bills"));
                    transactions.Add(CreateTx(date.AddHours(4), -145.00m, "Expense", "GEICO AUTO INS", "Utilities & Bills"));
                    transactions.Add(CreateTx(date.AddHours(5), -85.00m, "Expense", "ANNEARUNDELCO WATER", "Utilities & Bills"));
                }

                // Monthly Subscriptions (Randomized days, but fixed amounts)
                if (date.Day == 5) transactions.Add(CreateTx(date.AddHours(1), -20.00m, "Expense", "OPENAI SUBSCRIPTION", "AI & Software Tools"));
                if (date.Day == 12) transactions.Add(CreateTx(date.AddHours(2), -22.00m, "Expense", "ELEVENLABS", "AI & Software Tools"));
                if (date.Day == 18) transactions.Add(CreateTx(date.AddHours(3), -15.99m, "Expense", "NETFLIX", "Subscriptions & Streaming"));
                if (date.Day == 22) transactions.Add(CreateTx(date.AddHours(2), -29.99m, "Expense", "BLACKMAGIC DESIGN", "AI & Software Tools"));
            }

            // ==========================================
            // 2. DAILY VARIABLE EXPENSES (The Human Element)
            // ==========================================
            var dailyExpenseFaker = new Faker<Shared.Models.Transaction>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                // Ensure times are realistic for a working family (7 AM to 9 PM)
                .RuleFor(t => t.Date, f => f.Date.Between(startDate, endDate).Date.AddHours(f.Random.Int(7, 21)).AddMinutes(f.Random.Int(0, 59)))
                .RuleFor(t => t.Type, "Expense")
                .RuleFor(t => t.Category, f => f.Random.WeightedRandom(
                    new[] { "Groceries", "Dining & Coffee", "Gas & Transit", "Shopping", "Health & Fitness" },
                    new[] { 0.35f, 0.25f, 0.15f, 0.20f, 0.05f })) // Weighted probabilities
                .RuleFor(t => t.OriginalDescription, (f, t) => t.Category switch
                {
                    // Regional grocery go-tos
                    "Groceries" => f.PickRandom("WEGMANS", "H MART", "LOTTE PLAZA MARKET", "GIANT FOOD", "TARGET", "COSTCO WHOLESALE"),
                    // Mix of fast food, coffee, and Maryland sit-down spots
                    "Dining & Coffee" => f.PickRandom("STARBUCKS", "CHICK-FIL-A", "CHIPOTLE", "IRON ROOSTER", "CAVA", "YOYA SUSHI"),
                    // Commuter transit
                    "Gas & Transit" => f.PickRandom("WAWA", "ROYAL FARMS", "EXXON", "EZ PASS MARYLAND"),
                    // E-commerce, hardware, tech, and skincare
                    "Shopping" => f.PickRandom("AMAZON.COM", "AMZN MKTP", "HOME DEPOT", "ARUNDEL MILLS", "MICRO CENTER", "BEAUTY OF JOSEON", "MISSHA", "SONY STORE"),
                    // Medical co-pays and gym equipment
                    "Health & Fitness" => f.PickRandom("UNIVERSITY OF MARYLAND PEDIATRIC", "CVS PHARMACY", "ROGUE FITNESS", "QUEST DIAGNOSTICS"),
                    _ => f.Company.CompanyName()
                })
                .RuleFor(t => t.CleanedDescription, (f, t) => t.OriginalDescription) // Assuming cleanup happens later in your pipeline
                .RuleFor(t => t.Amount, (f, t) => t.Category switch
                {
                    "Groceries" => Math.Round(f.Random.Decimal(-250m, -40m), 2),
                    "Dining & Coffee" => Math.Round(f.Random.Decimal(-85m, -6m), 2),
                    "Gas & Transit" => Math.Round(f.Random.Decimal(-65m, -25m), 2),
                    "Shopping" => Math.Round(f.Random.Decimal(-300m, -15m), 2),
                    "Health & Fitness" => Math.Round(f.Random.Decimal(-150m, -15m), 2),
                    _ => Math.Round(f.Random.Decimal(-100m, -10m), 2)
                })
                .RuleFor(t => t.Institution, "M&T")
                .RuleFor(t => t.Account, "M&T Checking")
                .RuleFor(t => t.UniqueHash, f => Guid.NewGuid().ToString("N"))
                .RuleFor(t => t.Source, "Demo")
                .RuleFor(t => t.CreatedAt, f => DateTime.UtcNow);

            // Generate roughly 2-3 random transactions per day
            int totalDays = (endDate - startDate).Days;
            transactions.AddRange(dailyExpenseFaker.Generate(totalDays * 3));

            // ==========================================
            // 3. SEASONAL & SPECIAL OCCASIONS
            // ==========================================

            // Summer Beach Trip (July)
            var julyDate = new DateTime(endDate.Year, 7, 15);
            if (julyDate >= startDate && julyDate <= endDate)
            {
                transactions.Add(CreateTx(julyDate.AddDays(1).AddHours(14), -850.00m, "Expense", "REHOBOTH BEACH HOTEL", "Shopping")); // Or Travel if you add it
                transactions.Add(CreateTx(julyDate.AddDays(2).AddHours(12), -125.50m, "Expense", "DOGFISH HEAD BREWPUB", "Dining & Coffee"));
                transactions.Add(CreateTx(julyDate.AddDays(3).AddHours(18), -85.20m, "Expense", "NICOLA PIZZA REHOBOTH", "Dining & Coffee"));
                transactions.Add(CreateTx(julyDate.AddDays(4).AddHours(9), -45.00m, "Expense", "WAWA", "Gas & Transit"));
            }

            // Holiday Shopping (December)
            var decemberDate = new DateTime(endDate.Year, 12, 10);
            if (decemberDate >= startDate && decemberDate <= endDate)
            {
                transactions.Add(CreateTx(decemberDate.AddHours(18), -450.00m, "Expense", "BEST BUY", "Shopping"));
                transactions.Add(CreateTx(decemberDate.AddHours(19), -210.00m, "Expense", "TARGET", "Shopping"));
                transactions.Add(CreateTx(decemberDate.AddHours(20), -180.00m, "Expense", "AMAZON.COM", "Shopping"));
            }

            // Return sorted chronologically so it perfectly mimics a real bank export
            return transactions.OrderBy(t => t.Date).ToList();
        }

        // Helper to quickly build fixed transactions
        private static Shared.Models.Transaction CreateTx(DateTime date, decimal amount, string type, string description, string category)
        {
            return new Shared.Models.Transaction
            {
                Id = Guid.NewGuid(),
                Date = date,
                Amount = amount,
                Type = type,
                OriginalDescription = description,
                CleanedDescription = description,
                Category = category,
                Institution = "M&T",
                Account = "M&T Checking",
                UniqueHash = Guid.NewGuid().ToString("N"),
                Source = "Demo",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}