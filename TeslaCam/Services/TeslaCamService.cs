using System;
using System.Linq;
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
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("TeslaCam service started");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var clipType in _options.ProcessClipTypes)
                    ProcessClipType(clipType, cancellationToken);
                
                await Task.Delay(TimeSpan.FromSeconds(_options.UploadInterval), cancellationToken);
            }
            
            _logger.LogInformation("TeslaCam service stopped");
        }

        private void ProcessClipType(ClipType clipType, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Processing '{clipType}' clips");

            var clips = _fileSystemService.GetClips(clipType).ToArray();
            _logger.LogInformation($"Found {clips.Length} clips to process");
            
            if (clipType == ClipType.Recent)
            {
                var clipsToSkip = clips.Where(c => c.Date > DateTimeOffset.Now - TimeSpan.FromMinutes(2));
                var clipsToUpload = clips.Except(clipsToSkip).Where(IsClipValid);
                var clipsToDelete = clips.Except(clipsToSkip).Except(clipsToUpload);
            }
            else
            {
                var groupedClips = clips.GroupBy(c => c.EventDate);
                
                foreach (var eventClips in clips.GroupBy(c => c.EventDate))
                {
                    var clipsByMinute = eventClips.GroupBy(c => c.Date)
                        .OrderByDescending(c => c.Key)
                        .Take(_options.KeepClipsPerEventAmount);

                    var clipsToUpload = clipsByMinute.SelectMany(cbm => cbm);
                    var clipsToDelete = eventClips.Except(clipsToUpload);
                }    
            }
            
            
            // _uploadService.UploadClipsAsync(clips, cancellationToken);
            
            _logger.LogInformation($"Finished processing {clipType} clips");
        }

        private bool IsClipValid(Clip clip)
        {
            return clip.Date != DateTimeOffset.MinValue
                   && clip.Camera != Camera.Unknown
                   && clip.File.Length > MegabyteInBytes;
        }
    }
}