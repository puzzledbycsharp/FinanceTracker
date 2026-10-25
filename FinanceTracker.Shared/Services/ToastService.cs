using System;
using System.Threading.Tasks;

namespace FinanceTracker.Shared.Services
{
    public class ToastMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int Points { get; set; }
        public string Icon { get; set; } = "bi-trophy-fill";
    }

    public class ToastService
    {
        public event Action<ToastMessage>? OnShow;
        public event Action<Guid>? OnHide;

        // NEW: Background broadcast channel for the Gamification Engine
        public event Func<string, string, string, int, Task>? OnRewardEarned;

        public void ShowAchievement(string title, string message, int points, string icon = "bi-trophy-fill")
        {
            ShowMessage(title, message, icon, points);
        }

        public void ShowMessage(string title, string message, string icon, int points = 0)
        {
            var toast = new ToastMessage
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Icon = icon,
                Points = points
            };

            // 1. Show the UI toast
            OnShow?.Invoke(toast);

            // 2. If the toast has points, alert the background gamification listener
            if (points > 0)
            {
                OnRewardEarned?.Invoke(title, message, icon, points);
            }

            // 3. Auto-dismiss after 6 seconds
            Task.Run(async () =>
            {
                await Task.Delay(6000);
                OnHide?.Invoke(toast.Id);
            });
        }

        public void HideToast(Guid id)
        {
            OnHide?.Invoke(id);
        }
    }
}