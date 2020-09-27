using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
        private const string ArchiveDirectory = "archive";
        
        private readonly ILogger<FileSystemService> _logger;
        private readonly TeslaCamOptions _options;

        public FileSystemService(IOptions<TeslaCamOptions> teslaCamOptions, ILogger<FileSystemService> logger)
        {
            _logger = logger;
            _options = teslaCamOptions.Value;
        }

        public IEnumerable<Clip> GetClips(ClipType clipType)
        {
            if (_options.MountingRequired)
                MountFileSystem();
            
            var clipDirectory = new DirectoryInfo(GetDirectoryForClipType(clipType));
            
            IEnumerable<Clip> clips;
            
            if (!clipDirectory.Exists)
            {
                _logger.LogError($"Root directory '{_options.MountPoint}' not found");
                clips = Enumerable.Empty<Clip>();
            }
            else if (clipType == ClipType.Recent)
            {
                clips = clipDirectory.EnumerateFiles()
                    .Select(fileInfo => new Clip(fileInfo, clipType))
                    .ToArray();
            }
            else
            {
                clips = clipDirectory.EnumerateDirectories()
                    .SelectMany(dirInfo => dirInfo.EnumerateFiles()
                        .Select(fileInfo => new Clip(fileInfo, clipType, dirInfo.Name)))
                    .ToArray();
            }

            if (_options.MountingRequired)
                UnmountFileSystem();

            return clips;
        }

        public void DeleteClips(IEnumerable<Clip> clips)
        {
            var clipsArray = clips.ToArray();

            if (_options.MountingRequired)
                MountFileSystem(true);
            
            for (var i = 0; i < clipsArray.Length; i++)
            {
                var clip = clipsArray[i];
                
                _logger.LogInformation($"Deleting clip '{clip.File.Name}' ({i + 1}/{clipsArray.Length})");
                clip.File.Delete();
            }
            
            if (_options.MountingRequired)
                UnmountFileSystem();
        }

        public void ArchiveClips(IEnumerable<Clip> clips, CancellationToken cancellationToken)
        {
            var clipsArray = clips.ToArray();
            
            if (_options.MountingRequired)
                MountFileSystem();
            
            for (var i = 0; i < clipsArray.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var clip = clipsArray[i];

                _logger.LogInformation($"Archiving clip '{clip.File.Name}' ({i + 1}/{clipsArray.Length})");
                
                var archiveDirectory = GetClipArchiveDirectory(clip);
                Directory.CreateDirectory(archiveDirectory);

                clip.File.CopyTo(Path.Join(archiveDirectory, clip.File.Name), true);
            }
            
            if (_options.MountingRequired)
                UnmountFileSystem();
        }

        public bool IsArchived(Clip clip)
        {
            var archiveDirectory = GetClipArchiveDirectory(clip);
            var fileInfo = new FileInfo(Path.Join(archiveDirectory, clip.File.Name));
            
            return fileInfo.Exists && fileInfo.Length == clip.File.Length;
        }

        private void MountFileSystem(bool readWrite = false)
        {
            _logger.LogDebug($"Mounting '{_options.MountPoint}'");

            var startInfo = new ProcessStartInfo("/usr/bin/mount");
            
            if (readWrite)
                startInfo.ArgumentList.Add("-orw");
            
            startInfo.ArgumentList.Add(_options.MountPoint);

            Process.Start(startInfo).WaitForExit();
        }

        private void UnmountFileSystem()
        {
            _logger.LogDebug($"Unmounting '{_options.MountPoint}'");

            Process.Start("/usr/bin/umount", _options.MountPoint).WaitForExit();
        }

        private string GetDirectoryForClipType(ClipType clipType)
        {
            var clipsDirectory = clipType switch
            {
                ClipType.Recent => RecentClipsDirectory,
                ClipType.Saved => SavedClipsDirectory,
                ClipType.Sentry => SentryClipsDirectory,
                _ => null
            };

            return Path.Join(_options.MountPoint, TeslaCamDirectory, clipsDirectory);
        }

        private string? GetClipArchiveDirectory(Clip clip)
        {
            switch (clip.Type)
            {
                case ClipType.Recent:
                    return Path.Join(_options.DataDirectory, ArchiveDirectory);
                case ClipType.Saved:
                case ClipType.Sentry:
                    return Path.Join(_options.DataDirectory, ArchiveDirectory, clip.EventDate!.Value.ToString("s"));
                
                default:
                    return null;
            }
        }
    }
}