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
        private const int ArchiveIntervalSeconds = 15;
        
        private readonly ILogger<ArchiveWorker> _logger;
        private readonly ITeslaCamService _teslaCamService;

        public ArchiveWorker(ILogger<ArchiveWorker> logger, ITeslaCamService teslaCamService)
        {
            _logger = logger;
            _teslaCamService = teslaCamService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(ArchiveIntervalSeconds), stoppingToken);
                
                _logger.LogDebug("Starting archiving");
                    
                _teslaCamService.ArchiveRecentClips(stoppingToken);
                _teslaCamService.ArchiveEventClips(ClipType.Saved, stoppingToken);
                _teslaCamService.ArchiveEventClips(ClipType.Sentry, stoppingToken);
                    
                _logger.LogDebug("Finished archiving");
            }
        }
    }
}