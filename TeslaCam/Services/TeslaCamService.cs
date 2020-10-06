using System;
using System.Collections.Generic;
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
        private const long MegabyteInBytes = 1 * 1024 * 1024;
        
        private readonly IFileSystemService _fileSystemService;
        private readonly TeslaCamOptions _options;
        private readonly ILogger<TeslaCamService> _logger;
        private readonly IKernelService _kernelService;
        private readonly IUsbFileSystemService _usbFileSystemService;
        private readonly INetworkService _networkService;
        private readonly INotificationService _notificationService;
        
        private readonly Dictionary<string, IUploader> _uploaders;

        public TeslaCamService(IOptions<TeslaCamOptions> teslaCamOptions, IFileSystemService fileSystemService,
            ILogger<TeslaCamService> logger, IKernelService kernelService, IUsbFileSystemService usbFileSystemService,
            IEnumerable<IUploader> uploaders, INetworkService networkService, INotificationService notificationService)
        {
            _options = teslaCamOptions.Value;

            _fileSystemService = fileSystemService;
            _logger = logger;
            _kernelService = kernelService;
            _usbFileSystemService = usbFileSystemService;
            _networkService = networkService;
            _notificationService = notificationService;

            _uploaders = uploaders.ToDictionary(u => u.Name);
        }

        public void ArchiveClips(ClipType clipType, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            
            if (!_options.ClipTypesToProcess.Contains(clipType))
            {
                _logger.LogDebug($"Not archiving {clipType} clips because they are not enabled");
                return;
            }
            
            _logger.LogDebug($"Archiving {clipType} clips");

            using var usbContext = _usbFileSystemService.AcquireContext();
            usbContext.Mount(false);

            var clips = (clipType == ClipType.Recent
                    ? GetRecentClips()
                    : GetEventClips(clipType, cancellationToken))
                .ToArray();
            
            if (clips.Length == 0)
            {
                _logger.LogDebug($"No new {clipType} clips to archive");
                return;
            }
            
            _logger.LogDebug($"Will archive {clips.Length} {clipType} clips");
            _fileSystemService.ArchiveClips(clips, cancellationToken);
        }

        private IEnumerable<Clip> GetRecentClips()
        {
            return _fileSystemService
                .GetClips(ClipType.Recent)
                .Where(IsClipValid)
                .Where(c => _options.CamerasToProcess.Contains(c.Camera))
                .Where(c => !_fileSystemService.IsArchived(c));
        }
        
        private IEnumerable<Clip> GetEventClips(ClipType clipType, CancellationToken cancellationToken)
        {
            var clips = _fileSystemService
                .GetClips(clipType)
                .ToArray();

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
                    .Where(IsClipValid)
                    .Where(c => _options.CamerasToProcess.Contains(c.Camera))
                    .Where(c => !_fileSystemService.IsArchived(c)));
            }

            _fileSystemService.TouchClips(clips.Except(clipsToArchive), cancellationToken);
            
            return clipsToArchive;
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
        
        public void CleanUsbDrive(CancellationToken cancellationToken)
        {
            using (var context = _usbFileSystemService.AcquireContext())
            {
                _kernelService.RemoveMassStorageGadgetModule();
                context.Mount(true);

                var archivedClips = _fileSystemService.GetClips(ClipType.Saved)
                    .Concat(_fileSystemService.GetClips(ClipType.Sentry))
                    .Where(c => _fileSystemService.IsArchived(c));

                _fileSystemService.DeleteClips(archivedClips, cancellationToken);

                var uploadedClips = _fileSystemService.GetUploadedClips();
                _fileSystemService.DeleteClips(uploadedClips, cancellationToken);
            }

            _kernelService.LoadMassStorageGadgetModule();
        }

        private static bool IsClipValid(Clip clip)
        {
            return clip.Date != DateTimeOffset.MinValue
                   && clip.Camera != Camera.Unknown
                   && clip.File.Length > MegabyteInBytes;
        }
    }
}