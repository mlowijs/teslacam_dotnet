using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TeslaCam.Contracts;

namespace TeslaCam.HostedServices
{
    public class NotificationWorker : BackgroundService
    {
        private const int NotificationIntervalSeconds = 5;
        
        private readonly INotificationWorkerService _notificationWorkerService;

        public NotificationWorker(INotificationWorkerService notificationWorkerService)
        {
            _notificationWorkerService = notificationWorkerService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _notificationWorkerService.ProcessNotificationsAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(NotificationIntervalSeconds), stoppingToken);
            }
        }
    }
}