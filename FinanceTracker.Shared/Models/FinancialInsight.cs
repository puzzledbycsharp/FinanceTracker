using System;

namespace FinanceTracker.Shared.Models
{
    public enum InsightSeverity
    {
        Info,
        Warning,
        Alert
    }

    public class FinancialInsight
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public InsightSeverity Severity { get; set; } = InsightSeverity.Info;
        public string Icon { get; set; } = "bi-lightbulb";
        public decimal PotentialSavings { get; set; }
    }
}