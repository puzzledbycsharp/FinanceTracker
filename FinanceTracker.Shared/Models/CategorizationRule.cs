namespace FinanceTracker.Shared.Models
{
    public class CategorizationRule
    {
        public int Id { get; set; }

        // The substring to look for, e.g., "WAL-MART" or "NETFLIX"
        public string MatchKeyword { get; set; }

        // The category to assign, e.g., "Groceries" or "Entertainment"
        public string Category { get; set; }
    }
}