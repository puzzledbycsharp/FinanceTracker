using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FinanceTracker.Shared.Models;

namespace FinanceTracker.Shared
{
    public static class DemoDataSeeder
    {
        public static List<Transaction> Generate(int months = 6)
        {
            var transactions = new List<Transaction>();
            var endDate = DateTime.Now;
            var startDate = endDate.AddMonths(-months);

            var f = new Faker();
            int checkNumber = 2084;
            int fridayCount = 0;

            // ==========================================
            // 1. FIXED BILLS, DUAL INCOME, AND TRANSFERS
            // (Ashburn, VA - Family of 5)
            // ==========================================
            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                // Friday Logic (Paychecks and Allowances)
                if (date.DayOfWeek == DayOfWeek.Friday)
                {
                    fridayCount++;

                    // INCOME 1: AI Architect at AWS (Bi-weekly)
                    if (fridayCount % 2 == 0)
                    {
                        transactions.Add(CreateTx(date.AddHours(2), 5850.00m, "Income", "AWS - PAYROLL DIR DEP", "Income", "M&T", "M&T Checking"));
                    }

                    // INCOME 2: Working Spouse at Inova Health System (Alternate Bi-weekly)
                    if (fridayCount % 2 != 0)
                    {
                        transactions.Add(CreateTx(date.AddHours(3), 2950.00m, "Income", "INOVA HEALTH DIR DEP", "Income", "M&T", "M&T Checking"));
                    }

                    // Expense: Zelle for Weekend Teen Babysitter (Every Friday)
                    transactions.Add(CreateTx(date.AddHours(18), 160.00m, "Expense", "ZELLE TO EMILY (BABYSITTING)", "Childcare", "M&T", "M&T Checking"));
                }

                // 1st of the Month: Housing & Childcare (Scaled for 3 Kids)
                if (date.Day == 1)
                {
                    transactions.Add(CreateTx(date.AddHours(1), 3850.00m, "Expense", "CAPITAL ONE MORTGAGE", "Housing", "M&T", "M&T Checking"));
                    transactions.Add(CreateTx(date.AddHours(8), 2950.00m, "Expense", "PRIMROSE SCHOOL ASHBURN", "Childcare", "M&T", "M&T Checking")); // Heavy daycare cost
                }

                // 15th of the Month: Virginia Utilities & Insurance
                if (date.Day == 15)
                {
                    transactions.Add(CreateTx(date.AddHours(3), 245.00m, "Expense", "DOMINION ENERGY", "Utilities", "M&T", "M&T Checking"));
                    transactions.Add(CreateTx(date.AddHours(4), 115.00m, "Expense", "LOUDOUN WATER", "Utilities", "M&T", "M&T Checking"));
                    transactions.Add(CreateTx(date.AddHours(5), 185.00m, "Expense", "STATE FARM AUTO INS", "Insurance", "M&T", "M&T Checking"));
                }

                // 25th of the Month: PHYSICAL CHECKS & SAVINGS TRANSFERS
                if (date.Day == 25)
                {
                    transactions.Add(CreateTx(date.AddHours(10), 1200.00m, "Transfer", "ONLINE TRANSFER TO JOINT SAVINGS", "Transfer", "M&T", "M&T Checking"));
                    transactions.Add(CreateTx(date.AddHours(10), 1200.00m, "Transfer", "ONLINE TRANSFER FROM CHECKING", "Transfer", "M&T", "Joint Savings"));
                    transactions.Add(CreateTx(date.AddHours(14), 120.00m, "Expense", $"CHECK #{checkNumber++}", "Services", "M&T", "M&T Checking")); // Piano lessons, house cleaner, etc.
                }

                // 28th of the Month: CREDIT CARD AUTO-PAY
                if (date.Day == 28)
                {
                    transactions.Add(CreateTx(date.AddHours(5), 2150.00m, "Transfer", "WEB XFER TO CHASE VISA", "Transfer", "M&T", "M&T Checking"));
                    transactions.Add(CreateTx(date.AddHours(5), 2150.00m, "Transfer", "PAYMENT THANK YOU - WEB", "Transfer", "Chase", "Chase Amazon Visa"));
                }

                // Monthly Subscriptions (AI Architect tools & Family entertainment)
                if (date.Day == 5) transactions.Add(CreateTx(date.AddHours(1), 20.00m, "Expense", "OPENAI SUBSCRIPTION", "AI & Software Tools", "Chase", "Chase Amazon Visa"));
                if (date.Day == 12) transactions.Add(CreateTx(date.AddHours(2), 15.99m, "Expense", "NETFLIX", "Subscriptions & Streaming", "Chase", "Chase Amazon Visa"));
                if (date.Day == 18) transactions.Add(CreateTx(date.AddHours(3), 24.99m, "Expense", "DISNEY PLUS FAMILY", "Subscriptions & Streaming", "Chase", "Chase Amazon Visa"));
                if (date.Day == 21) transactions.Add(CreateTx(date.AddHours(2), 45.00m, "Expense", "GITHUB COPILOT", "AI & Software Tools", "Chase", "Chase Amazon Visa"));
            }

            // ==========================================
            // 2. DAILY VARIABLE EXPENSES (Ashburn Geography)
            // ==========================================
            var dailyExpenseFaker = new Faker<Transaction>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Date, f => f.Date.Between(startDate, endDate).Date.AddHours(f.Random.Int(7, 21)).AddMinutes(f.Random.Int(0, 59)))
                .RuleFor(t => t.Type, "Expense")
                .RuleFor(t => t.Category, f => f.Random.WeightedRandom(
                    new[] { "Groceries", "Dining & Coffee", "Gas & Transit", "Shopping", "Kids & Health", "Cash" },
                    new[] { 0.25f, 0.30f, 0.15f, 0.20f, 0.05f, 0.05f }))
                .RuleFor(t => t.OriginalDescription, (f, t) => t.Category switch
                {
                    "Groceries" => f.PickRandom("WEGMANS ASHBURN", "HARRIS TEETER", "WHOLE FOODS MKT", "TRADER JOES", "TARGET"),
                    "Dining & Coffee" => f.PickRandom("STARBUCKS", "PEETS COFFEE", "FORDS FISH SHACK", "MATCHBOX ONE LOUDOUN", "SENSE OF THAI", "CHICK-FIL-A"),
                    "Gas & Transit" => f.PickRandom("WAWA", "SHELL", "EZ PASS VA TOLL", "WMATA SILVER LINE"),
                    "Shopping" => f.PickRandom("AMAZON.COM", "HOME DEPOT", "DULLES TOWN CENTER", "BEST BUY"),
                    "Kids & Health" => f.PickRandom("INOVA PEDIATRICS", "ASHBURN ICE HOUSE", "LOUDOUN SOCCER", "CVS PHARMACY"),
                    "Cash" => f.PickRandom("M&T ATM WITHDRAWAL"),
                    _ => f.Company.CompanyName()
                })
                .RuleFor(t => t.CleanedDescription, (f, t) => t.OriginalDescription)
                // Tuned amounts to reflect a family of 5 in Loudoun County
                .RuleFor(t => t.Amount, (f, t) => t.Category switch
                {
                    "Groceries" => Math.Round(f.Random.Decimal(65m, 320m), 2),
                    "Dining & Coffee" => Math.Round(f.Random.Decimal(8m, 110m), 2),
                    "Gas & Transit" => Math.Round(f.Random.Decimal(25m, 75m), 2),
                    "Shopping" => Math.Round(f.Random.Decimal(25m, 250m), 2),
                    "Kids & Health" => Math.Round(f.Random.Decimal(20m, 180m), 2),
                    "Cash" => Math.Round(f.Random.Decimal(20m, 80m), 2),
                    _ => Math.Round(f.Random.Decimal(15m, 85m), 2)
                })
                .RuleFor(t => t.Institution, (f, t) => t.Category == "Cash" ? "M&T" : "Chase")
                .RuleFor(t => t.Account, (f, t) => t.Category == "Cash" ? "M&T Checking" : "Chase Amazon Visa")
                .RuleFor(t => t.UniqueHash, f => Guid.NewGuid().ToString("N"))
                .RuleFor(t => t.Source, "Demo")
                .RuleFor(t => t.CreatedAt, f => DateTime.UtcNow);

            // Generates ~1.4 random transactions a day for a busier household
            int totalDays = (endDate - startDate).Days;
            transactions.AddRange(dailyExpenseFaker.Generate((int)(totalDays * 1.4)));

            return transactions.OrderBy(t => t.Date).ToList();
        }

        private static Transaction CreateTx(DateTime date, decimal amount, string type, string description, string category, string institution, string account)
        {
            return new Transaction
            {
                Id = Guid.NewGuid(),
                Date = date,
                Amount = amount,
                Type = type,
                OriginalDescription = description,
                CleanedDescription = description,
                Category = category,
                Institution = institution,
                Account = account,
                UniqueHash = Guid.NewGuid().ToString("N"),
                Source = "Demo",
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}