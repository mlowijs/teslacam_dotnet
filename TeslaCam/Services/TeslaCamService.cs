using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Model;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class TeslaCamService : ITeslaCamService
    {
        private const int MegabyteInBytes = 1 * 1024 * 1024;
        
        private readonly IFileSystemService _fileSystemService;
        private readonly TeslaCamOptions _options;
        private readonly ILogger<TeslaCamService> _logger;

        public TeslaCamService(IOptions<TeslaCamOptions> teslaCamOptions, IFileSystemService fileSystemService, ILogger<TeslaCamService> logger)
        {
            _options = teslaCamOptions.Value;
            
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        public void ArchiveRecentClips(CancellationToken cancellationToken)
        {
            if (!_options.ClipTypesToProcess.Contains(ClipType.Recent))
            {
                _logger.LogInformation("Not archiving Recent clips because they are not enabled");
                return;
            }
            
            _logger.LogInformation("Archiving Recent clips");

            var clips = _fileSystemService
                .GetClips(ClipType.Recent)
                .Where(c => c.IsValid)
                .Where(c => _options.CamerasToProcess.Contains(c.Camera))
                .Where(c => !_fileSystemService.IsArchived(c))
                .ToArray();

            if (clips.Length == 0)
            {
                _logger.LogInformation("No new Recent clips to archive");
                return;
            }

            _logger.LogInformation($"Will archive {clips.Length} Recent clips");

            _fileSystemService.ArchiveClips(clips, cancellationToken);
        }

        public void ArchiveEventClips(ClipType clipType, CancellationToken cancellationToken)
        {
            if (!_options.ClipTypesToProcess.Contains(clipType))
            {
                _logger.LogInformation($"Not archiving {clipType} clips because they are not enabled");
                return;
            }
            
            _logger.LogInformation($"Archiving {clipType} clips");

            var clips = _fileSystemService
                .GetClips(clipType);

            var clipsToArchive = new List<Clip>();
            
            foreach (var eventClips in clips.GroupBy(c => c.EventDate))
            {
                // Group clips by minute and only take minutes we want to keep
                var clipsByMinute = eventClips
                    .GroupBy(c => c.Date)
                    .OrderByDescending(c => c.Key)
                    .Take(_options.KeepClipsPerEventAmount);

                // Filter clips in every minute
                clipsToArchive.AddRange(clipsByMinute
                    .SelectMany(cbm => cbm)
                    .Where(c => c.IsValid)
                    .Where(c => _options.CamerasToProcess.Contains(c.Camera))
                    .Where(c => !_fileSystemService.IsArchived(c)));
            }
            
            if (clipsToArchive.Count == 0)
            {
                _logger.LogInformation($"No new {clipType} clips to archive");
                return;
            }
            
            _logger.LogInformation($"Will archive {clipsToArchive.Count} {clipType} clips");
            
            _fileSystemService.ArchiveClips(clipsToArchive, cancellationToken);
        }
    }
}