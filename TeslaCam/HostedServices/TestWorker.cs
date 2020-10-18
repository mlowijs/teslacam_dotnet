using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TeslaCam.Contracts;

namespace TeslaCam.HostedServices
{
    public class TestWorker : BackgroundService
    {
        private readonly INotificationService _notificationService;

        public TestWorker(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _notificationService.NotifyAsync("Clips Uploaded", "Uploaded 123 clips", stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}