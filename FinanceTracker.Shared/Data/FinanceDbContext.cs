using Microsoft.EntityFrameworkCore;
using FinanceTracker.Shared.Models;

namespace FinanceTracker.Shared.Data
{
    public class FinanceDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<CategorizationRule> CategorizationRules { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<CategoryConfig> CategoryConfigs { get; set; } // NEW TABLE

        public DbSet<GamificationProfile> GamificationProfiles { get; set; }
        public DbSet<EarnedAchievement> EarnedAchievements { get; set; }

        public DbSet<UserProfile> UserProfiles { get; set; }

        private readonly string _databasePath;

        public FinanceDbContext(string databasePath)
        {
            _databasePath = databasePath;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed User Profiles
            modelBuilder.Entity<UserProfile>().HasData(
                new UserProfile { Id = 1, Name = "Shared / Household", WeeklyBudgetLimit = 400m },
                new UserProfile { Id = 2, Name = "Zachary", WeeklyBudgetLimit = 150m },
                new UserProfile { Id = 3, Name = "Jadyn", WeeklyBudgetLimit = 150m }
            );

            // Seed default rules from the separate SeedData class
            modelBuilder.Entity<CategorizationRule>().HasData(SeedData.GetDefaultRules());

            modelBuilder.Entity<CategoryConfig>().HasData(
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Housing", IsFixedBill = true, HexColor = "#0d6efd" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Utilities & Bills", IsFixedBill = true, HexColor = "#6c757d" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Subscriptions & Streaming", IsFixedBill = true, HexColor = "#6610f2" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "AI & Software Tools", IsFixedBill = true, HexColor = "#d63384" },

                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Groceries", IsFixedBill = false, HexColor = "#198754" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Dining & Coffee", IsFixedBill = false, HexColor = "#fd7e14" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Gas & Transit", IsFixedBill = false, HexColor = "#ffc107" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Shopping", IsFixedBill = false, HexColor = "#0dcaf0" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Health & Fitness", IsFixedBill = false, HexColor = "#dc3545" },

                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Income", IsFixedBill = true, HexColor = "#20c997" },
                    new CategoryConfig { Id = Guid.NewGuid(), Name = "Transfer", IsFixedBill = true, HexColor = "#adb5bd" }
                );
        }
    }
}