using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Model;
using TeslaCam.Options;

namespace TeslaCam.HostedServices
{
    public class ArchiveWorker : BackgroundService
    {
        private const int ArchiveIntervalSeconds = 10;
        
        private readonly IFileSystemService _fileSystemService;
        private readonly TeslaCamOptions _options;
        private readonly ILogger<ArchiveWorker> _logger;

        public ArchiveWorker(IOptions<TeslaCamOptions> teslaCamOptions, IFileSystemService fileSystemService, ILogger<ArchiveWorker> logger)
        {
            _options = teslaCamOptions.Value;
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _logger.LogInformation("Starting archive worker");

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Starting archiving");
                    
                    ArchiveRecent();
                    
                    _logger.LogInformation("Archiving complete");

                    await Task.Delay(TimeSpan.FromSeconds(ArchiveIntervalSeconds), stoppingToken);
                }

                _logger.LogInformation("Stopping archive worker");
            }, stoppingToken);
        }
        
        private void ArchiveRecent()
        {
            if (!_options.ClipTypesToProcess.Contains(ClipType.Recent))
            {
                _logger.LogDebug("Not archiving Recent clips because they are not enabled");
                return;
            }
            
            _logger.LogInformation("Archiving Recent clips");

            var clips = _fileSystemService.GetClips(ClipType.Recent).ToArray();

            if (clips.Length == 0)
                return;
            
            var clipsToArchive = clips
                .Where(c => c.IsValid)
                .Where(c => _options.CamerasToProcess.Contains(c.Camera))
                .Where(c =>
                {
                    var fileInfo = new FileInfo(Path.Join(_options.ArchiveDirectory, c.File.Name));
                    
                    return !fileInfo.Exists || fileInfo.Length != c.File.Length;
                })
                .ToArray();
            
            _logger.LogInformation($"Will archive {clipsToArchive.Length} clips");

            _fileSystemService.ArchiveClips(clipsToArchive);
        }
    }
}