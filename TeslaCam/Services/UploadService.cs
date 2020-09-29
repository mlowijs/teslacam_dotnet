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
        
        private readonly Dictionary<string, IUploader> _uploaders;
        private readonly INetworkService _networkService;

        public UploadService(IEnumerable<IUploader> uploaders, IFileSystemService fileSystemService,
            IOptions<TeslaCamOptions> teslaCamOptions, ILogger<UploadService> logger, INetworkService networkService)
        {
            _options = teslaCamOptions.Value;
            _fileSystemService = fileSystemService;
            _logger = logger;
            _networkService = networkService;

            _uploaders = uploaders.ToDictionary(u => u.Name);
        }

        public async Task UploadClipsAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            
            var clips = _fileSystemService
                .GetArchivedClips()
                .ToArray();

            var uploader = _uploaders[_options.Uploader];

            if (uploader.RequiresInternet && !await _networkService.IsConnectedToInternet())
            {
                _logger.LogInformation("No Internet connection, skipping upload");
                return;
            }

            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                
                _logger.LogInformation($"Uploading clip '{clip.File.Name}' ({i + 1}/{clips.Length})");

                if (await uploader.UploadClipAsync(clip, cancellationToken))
                    _fileSystemService.DeleteClip(clip);
            }
            
            _logger.LogInformation("Uploading archived clips");
        }
    }
}