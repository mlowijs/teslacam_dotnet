using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TeslaCam.HostedServices
{
    public class TestWorker : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}