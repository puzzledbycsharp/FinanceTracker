using Google.Apis.Drive.v3;
using System.Threading.Tasks;

namespace FinanceTracker.Shared.Services
{
    public interface IGoogleAuthService
    {
        // Handles the platform-specific OAuth popup and returns an authenticated Drive API service
        Task<DriveService> AuthenticateAsync();
    }
}