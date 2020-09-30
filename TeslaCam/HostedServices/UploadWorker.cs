using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeslaCam.Contracts;

namespace TeslaCam.HostedServices
{
    public class UploadWorker : BackgroundService
    {
        private const int UploadIntervalSeconds = 10;

        private readonly ILogger<UploadWorker> _logger;
        private readonly IUploadService _uploadService;

        public UploadWorker(ILogger<UploadWorker> logger, IUploadService uploadService)
        {
            _logger = logger;
            _uploadService = uploadService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(UploadIntervalSeconds), stoppingToken);
                
                    _logger.LogDebug("Starting uploading");
                
                    await _uploadService.UploadClipsAsync(stoppingToken);
                
                    _logger.LogDebug("Finished uploading");
                }
                catch (Exception e)
                {
                    _logger.LogError($"Unhandled exception occurred: {e.Message}");
                }
            }
        }
    }
}