using FinanceTracker.Shared.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace FinanceTracker.Web.Services
{
    public class WebGoogleAuthService : IGoogleAuthService
    {
        private readonly IAccessTokenProvider _tokenProvider;

        public WebGoogleAuthService(IAccessTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public async Task<DriveService> AuthenticateAsync()
        {
            // 1. Request the token using Blazor's built-in OIDC provider
            var tokenResult = await _tokenProvider.RequestAccessToken(new AccessTokenRequestOptions
            {
                Scopes = new[] { "https://www.googleapis.com/auth/drive.appdata" }
            });

            if (tokenResult.TryGetToken(out var token))
            {
                // 2. Wrap the raw access token into a GoogleCredential object
                var credential = GoogleCredential.FromAccessToken(token.Value);

                // 3. Initialize and return the DriveService
                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "FinanceTracker Web"
                });
            }

            throw new System.Exception("Could not authenticate with Google. Login popup may have been blocked.");
        }
    }
}