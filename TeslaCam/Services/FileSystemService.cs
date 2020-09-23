using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Model;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class FileSystemService : IFileSystemService
    {
        private const string TeslaCamDirectory = "TeslaCam";
        private const string RecentClipsDirectory = "RecentClips";
        private const string SavedClipsDirectory = "SavedClips";
        private const string SentryClipsDirectory = "SentryClips";
        
        private readonly ILogger<FileSystemService> _logger;
        private readonly TeslaCamOptions _options;

        public FileSystemService(IOptions<TeslaCamOptions> teslaCamOptions, ILogger<FileSystemService> logger)
        {
            _logger = logger;
            _options = teslaCamOptions.Value;
        }

        public IEnumerable<Clip> GetClips(ClipType clipType)
        {
            var clipDirectory = new DirectoryInfo(GetDirectoryForClipType(clipType));

            if (!clipDirectory.Exists)
            {
                _logger.LogError($"Root directory '{_options.RootDirectory}' not found");
                return Enumerable.Empty<Clip>();
            }
                

            if (clipType == ClipType.Recent)
            {
                return clipDirectory.EnumerateFiles()
                    .Select(fileInfo => new Clip(fileInfo, clipType));
            }

            return clipDirectory.EnumerateDirectories()
                .SelectMany(dirInfo => dirInfo.EnumerateFiles()
                    .Select(fileInfo => new Clip(fileInfo, clipType, dirInfo.Name)));
        }

        public void MountUsbFileSystem()
        {
            _logger.LogDebug($"Mounting '{_options.RootDirectory}'");

            Process.Start("mount", _options.RootDirectory);
        }

        public void UnmountUsbFileSystem()
        {
            _logger.LogDebug($"Unmounting '{_options.RootDirectory}'");

            Process.Start("umount", _options.RootDirectory);
        }

        private string GetDirectoryForClipType(ClipType clipType)
        {
            var clipsDirectory = clipType switch
            {
                ClipType.Recent => RecentClipsDirectory,
                ClipType.Saved => SavedClipsDirectory,
                ClipType.Sentry => SentryClipsDirectory
            };

            return Path.Join(_options.RootDirectory, TeslaCamDirectory, clipsDirectory);
        }
    }
}