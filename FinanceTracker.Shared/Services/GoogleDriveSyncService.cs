using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceTracker.Shared.Services
{
    public class GoogleDriveSyncService
    {
        private readonly IGoogleAuthService _authService;
        private readonly string _localDbPath;

        public GoogleDriveSyncService(IGoogleAuthService authService, string localDbPath)
        {
            _authService = authService;
            _localDbPath = localDbPath;
        }

        public async Task BackupToDriveAsync()
        {
            var driveService = await _authService.AuthenticateAsync();

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = "finance.db",
                Parents = new[] { "appDataFolder" } // Stores in the hidden app folder
            };

            using var stream = new FileStream(_localDbPath, FileMode.Open, FileAccess.Read);

            // Check if file already exists to update it, otherwise create new
            var existingFileId = await GetExistingBackupIdAsync(driveService);

            if (existingFileId != null)
            {
                var request = driveService.Files.Update(fileMetadata, existingFileId, stream, "application/x-sqlite3");
                await request.UploadAsync();
            }
            else
            {
                var request = driveService.Files.Create(fileMetadata, stream, "application/x-sqlite3");
                await request.UploadAsync();
            }
        }

        public async Task RestoreFromDriveAsync()
        {
            var driveService = await _authService.AuthenticateAsync();
            var existingFileId = await GetExistingBackupIdAsync(driveService);

            if (existingFileId != null)
            {
                using var stream = new FileStream(_localDbPath, FileMode.Create, FileAccess.Write);
                var request = driveService.Files.Get(existingFileId);
                await request.DownloadAsync(stream);
            }
        }

        private async Task<string> GetExistingBackupIdAsync(DriveService driveService)
        {
            var request = driveService.Files.List();
            request.Spaces = "appDataFolder";
            request.Fields = "files(id, name)";
            var result = await request.ExecuteAsync();

            return result.Files.FirstOrDefault(f => f.Name == "finance.db")?.Id;
        }
    }
}