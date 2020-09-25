using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeslaCam.Options;

namespace TeslaCam.HostedServices
{
    public class CleanWorker : BackgroundService
    {
        private readonly TeslaCamOptions _options;
        
        private DateTimeOffset _lastCleanDate;

        public CleanWorker(IOptions<TeslaCamOptions> teslaCamOptions)
        {
            _options = teslaCamOptions.Value;
            _lastCleanDate = DateTimeOffset.UtcNow.Date;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var now = DateTimeOffset.UtcNow;

                    if (now.Date <= _lastCleanDate)
                        continue;

                    if (DateTimeOffset.UtcNow.TimeOfDay <= _options.CleanTime)
                        continue;

                    // do cleaning

                    _lastCleanDate = now.Date;
                    
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }, stoppingToken);
        }
    }
}