using System;
using System.Collections.Immutable;
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

            _uploadService.UploadClipsAsync(clips, cancellationToken);
            
            _logger.LogInformation($"Finished processing {clipType} clips");
        }
    }
}