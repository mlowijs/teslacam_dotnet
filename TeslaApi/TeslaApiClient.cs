using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TeslaApi.Converters;
using TeslaApi.Model;

namespace TeslaApi
{
    public class TeslaApiClient
    {
        private const string TeslaApiBaseAddress = "https://owner-api.teslamotors.com/";
        private const string OauthClientId = "81527cff06843c8634fdc09e8ac0abefb46ac849f38fe1e431c2ef2106796384";
        private const string OauthClientSecret = "c7257eb71a564034f9419ee651c7d0e5f7aa6bfbd18bafb5c5c033b093bb2fa3";
        
        private readonly HttpClient _httpClient;
        private readonly string _email;
        private readonly string _password;
        
        private string? _refreshToken;
        private DateTimeOffset _expiresAt;

        public TeslaApiClient(string email, string password)
        {
            _email = email;
            _password = password;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(TeslaApiBaseAddress),
            };
        }

        public TeslaApiClient(string token) : this(null, null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _expiresAt = DateTimeOffset.UtcNow.AddDays(1);
        }

        public async Task<IEnumerable<TeslaVehicle>> ListVehicles()
        {
            await Authenticate();

            return await DoRequestAsync<IEnumerable<TeslaVehicle>>(HttpMethod.Get, "api/1/vehicles");
        }
        
        public async Task<TeslaVehicle> WakeUp(long vehicleId)
        {
            await Authenticate();

            return await DoRequestAsync<TeslaVehicle>(HttpMethod.Post, $"api/1/vehicles/{vehicleId}/wake_up");
        }
        
        public async Task<bool> SetSentryMode(long vehicleId, bool enabled)
        {
            await Authenticate();

            var response = await DoRequestAsync<CommandResponse<bool>>(
                HttpMethod.Post,
                $"api/1/vehicles/{vehicleId}/command/set_sentry_mode",
                new { on = enabled });

            return response.Result;
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
                    ["email"] = _email,
                    ["password"] = _password
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

        private async Task<TResponse> DoRequestAsync<TResponse>(HttpMethod method, string url, object? payload = null)
            where TResponse : class
        {
            var requestMessage = new HttpRequestMessage(method, url);
            
            if (payload != null)
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8);
            
            var responseMessage = await _httpClient.SendAsync(requestMessage);
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            if (!responseMessage.IsSuccessStatusCode)
            {
                // do something
            }

            var apiResponse =
                JsonSerializer.Deserialize<TeslaApiResponse<TResponse>>(responseString);

            return apiResponse.Response;
        }
    }
}