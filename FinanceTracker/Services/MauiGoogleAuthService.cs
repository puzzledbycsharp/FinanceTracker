using FinanceTracker.Shared.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.Maui.Authentication;
using System;
using System.Threading.Tasks;

namespace FinanceTracker.Services
{
    public class MauiGoogleAuthService : IGoogleAuthService
    {
        // This Client ID comes from the Google Cloud Console (Android/iOS Client ID)
        private const string ClientId = "YOUR_MOBILE_CLIENT_ID.apps.googleusercontent.com";

        // This must match the custom scheme you configure in your AndroidManifest / iOS Info.plist
        private const string CallbackScheme = "financetracker";

        public async Task<DriveService> AuthenticateAsync()
        {
            try
            {
                // 1. Build the Google OAuth URL manually
                string authUrl = $"https://accounts.google.com/o/oauth2/v2/auth" +
                                 $"?client_id={ClientId}" +
                                 $"&response_type=token" +
                                 $"&redirect_uri={CallbackScheme}:/oauth2redirect" +
                                 $"&scope=https://www.googleapis.com/auth/drive.appdata";

                // 2. Launch the native system browser for secure login
                var authResult = await WebAuthenticator.Default.AuthenticateAsync(
                    new Uri(authUrl),
                    new Uri($"{CallbackScheme}:/oauth2redirect")
                );

                // 3. Retrieve the token from the redirect URL parameters
                string accessToken = authResult.AccessToken;

                // 4. Wrap the token and initialize the DriveService
                var credential = GoogleCredential.FromAccessToken(accessToken);

                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FinanceTracker Mobile"
                });
            }
            catch (TaskCanceledException)
            {
                throw new Exception("The user canceled the login prompt.");
            }
        }
    }
}