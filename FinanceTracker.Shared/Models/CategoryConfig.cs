using System;

namespace FinanceTracker.Shared.Models
{
    public class CategoryConfig
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        // If true, this category is excluded from weekly gamification limits (e.g., Rent, Utilities)
        public bool IsFixedBill { get; set; }

        // Optional: Handy for coloring badges or charts later
        public string HexColor { get; set; } = "#6c757d";
    }
}