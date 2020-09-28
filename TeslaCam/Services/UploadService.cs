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

        public UploadService(IEnumerable<IUploader> uploaders, IFileSystemService fileSystemService,
            IOptions<TeslaCamOptions> teslaCamOptions, ILogger<UploadService> logger)
        {
            _options = teslaCamOptions.Value;
            _fileSystemService = fileSystemService;
            _logger = logger;

            _uploaders = uploaders.ToDictionary(u => u.Name);
        }

        public async Task UploadClipsAsync(CancellationToken cancellationToken)
        {
            var clips = _fileSystemService
                .GetArchivedClips()
                .ToArray();

            var uploader = _uploaders[_options.Uploader];

            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                
                _logger.LogInformation($"Uploading clip '{clip.File.Name}' ({i + 1}/{clips.Length})");

                await uploader.UploadClipAsync(clip, cancellationToken);
            }
            
            _logger.LogInformation("Uploading archived clips");
        }
    }
}