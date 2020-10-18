using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;

namespace TeslaCam.Notifiers.Telegram
{
    public class TelegramNotifier : INotifier
    {
        private const string TelegramBaseUrl = "https://api.telegram.org/bot{0}/";
        
        private readonly TelegramOptions _options;
        
        private readonly HttpClient _httpClient;
        
        public TelegramNotifier(IOptions<TelegramOptions> telegramOptions)
        {
            _options = telegramOptions.Value;
            
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(string.Format(TelegramBaseUrl, _options.BotToken))
            };
        }
        
        public string Name => "Telegram";
        
        public async Task<bool> NotifyAsync(string title, string message, CancellationToken cancellationToken)
        {
            var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["chat_id"] = _options.ChatId.ToString(),
                ["parse_mode"] = "MarkdownV2",
                ["text"] = $"*{title}*\n{message}"
            });

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "sendMessage")
            {
                Content = content
            };

            try
            {
                var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
                var responseString = await responseMessage.Content.ReadAsStringAsync();

                if (!responseMessage.IsSuccessStatusCode)
                {
                    // _logger.LogError($"Error sending notification: {responseString}");
                    return false;
                }
            }
            catch (TaskCanceledException httpRequestException)
            {
                // _logger.LogError(httpRequestException, "Error sending notification:");
                return false;
            }

            return true;
        }
    }
}