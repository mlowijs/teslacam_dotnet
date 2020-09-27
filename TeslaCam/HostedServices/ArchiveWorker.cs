using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TeslaCam.Contracts;
using TeslaCam.Model;

namespace TeslaCam.HostedServices
{
    public class ArchiveWorker : BackgroundService
    {
        private const int ArchiveIntervalSeconds = 10;
        
        private readonly ILogger<ArchiveWorker> _logger;
        private readonly ITeslaCamService _teslaCamService;

        public ArchiveWorker(ILogger<ArchiveWorker> logger, ITeslaCamService teslaCamService)
        {
            _logger = logger;
            _teslaCamService = teslaCamService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _logger.LogInformation("Starting archive worker");

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Starting archiving");
                    
                    _teslaCamService.ArchiveRecentClips();
                    _teslaCamService.ArchiveEventClips(ClipType.Saved);
                    _teslaCamService.ArchiveEventClips(ClipType.Sentry);
                    
                    _logger.LogInformation("Archiving complete");

                    await Task.Delay(TimeSpan.FromSeconds(ArchiveIntervalSeconds), stoppingToken);
                }
            }, stoppingToken);
        }
    }
}