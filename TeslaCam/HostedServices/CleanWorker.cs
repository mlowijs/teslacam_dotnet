using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.HostedServices
{
    public class CleanWorker : BackgroundService
    {
        private readonly ITeslaCamService _teslaCamService;
        private readonly ILogger<CleanWorker> _logger;
        private readonly TeslaCamOptions _options;
        
        private DateTimeOffset _nextCleanTime;

        public CleanWorker(ILogger<CleanWorker> logger, IOptions<TeslaCamOptions> teslaCamOptions, ITeslaCamService teslaCamService)
        {
            _logger = logger;
            _teslaCamService = teslaCamService;
            _options = teslaCamOptions.Value;

            _nextCleanTime = DateTimeOffset.UtcNow;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (DateTimeOffset.UtcNow >= _nextCleanTime)
                    {
                        _teslaCamService.CleanUsbFileSystem(stoppingToken);

                        _nextCleanTime = DateTimeOffset.UtcNow + _options.CleanInterval;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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