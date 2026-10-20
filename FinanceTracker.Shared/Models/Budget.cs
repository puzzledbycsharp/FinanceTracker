using System;

namespace FinanceTracker.Shared.Models
{
    public class Budget
    {
        public Guid Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public decimal MonthlyLimit { get; set; }

        // NEW: Ties the budget to a specific month (e.g., "2026-08")
        public string Period { get; set; } = string.Empty;
    }
}