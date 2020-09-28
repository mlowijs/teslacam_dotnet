using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TeslaCam.Contracts;

namespace TeslaCam.HostedServices
{
    public class UploadWorker : BackgroundService
    {
        private const int UploadIntervalSeconds = 30;

        private readonly IUploadService _uploadService;

        public UploadWorker(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _uploadService.UploadClipsAsync(stoppingToken);
                
                await Task.Delay(TimeSpan.FromSeconds(UploadIntervalSeconds), stoppingToken);
            }
        }
    }
}