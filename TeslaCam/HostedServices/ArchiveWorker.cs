using System;
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

        private void ArchiveRecent()
        {
            if (_options.RootRequiresMounting)
                _fileSystemService.MountFileSystem();
                    
            var clips = _fileSystemService.GetClips(ClipType.Recent).ToArray();

            if (clips.Length == 0)
                return;
            
            var clipsToArchive = clips
                .Where(c => c.IsValid)
                .Where(c => _options.CamerasToProcess.Contains(c.Camera))
                .ToArray();
            
            _logger.LogInformation(
                $"Will archive {clipsToArchive.Length} clips");

            _fileSystemService.ArchiveClips(clipsToArchive);
            
            if (_options.RootRequiresMounting)
                _fileSystemService.UnmountFileSystem();
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _logger.LogInformation("Starting archive worker");

                while (!stoppingToken.IsCancellationRequested)
                {
                    
                    
                    _logger.LogInformation("Archiving complete");

                    await Task.Delay(TimeSpan.FromSeconds(ArchiveIntervalSeconds), stoppingToken);
                }

                _logger.LogInformation("Stopping archive worker");
            }, stoppingToken);
        }
    }
}