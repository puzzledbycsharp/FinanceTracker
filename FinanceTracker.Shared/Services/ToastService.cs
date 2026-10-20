using System;
using System.Collections.Generic;
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

        public void ShowAchievement(string title, string message, int points, string icon = "bi-trophy-fill")
        {
            var toast = new ToastMessage
            {
                Title = title,
                Message = message,
                Points = points,
                Icon = icon
            };

            OnShow?.Invoke(toast);

            // Auto-dismiss after 6 seconds
            Task.Run(async () =>
            {
                await Task.Delay(6000);
                OnHide?.Invoke(toast.Id);
            });
        }
    }
}