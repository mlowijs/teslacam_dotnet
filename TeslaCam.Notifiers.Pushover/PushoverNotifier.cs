using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;

namespace TeslaCam.Notifiers.Pushover
{
    public class PushoverNotifier : INotifier
    {
        private const string PushoverBaseUrl = "https://api.pushover.net/";

        private readonly PushoverOptions _options;
        private readonly ILogger<PushoverNotifier> _logger;
        
        private readonly HttpClient _httpClient;
        
        public PushoverNotifier(IOptions<PushoverOptions> pushoverOptions, ILogger<PushoverNotifier> logger)
        {
            _logger = logger;
            _options = pushoverOptions.Value;
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(PushoverBaseUrl)
            };
        }
        
        public string Name => "pushover";
        
        public async Task NotifyAsync(string title, string message, CancellationToken cancellationToken)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = _options.ApiToken,
                ["user"] = _options.UserKey,
                ["title"] = title,
                ["message"] = message
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "1/messages.json")
            {
                Content = content
            };

            var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
            var responseString = await responseMessage.Content.ReadAsStringAsync();
            
            if (!responseMessage.IsSuccessStatusCode)
                _logger.LogError($"Error sending notification: {responseString}");
        }
    }
}