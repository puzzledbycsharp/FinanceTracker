using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace FinanceTracker.Shared.Services
{
    public class GoogleDriveSyncService
    {
        private readonly HttpClient _httpClient;
        private readonly ToastService _toastService;
        private readonly IConfiguration _configuration;

        // HARDCODED TEMPORARY TOKEN 
        private string _hardcodedAccessToken = "YOUR_TEMPORARY_ACCESS_TOKEN";

        public GoogleDriveSyncService(HttpClient httpClient, ToastService toastService, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _toastService = toastService;
            _configuration = configuration;
        }

        public async Task BackupToDriveAsync()
        {
            try
            {
                // 1. Determine which mode we are in
                bool isDemoMode = _configuration["EnvironmentSettings:IsDemoMode"]?.Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;

                // Set the local filename based on your Program.cs configuration
                string localFileName = isDemoMode ? "demo.db" : "finance-test.db"; // Change "finance-test.db" to whatever your demo file is actually named!

                // Set the cloud filename so demo data doesn't overwrite real data in Google Drive
                string cloudFileName = isDemoMode ? "finance_demo_backup.db" : "finance.db";

                // 2. Read the active SQLite database
                if (!File.Exists(localFileName))
                {
                    _toastService.ShowMessage("Backup Failed", $"Database file '{localFileName}' not found.", "bi-exclamation-triangle", 0, false);
                    return;
                }

                byte[] dbBytes = await File.ReadAllBytesAsync(localFileName);

                // 3. Prepare the Google Drive multipart upload
                // We use multipart/related so we can send the file content AND the filename metadata in one request
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://www.googleapis.com/upload/drive/v3/files?uploadType=multipart");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _hardcodedAccessToken);

                var multipartContent = new MultipartContent("related");

                // Part A: Metadata (The filename)
                var metadataContent = new StringContent($"{{\"name\": \"{cloudFileName}\"}}", System.Text.Encoding.UTF8, "application/json");
                multipartContent.Add(metadataContent);

                // Part B: The actual SQLite File
                var fileContent = new ByteArrayContent(dbBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-sqlite3");
                multipartContent.Add(fileContent);

                request.Content = multipartContent;

                // 4. Execute the upload
                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _toastService.ShowMessage("Cloud Sync", $"Successfully backed up {cloudFileName} to Drive.", "bi-cloud-check", 0, false);
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    _toastService.ShowMessage("Sync Failed", $"Error: {response.StatusCode}", "bi-x-circle", 0, false);
                    Console.WriteLine($"Drive Upload Error: {error}");
                }
            }
            catch (Exception ex)
            {
                _toastService.ShowMessage("Sync Error", ex.Message, "bi-x-circle", 0, false);
                Console.WriteLine($"Exception during Drive sync: {ex.Message}");
            }
        }
    }
}