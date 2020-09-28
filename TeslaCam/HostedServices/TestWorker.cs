using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TeslaCam.Contracts;

namespace TeslaCam.HostedServices
{
    public class TestWorker : BackgroundService
    {
        private readonly IUploadService _uploadService;

        public TestWorker(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}