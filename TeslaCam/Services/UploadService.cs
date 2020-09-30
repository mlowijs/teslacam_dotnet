using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class UploadService : IUploadService
    {
        private readonly TeslaCamOptions _options;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger<UploadService> _logger;
        private readonly INetworkService _networkService;
        private readonly INotificationService _notificationService;
        
        private readonly Dictionary<string, IUploader> _uploaders;
        
        public UploadService(IEnumerable<IUploader> uploaders, IFileSystemService fileSystemService,
            IOptions<TeslaCamOptions> teslaCamOptions, ILogger<UploadService> logger, INetworkService networkService,
            INotificationService notificationService)
        {
            _options = teslaCamOptions.Value;
            _fileSystemService = fileSystemService;
            _logger = logger;
            _networkService = networkService;
            _notificationService = notificationService;

            _uploaders = uploaders.ToDictionary(u => u.Name);
        }

        public async Task UploadClipsAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!_uploaders.TryGetValue(_options.Uploader, out var uploader))
                return;
            
            if (uploader.RequiresInternet && !await _networkService.IsConnectedToInternet())
            {
                _logger.LogDebug("No Internet connection, skipping upload");
                return;
            }
            
            _logger.LogDebug("Uploading archived clips");
            
            var clips = _fileSystemService
                .GetArchivedClips()
                .ToArray();
            
            if (clips.Length == 0)
            {
                _logger.LogDebug("No archived clips to upload");
                return;
            }

            _logger.LogDebug($"Will upload {clips.Length} clips");
            
            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                
                _logger.LogInformation($"Uploading clip '{clip.File.Name}' ({i + 1}/{clips.Length})");

                if (await uploader.UploadClipAsync(clip, cancellationToken))
                    _fileSystemService.TruncateClip(clip);
            }

            await _notificationService.NotifyAsync("Clips uploaded", $"Uploaded {clips.Length} clips.", cancellationToken);
        }
    }
}