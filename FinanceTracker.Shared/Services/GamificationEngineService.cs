using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FinanceTracker.Shared.Data;
using FinanceTracker.Shared.Models;

namespace FinanceTracker.Shared.Services
{
    public class GamificationEngineService
    {
        private readonly FinanceDbContext _dbContext;
        private readonly ToastService _toastService;

        public GamificationEngineService(FinanceDbContext dbContext, ToastService toastService)
        {
            _dbContext = dbContext;
            _toastService = toastService;
        }

        public async Task EvaluateAchievementsAsync()
        {
            var profile = await GetOrCreateProfileAsync();
            var today = DateTime.Today;
            var yesterday = today.AddDays(-1);

            // 1. DAILY: "No Spend Day" (Evaluates yesterday)
            // Checks if there were zero discretionary expenses yesterday.
            await EvaluateNoSpendDayAsync(yesterday, profile);

            // 2. WEEKLY: "Frugal Week" (Evaluates the last 7 rolling days)
            // Checks if total discretionary spending for the last 7 days was under $100.
            await EvaluateFrugalWeekAsync(today, profile);

            await _dbContext.SaveChangesAsync();
        }

        private async Task EvaluateNoSpendDayAsync(DateTime targetDate, GamificationProfile profile)
        {
            string badgeId = $"NoSpend-{targetDate:yyyy-MM-dd}";

            // Don't award if we already gave this specific badge
            if (await HasAchievementAsync(badgeId)) return;

            var fixedCategories = await _dbContext.CategoryConfigs
                .Where(c => c.IsFixedBill)
                .Select(c => c.Name)
                .ToListAsync();

            // Did the user spend any discretionary money on this date?
            bool spentMoney = await _dbContext.Transactions
                .AnyAsync(t => t.Date.Date == targetDate.Date
                            && t.Type == "Expense"
                            && !fixedCategories.Contains(t.Category ?? ""));

            if (!spentMoney)
            {
                profile.CurrentNoSpendStreak++;
                if (profile.CurrentNoSpendStreak > profile.LongestNoSpendStreak)
                    profile.LongestNoSpendStreak = profile.CurrentNoSpendStreak;

                int points = 15;
                await AwardBadgeAsync(badgeId, "No Spend Day!", $"You kept your wallet closed on {targetDate:ddd, MMM d}.", points, "bi-wallet2", profile);

                // Bonus: 3-Day Streak!
                if (profile.CurrentNoSpendStreak % 3 == 0)
                {
                    await AwardBadgeAsync($"Streak3-{targetDate:yyyy-MM-dd}", "3-Day Streak!", "Three days in a row with zero discretionary spending!", 50, "bi-fire", profile);
                }
            }
            else
            {
                profile.CurrentNoSpendStreak = 0; // Reset streak
            }
        }

        private async Task EvaluateFrugalWeekAsync(DateTime today, GamificationProfile profile)
        {
            var oneWeekAgo = today.AddDays(-7);
            string badgeId = $"FrugalWeek-{today:yyyy-ww}"; // 'ww' gives the week number of the year

            if (await HasAchievementAsync(badgeId)) return;

            var fixedCategories = await _dbContext.CategoryConfigs.Where(c => c.IsFixedBill).Select(c => c.Name).ToListAsync();

            decimal weeklySpend = await _dbContext.Transactions
                .Where(t => t.Date.Date > oneWeekAgo && t.Date.Date <= today
                         && t.Type == "Expense"
                         && !fixedCategories.Contains(t.Category ?? ""))
                .SumAsync(t => t.Amount);

            // If discretionary spend is strictly under $150 for the 7-day sprint
            if (weeklySpend < 150m)
            {
                await AwardBadgeAsync(badgeId, "Weekly Budget Boss", $"You kept discretionary spending under $150 this week ({weeklySpend:C}).", 100, "bi-trophy", profile);
            }
        }

        // --- Core Helpers ---
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
            return await _dbContext.EarnedAchievements.AnyAsync(a => a.Title == titleId);
        }

        private async Task AwardBadgeAsync(string titleId, string displayTitle, string description, int points, string icon, GamificationProfile profile)
        {
            profile.TotalPoints += points;

            var achievement = new EarnedAchievement
            {
                Title = titleId, // Internal ID to prevent duplicates
                Description = displayTitle, // What we show to the user
                PointsAwarded = points,
                Icon = icon,
                DateEarned = DateTime.UtcNow
            };

            await _dbContext.EarnedAchievements.AddAsync(achievement);

            // Trigger the UI Toast
            _toastService.ShowAchievement(displayTitle, description, points, icon);
        }
    }
}