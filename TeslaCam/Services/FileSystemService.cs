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
        private const string ArchiveDirectory = "Archive";
        
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

        public void DeleteClips(IEnumerable<Clip> clips)
        {
            var clipsArray = clips.ToArray();

            for (var i = 0; i < clipsArray.Length; i++)
            {
                var clip = clipsArray[i];
                
                _logger.LogInformation($"Deleting clip '{clip.File.Name}' ({i + 1}/{clipsArray.Length})");
                clip.File.Delete();
            }
        }

        public void ArchiveClips(IEnumerable<Clip> clips)
        {
            var clipsArray = clips.ToArray();
            
            for (var i = 0; i < clipsArray.Length; i++)
            {
                var clip = clipsArray[i];
                var archiveClipPath = Path.Join(_options.ArchiveDirectory, clip.File.Name);

                _logger.LogInformation($"Archiving clip '{clip.File.Name}' ({i + 1}/{clipsArray.Length})");
                
                var existingFile = new FileInfo(archiveClipPath);
                
                if (!existingFile.Exists || existingFile.Length != clip.File.Length)
                    clip.File.CopyTo(archiveClipPath, true);
            }
        }

        public void MountFileSystem(bool readWrite = false)
        {
            _logger.LogDebug($"Mounting '{_options.RootDirectory}'");

            var startInfo = new ProcessStartInfo("/usr/bin/mount");
            
            if (readWrite)
                startInfo.ArgumentList.Add("-orw");
            
            startInfo.ArgumentList.Add(_options.RootDirectory);

            Process.Start(startInfo).WaitForExit();
        }

        public void UnmountFileSystem()
        {
            _logger.LogDebug($"Unmounting '{_options.RootDirectory}'");

            Process.Start("/usr/bin/umount", _options.RootDirectory).WaitForExit();
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

            return Path.Join(_options.RootDirectory, TeslaCamDirectory, clipsDirectory);
        }
    }
}