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

                // Call ToastService normally; the background listener will automatically catch it and save it
                _toastService.ShowMessage("No Spend Day!", $"You kept your wallet closed on {targetDate:ddd, MMM d}.", "bi-wallet2", 15);

                if (profile.CurrentNoSpendStreak % 3 == 0)
                {
                    _toastService.ShowMessage("3-Day Streak!", "Three days in a row with zero discretionary spending!", "bi-fire", 50);
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
                _toastService.ShowMessage("Weekly Budget Boss", $"You kept discretionary spending under $150 this week ({weeklySpend:C}).", "bi-trophy", 100);
            }
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