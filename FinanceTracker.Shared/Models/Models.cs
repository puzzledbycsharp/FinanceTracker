namespace FinanceTracker.Shared.Models
{
    public class Transaction
    {

        public Transaction()
        {
            // Default constructor
        }
        // Update the Id property to generate a GUID automatically
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime Date { get; set; }
        public string OriginalDescription { get; set; } = string.Empty;
        public string CleanedDescription { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Type { get; set; } = "Expense";
        public string Category { get; set; } = "Uncategorized";
        public string Institution { get; set; } = string.Empty;

        // ADD THIS NEW PROPERTY:
        public string Account { get; set; } = "Unassigned";

        public string UniqueHash { get; set; } = string.Empty;

        public bool IsSplit { get; set; } = false;
        public Guid? ParentTransactionId { get; set; }



        // NEW: Phase 2 Duplicate Detection
        public string Source { get; set; } = "Manual"; // "Manual" or "Import"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsConflict { get; set; } = false;
        public Guid? ConflictingTransactionId { get; set; } // Links the two records together

        // NEW: Multi-User Profile Link
        public int? UserProfileId { get; set; }
    }

    public class GamificationProfile
    {
        public int Id { get; set; } = 1; // Single row to track overall user stats
        public int TotalPoints { get; set; } = 0;
        public int CurrentNoSpendStreak { get; set; } = 0;
        public int LongestNoSpendStreak { get; set; } = 0;
    }

    public class EarnedAchievement
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PointsAwarded { get; set; }
        public DateTime DateEarned { get; set; } = DateTime.UtcNow;
        public string Icon { get; set; } = "bi-star-fill";
    }
}