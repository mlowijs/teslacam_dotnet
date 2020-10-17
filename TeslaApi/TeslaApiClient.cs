using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TeslaApi.Model;

namespace TeslaApi
{
    public class TeslaApiClient
    {
        private const string TeslaApiBaseAddress = "https://owner-api.teslamotors.com/";
        private const string OauthClientId = "81527cff06843c8634fdc09e8ac0abefb46ac849f38fe1e431c2ef2106796384";
        private const string OauthClientSecret = "c7257eb71a564034f9419ee651c7d0e5f7aa6bfbd18bafb5c5c033b093bb2fa3";
        
        private readonly HttpClient _httpClient;
        private readonly string? _email;
        private readonly string? _password;
        
        private string? _refreshToken;
        private DateTimeOffset _expiresAt;

        private TeslaApiClient()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(TeslaApiBaseAddress),
            };
        }
        
        public TeslaApiClient(string email, string password)
            : this()
        {
            _email = email;
            _password = password;
        }

        public static TeslaApiClient FromTokenInformation(TokenInformation tokenInformation)
        {
            var apiClient = new TeslaApiClient();
            apiClient.SetTokenInformation(tokenInformation);

            return apiClient;
        }

        public async Task<IEnumerable<TeslaVehicle>> ListVehiclesAsync(CancellationToken cancellationToken)
        {
            await AuthenticateAsync(cancellationToken);

            return await DoRequestAsync<IEnumerable<TeslaVehicle>>(HttpMethod.Get, "api/1/vehicles", null, cancellationToken);
        }
        
        public async Task<TeslaVehicle> WakeUpAsync(long vehicleId, CancellationToken cancellationToken)
        {
            await AuthenticateAsync(cancellationToken);

            return await DoRequestAsync<TeslaVehicle>(HttpMethod.Post, $"api/1/vehicles/{vehicleId}/wake_up", null, cancellationToken);
        }
        
        public async Task<bool> SetSentryModeAsync(long vehicleId, bool enabled, CancellationToken cancellationToken)
        {
            await AuthenticateAsync(cancellationToken);

            var response = await DoRequestAsync<CommandResponse<bool>>(
                HttpMethod.Post,
                $"api/1/vehicles/{vehicleId}/command/set_sentry_mode",
                new { on = enabled },
                cancellationToken);

            return response.Result;
        }
        
        private async Task AuthenticateAsync(CancellationToken cancellationToken)
        {
            if (_httpClient.DefaultRequestHeaders.Authorization != null && _expiresAt > DateTimeOffset.UtcNow)
                return;

            var tokenInformation = await RefreshAccessTokenAsync(cancellationToken);

            if (tokenInformation == null)
            {
                tokenInformation = await DoTokenRequestAsync(new Dictionary<string, string?>
                {
                    ["grant_type"] = "password",
                    ["client_id"] = OauthClientId,
                    ["client_secret"] = OauthClientSecret,
                    ["email"] = _email,
                    ["password"] = _password
                }, cancellationToken);

                if (tokenInformation == null)
                    throw new TeslaApiException($"Tesla API authentication failed");
            }
            
            SetTokenInformation(tokenInformation);
        }

        private async Task<TokenInformation?> RefreshAccessTokenAsync(CancellationToken cancellationToken)
        {
            if (_refreshToken == null)
                return null;

            return await DoTokenRequestAsync(new Dictionary<string, string?>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = _refreshToken
            }, cancellationToken);
        }

        private async Task<TokenInformation?> DoTokenRequestAsync(Dictionary<string, string?> formData, CancellationToken cancellationToken)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "oauth/token")
            {
                Content = new FormUrlEncodedContent(formData)
            };
            
            var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            return responseMessage.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<TokenInformation>(responseString)
                : null;
        }

        private async Task<TResponse> DoRequestAsync<TResponse>(HttpMethod method, string url, object? payload, CancellationToken cancellationToken)
            where TResponse : class
        {
            var requestMessage = new HttpRequestMessage(method, url);
            
            if (payload != null)
                requestMessage.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            
            var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
            var responseString = await responseMessage.Content.ReadAsStringAsync();

            if (!responseMessage.IsSuccessStatusCode)
            {
                // TODO
            }

            var apiResponse =
                JsonSerializer.Deserialize<TeslaApiResponse<TResponse>>(responseString);

            return apiResponse.Response;
        }
        
        private void SetTokenInformation(TokenInformation tokenInformation)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenInformation.AccessToken);
            
            _refreshToken = tokenInformation.RefreshToken;
            _expiresAt = DateTimeOffset
                .FromUnixTimeSeconds(tokenInformation.CreatedAt)
                .AddSeconds(tokenInformation.ExpiresIn);
        }
    }
}