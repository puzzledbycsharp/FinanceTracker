namespace FinanceTracker.Shared.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // This allows each person to have a different target, e.g., $400 for combined, $150 for individual
        public decimal WeeklyBudgetLimit { get; set; } = 200m;
    }
}