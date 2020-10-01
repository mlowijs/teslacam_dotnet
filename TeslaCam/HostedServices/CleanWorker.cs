using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Options;

namespace TeslaCam.HostedServices
{
    public class CleanWorker : BackgroundService
    {
        private readonly ILogger<CleanWorker> _logger;
        private readonly TeslaCamOptions _options;
        
        private DateTimeOffset _nextCleanTime;

        public CleanWorker(ILogger<CleanWorker> logger, IOptions<TeslaCamOptions> teslaCamOptions)
        {
            _logger = logger;
            _options = teslaCamOptions.Value;
            
            _nextCleanTime = DateTimeOffset.UtcNow + _options.CleanInterval;
        }
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                
                if (DateTimeOffset.UtcNow < _nextCleanTime)
                    continue;

                // remove module
                // mount FS
                // delete archived clips
                // unmount FS
                // probe module

                _nextCleanTime = DateTimeOffset.UtcNow + _options.CleanInterval;
            }
        }
    }
}