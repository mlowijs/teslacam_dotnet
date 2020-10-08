using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class NotificationService : INotificationService, INotificationWorkerService
    {
        private class Notification
        {
            public string Title { get; set; } = "";
            public string Message { get; set; } = "";

            public int Attempts { get; set; } = 1;
        }

        private const int MaxAttempts = 3;
        
        private readonly TeslaCamOptions _options;
        private readonly ILogger<NotificationService> _logger;
        
        private readonly IDictionary<string, INotifier> _notifiers;
        private readonly ConcurrentQueue<Notification> _notificationQueue;
        
        public NotificationService(IEnumerable<INotifier> notifiers, IOptions<TeslaCamOptions> teslaCamOptions, ILogger<NotificationService> logger)
        {
            _logger = logger;
            _options = teslaCamOptions.Value;

            _notifiers = notifiers.ToDictionary(n => n.Name);
            _notificationQueue = new ConcurrentQueue<Notification>();
        }
        
        public Task NotifyAsync(string title, string message, CancellationToken cancellationToken)
        {
            if (!_notifiers.TryGetValue(_options.Notifier, out var notifier))
                return Task.CompletedTask;

            _notificationQueue.Enqueue(new Notification
            {
                Title = title,
                Message = message
            });

            _logger.LogDebug("Notification queued");
            return Task.CompletedTask;
        }

        public async Task ProcessNotificationsAsync(CancellationToken cancellationToken)
        {
            if (!_notifiers.TryGetValue(_options.Notifier, out var notifier))
                return;

            while (_notificationQueue.TryDequeue(out var notification))
            {
                _logger.LogDebug("Processing notification");

                if (await notifier.NotifyAsync(notification.Title, notification.Message, cancellationToken))
                    continue;
                
                notification.Attempts++;
                    
                if (notification.Attempts < MaxAttempts)
                    _notificationQueue.Enqueue(notification);
            }
        }
    }
}