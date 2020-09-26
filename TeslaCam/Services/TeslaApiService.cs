using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Exceptions;
using TeslaCam.Model;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class TeslaApiService : ITeslaApiService
    {
        private const string TeslaApiBaseAddress = "https://owner-api.teslamotors.com/";
        private const string OauthClientId = "81527cff06843c8634fdc09e8ac0abefb46ac849f38fe1e431c2ef2106796384";
        private const string OauthClientSecret = "c7257eb71a564034f9419ee651c7d0e5f7aa6bfbd18bafb5c5c033b093bb2fa3";

        private readonly TeslaApiOptions _options;
        private readonly HttpClient _httpClient;

        private string? _refreshToken;
        private DateTimeOffset _expiresAt;

        public TeslaApiService(IOptions<TeslaApiOptions> teslaApiOptions)
        {
            _options = teslaApiOptions.Value;
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(TeslaApiBaseAddress),
            };
        }
        
        public async Task EnableSentryMode()
        {
            await WakeVehicle(123);

            await SetSentryMode(123, true);
        }

        public Task DisableSentryMode()
        {
            return SetSentryMode(123, false);
        }

        private async Task WakeVehicle(long vehicleId)
        {
            await Authenticate();
            
            var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, $"api/1/vehicles/{vehicleId}/wake_up");
            
            await _httpClient.SendAsync(requestMessage);
        }
        
        private async Task SetSentryMode(long vehicleId, bool enabled)
        {
            await Authenticate();
            
            var requestMessage =
                new HttpRequestMessage(HttpMethod.Post, $"api/1/vehicles/{vehicleId}/command/set_sentry_mode");

            await _httpClient.SendAsync(requestMessage);
        }

        private async Task Authenticate()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization != null && _expiresAt > DateTimeOffset.UtcNow)
                return;
            
            var tokenResponse = await RefreshAccessToken();

            if (tokenResponse == null)
            {
                tokenResponse = await DoTokenRequestAsync(new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = OauthClientId,
                    ["client_secret"] = OauthClientSecret,
                    ["email"] = _options.UserName,
                    ["password"] = _options.Password
                });

                if (tokenResponse == null)
                    throw new TeslaApiException($"Tesla API authentication failed");
            }
            
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenResponse.AccessToken);
            
            _refreshToken = tokenResponse.RefreshToken;
            _expiresAt = DateTimeOffset
                .FromUnixTimeSeconds(tokenResponse.CreatedAt)
                .AddSeconds(tokenResponse.ExpiresIn);
        }

        private async Task<TokenResponse?> RefreshAccessToken()
        {
            if (_refreshToken == null)
                return null;

            return await DoTokenRequestAsync(new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = _refreshToken
            });
        }

        private async Task<TokenResponse?> DoTokenRequestAsync(IDictionary<string, string> formData)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "oauth/token")
            {
                Content = new FormUrlEncodedContent(formData)
            };
            
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            return responseMessage.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<TokenResponse>(responseString)
                : null;
        }
    }
}