using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class NotificationService : INotificationService
    {
        private readonly TeslaCamOptions _options;
        
        private readonly IDictionary<string, INotifier> _notifiers;
        
        public NotificationService(IEnumerable<INotifier> notifiers, IOptions<TeslaCamOptions> teslaCamOptions)
        {
            _options = teslaCamOptions.Value;

            _notifiers = notifiers.ToDictionary(n => n.Name);
        }
        
        public Task NotifyAsync(string title, string message, CancellationToken cancellationToken)
        {
            if (!_notifiers.TryGetValue(_options.Notifier, out var notifier))
                return Task.CompletedTask;

            return notifier.NotifyAsync(title, message, cancellationToken);
        }
    }
}