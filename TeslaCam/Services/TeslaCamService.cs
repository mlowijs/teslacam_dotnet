using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IUploadService _uploadService;
        private readonly TeslaCamOptions _options;
        private readonly ILogger<TeslaCamService> _logger;

        public TeslaCamService(IOptions<TeslaCamOptions> teslaCamOptions, IUploadService uploadService, IFileSystemService fileSystemService, ILogger<TeslaCamService> logger)
        {
            _options = teslaCamOptions.Value;
            
            _uploadService = uploadService;
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        public void ArchiveRecentClips()
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

            _fileSystemService.ArchiveClips(clips);
        }

        public void ArchiveEventClips(ClipType clipType)
        {
            if (!_options.ClipTypesToProcess.Contains(clipType))
            {
                _logger.LogInformation($"Not archiving {clipType} clips because they are not enabled");
                return;
            }
            
            _logger.LogInformation($"Archiving {clipType} clips");
            
            var clips = _fileSystemService
                .GetClips(clipType)
        }

        private void ProcessClipType(ClipType clipType, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing '{clipType}' clips");

            var clips = _fileSystemService.GetClips(clipType).ToArray();
            _logger.LogInformation($"Found {clips.Length} clips to process");
            
            // if (clipType == ClipType.Recent)
            // {
            //     var clipsToSkip = clips
            //         .Where(c => c.Date > DateTimeOffset.Now - TimeSpan.FromMinutes(2));
            //     
            //     var clipsToUpload = clips
            //         .Except(clipsToSkip)
            //         .Where(IsClipValid)
            //         .Where(c => _options.CamerasToProcess.Contains(c.Camera));
            //     
            //     var clipsToDelete = clips
            //         .Except(clipsToSkip)
            //         .Except(clipsToUpload);
            // }
            // else
            // {
            foreach (var eventClips in clips.GroupBy(c => c.EventDate))
            {
                // Group clips by minute and only take minutes we want to keep
                var clipsByMinute = eventClips
                    .GroupBy(c => c.Date)
                    .OrderByDescending(c => c.Key)
                    .Take(_options.KeepClipsPerEventAmount);

                var clipsToUpload = clipsByMinute
                    .SelectMany(cbm => cbm)
                    .Where(c => _options.CamerasToProcess.Contains(c.Camera));
                
                var clipsToDelete = eventClips.Except(clipsToUpload);
            }
            // }
            
            
            // _uploadService.UploadClipsAsync(clips, cancellationToken);
            
            _logger.LogInformation($"Finished processing {clipType} clips");
        }
    }
}