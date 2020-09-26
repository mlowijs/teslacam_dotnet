using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
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

        public TeslaApiService(IOptions<TeslaApiOptions> teslaApiOptions)
        {
            _options = teslaApiOptions.Value;
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(TeslaApiBaseAddress),
            };
        }

        private async Task Authenticate()
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "oauth/token");

            var content = new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = OauthClientId,
                ["client_secret"] = OauthClientSecret,
                ["email"] = _options.UserName,
                ["password"] = _options.Password
            };
            
            requestMessage.Content = new FormUrlEncodedContent(content);

            var responseMessage = await _httpClient.SendAsync(requestMessage);
        }
        
        public Task EnableSentryMode()
        {
            throw new System.NotImplementedException();
        }

        public Task DisableSentryMode()
        {
            throw new System.NotImplementedException();
        }
    }
}