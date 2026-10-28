using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Shared.Data;
using FinanceTracker.Shared.Models;

namespace FinanceTracker.Shared.Services
{
    public class GamificationEngineService : IDisposable
    {
        private readonly FinanceDbContext _dbContext;
        private readonly ToastService _toastService;

        public GamificationEngineService(FinanceDbContext dbContext, ToastService toastService)
        {
            _dbContext = dbContext;
            _toastService = toastService;

            // Subscribe to the global point events the moment this service spins up
            _toastService.OnRewardEarned += HandleRewardEarnedAsync;
        }

        // Silently catches any toast in the app that has points attached and saves it to SQLite
        private async Task HandleRewardEarnedAsync(string title, string description, string icon, int points)
        {
            var profile = await GetOrCreateProfileAsync();
            profile.TotalPoints += points;

            var achievement = new EarnedAchievement
            {
                Title = title,
                Description = description,
                PointsAwarded = points,
                Icon = icon,
                DateEarned = DateTime.Now
            };

            _dbContext.EarnedAchievements.Add(achievement);
            await _dbContext.SaveChangesAsync();
        }

        public async Task EvaluateAchievementsAsync()
        {
            var profile = await GetOrCreateProfileAsync();
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            await EvaluateNoSpendDayAsync(yesterday, profile);
            await EvaluateFrugalWeekAsync(today, profile);

            await _dbContext.SaveChangesAsync();
        }

        private async Task EvaluateNoSpendDayAsync(DateTime targetDate, GamificationProfile profile)
        {
            string badgeId = $"NoSpend-{targetDate:yyyy-MM-dd}";

            if (await HasAchievementAsync(badgeId)) return;

            var fixedCategories = await _dbContext.CategoryConfigs.Where(c => c.IsFixedBill).Select(c => c.Name).ToListAsync();
            bool spentMoney = await _dbContext.Transactions.AnyAsync(t => t.Date.Date == targetDate.Date && t.Type == "Expense" && !fixedCategories.Contains(t.Category ?? ""));

            if (!spentMoney)
            {
                profile.CurrentNoSpendStreak++;
                if (profile.CurrentNoSpendStreak > profile.LongestNoSpendStreak) profile.LongestNoSpendStreak = profile.CurrentNoSpendStreak;

                // Use the safe helper method
                await AwardBadgeAsync(badgeId, "No Spend Day!", $"You kept your wallet closed on {targetDate:ddd, MMM d}.", 15, "bi-wallet2", profile);

                if (profile.CurrentNoSpendStreak % 3 == 0)
                {
                    await AwardBadgeAsync($"Streak3-{targetDate:yyyy-MM-dd}", "3-Day Streak!", "Three days in a row with zero discretionary spending!", 50, "bi-fire", profile);
                }
            }
            else
            {
                profile.CurrentNoSpendStreak = 0;
            }
        }

        private async Task EvaluateFrugalWeekAsync(DateTime today, GamificationProfile profile)
        {
            var oneWeekAgo = today.AddDays(-7);
            string badgeId = $"FrugalWeek-{today:yyyy-ww}";

            if (await HasAchievementAsync(badgeId)) return;

            var fixedCategories = await _dbContext.CategoryConfigs.Where(c => c.IsFixedBill).Select(c => c.Name).ToListAsync();
            decimal weeklySpend = await _dbContext.Transactions.Where(t => t.Date.Date > oneWeekAgo && t.Date.Date <= today && t.Type == "Expense" && !fixedCategories.Contains(t.Category ?? "")).SumAsync(t => t.Amount);

            if (weeklySpend < 150m)
            {
                await AwardBadgeAsync(badgeId, "Weekly Budget Boss", $"You kept discretionary spending under $150 this week ({weeklySpend:C}).", 100, "bi-trophy", profile);
            }
        }

        // --- RESTORED HELPER ---
        private async Task AwardBadgeAsync(string badgeId, string displayTitle, string description, int points, string icon, GamificationProfile profile)
        {
            profile.TotalPoints += points;

            var achievement = new EarnedAchievement
            {
                Title = displayTitle,
                Description = badgeId, // We map the hidden ID here so HasAchievementAsync works correctly!
                PointsAwarded = points,
                Icon = icon,
                DateEarned = DateTime.Now
            };

            _dbContext.EarnedAchievements.Add(achievement);

            // Show the toast, but bypass the background event listener to prevent duplicate database saves
            _toastService.ShowMessage(displayTitle, description, icon, points, saveToLedger: false);
        }

        private async Task<GamificationProfile> GetOrCreateProfileAsync()
        {
            var profile = await _dbContext.GamificationProfiles.FirstOrDefaultAsync();
            if (profile == null)
            {
                profile = new GamificationProfile();
                _dbContext.GamificationProfiles.Add(profile);
                await _dbContext.SaveChangesAsync();
            }
            return profile;
        }

        private async Task<bool> HasAchievementAsync(string titleId)
        {
            return await _dbContext.EarnedAchievements.AnyAsync(a => a.Description == titleId);
        }

        public void Dispose()
        {
            // Clean up memory
            _toastService.OnRewardEarned -= HandleRewardEarnedAsync;
        }
    }
}