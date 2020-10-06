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
        private readonly ITeslaCamService _teslaCamService;

        public UploadWorker(ILogger<UploadWorker> logger, ITeslaCamService teslaCamService)
        {
            _logger = logger;
            _teslaCamService = teslaCamService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(UploadIntervalSeconds), stoppingToken);

                    _logger.LogDebug("Starting uploading");

                    await _teslaCamService.UploadClipsAsync(stoppingToken);

                    _logger.LogDebug("Finished uploading");
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, $"Unhandled exception occurred: {exception.Message}");
                }
            }
        }
    }
}