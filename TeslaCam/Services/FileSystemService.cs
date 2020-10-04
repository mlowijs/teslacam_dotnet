using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private const string TeslaCamDateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
        
        private const string ArchiveDirectory = "archive";
        private const string ArchiveDateFormat = "yyyyMMddHHmmss";

        private static readonly Regex TeslaCamDateTimeCameraRegex =
            new Regex(@"^(\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})-(\w+)\.mp4$", RegexOptions.Compiled);

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
                _logger.LogDebug($"Clip directory '{clipDirectory}' not found");
                return Enumerable.Empty<Clip>();
            }

            if (clipType == ClipType.Recent)
            {
                return clipDirectory.EnumerateFiles()
                    .Select(fileInfo => CreateClip(fileInfo, clipType))
                    .ToArray();
            }

            return clipDirectory.EnumerateDirectories()
                .SelectMany(dirInfo => dirInfo.EnumerateFiles()
                    .Select(fileInfo => CreateClip(fileInfo, clipType, dirInfo.Name)))
                .ToArray();
        }

        public void ArchiveClips(IEnumerable<Clip> clips, CancellationToken cancellationToken)
        {
            var archiveDirectory = Path.Join(_options.DataDirectory, ArchiveDirectory);
            Directory.CreateDirectory(archiveDirectory);
            
            var clipsArray = clips.ToArray();

            for (var i = 0; i < clipsArray.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var clip = clipsArray[i];

                _logger.LogInformation($"Archiving clip '{clip.File.Name}' ({i + 1}/{clipsArray.Length})");

                clip.File.CopyTo(GetClipArchivePath(clip), true);
            }
        }
        
        public void DeleteClips(IEnumerable<Clip> clips, CancellationToken cancellationToken)
        {
            var clipsArray = clips.ToArray();

            for (var i = 0; i < clipsArray.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var clip = clipsArray[i];

                _logger.LogDebug($"Deleting clip '{clip.File.Name}' ({i + 1}/{clipsArray.Length})");
                
                clip.File.Delete();
            }
        }
        
        public void TruncateClip(Clip clip)
        {
            _logger.LogInformation($"Truncating clip '{clip.File.Name}'");
            clip.File.Open(FileMode.Truncate).Close();
        }

        public bool IsArchived(Clip clip)
        {
            var archiveClip = new FileInfo(GetClipArchivePath(clip));

            return archiveClip.Exists;
        }

        public IEnumerable<Clip> GetArchivedClips()
        {
            var archiveDirectory = new DirectoryInfo(Path.Join(_options.DataDirectory, ArchiveDirectory));

            return archiveDirectory
                .EnumerateFiles()
                .Where(fi => fi.Length != 0)
                .Select(CreateArchiveClip);
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

        private string GetClipArchivePath(Clip clip)
        {
            return Path.Join(_options.DataDirectory, ArchiveDirectory, GetArchiveFileName(clip));
        }
        
        private static Clip CreateClip(FileInfo fileInfo, ClipType clipType, string? eventDirectoryName = null)
        {
            var clip = new Clip
            {
                File = fileInfo,
                Type = clipType,
            };

            if (eventDirectoryName != null)
            {
                clip.EventDate = DateTimeOffset.ParseExact(eventDirectoryName, TeslaCamDateTimeFormat, null,
                    DateTimeStyles.AssumeUniversal);
            }
            
            var regexMatch = TeslaCamDateTimeCameraRegex.Match(fileInfo.Name);

            if (!regexMatch.Success)
                return clip;

            clip.Date = DateTimeOffset.ParseExact(regexMatch.Groups[1].Value, TeslaCamDateTimeFormat, null,
                DateTimeStyles.AssumeUniversal);

            clip.Camera = regexMatch.Groups[2].Value switch
            {
                "front" => Camera.Front,
                "left_repeater" => Camera.LeftRepeater,
                "right_repeater" => Camera.RightRepeater,
                "back" => Camera.Back,
                _ => Camera.Unknown
            };
            
            return clip;
        }

        private static Clip CreateArchiveClip(FileInfo fileInfo)
        {
            var fileNameParts = Path.GetFileNameWithoutExtension(fileInfo.Name).Split("_");

            var clip = new Clip
            {
                File = fileInfo,
                Date = DateTimeOffset.ParseExact(fileNameParts[0], ArchiveDateFormat, null,
                    DateTimeStyles.AssumeUniversal),
                
                Type = Enum.Parse<ClipType>(fileNameParts[1]),
                Camera = Enum.Parse<Camera>(fileNameParts[2]),
                
                EventDate = fileNameParts.Length == 4
                    ? DateTimeOffset.ParseExact(fileNameParts[3], ArchiveDateFormat, null,
                        DateTimeStyles.AssumeUniversal)
                    : default
            };

            return clip;
        }
        
        private static string GetArchiveFileName(Clip clip)
        {
            var baseName = $"{clip.Date.ToString(ArchiveDateFormat)}_{clip.Type:D}_{clip.Camera:D}";

            return clip.Type == ClipType.Recent
                ? $"{baseName}.mp4"
                : $"{baseName}_{clip.EventDate!.Value.ToString(ArchiveDateFormat)}.mp4";
        }
    }
}