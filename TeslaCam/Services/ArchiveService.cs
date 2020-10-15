using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class ArchiveService : IArchiveService
    {
        private const string ArchiveDirectory = "archive";
        private const string ArchiveDateFormat = "yyyyMMddHHmmss";

        private readonly TeslaCamOptions _options;
        private readonly ILogger<ArchiveService> _logger;

        public ArchiveService(IOptions<TeslaCamOptions> teslaCamOptions, ILogger<ArchiveService> logger)
        {
            _logger = logger;
            _options = teslaCamOptions.Value;
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

        public void CreateClips(IEnumerable<Clip> clips, CancellationToken cancellationToken)
        {
            var filesToCreate = clips
                .Select(c => new FileInfo(GetClipArchivePath(c)))
                .Where(fi => !fi.Exists)
                .ToArray();

            for (var i = 0; i < filesToCreate.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var fileToCreate = filesToCreate[i];

                _logger.LogDebug($"Creating clip '{fileToCreate.Name}' ({i + 1}/{filesToCreate.Length})");

                fileToCreate.Create().Close();
            }
        }

        public void TruncateClip(Clip clip)
        {
            _logger.LogDebug($"Truncating clip '{clip.File.Name}'");
            clip.File.Open(FileMode.Truncate).Close();
        }

        public bool IsArchived(Clip clip)
        {
            var archiveClip = new FileInfo(GetClipArchivePath(clip));

            return archiveClip.Exists;
        }

        public IEnumerable<Clip> GetClips()
        {
            var archiveDirectory = new DirectoryInfo(Path.Join(_options.DataDirectory, ArchiveDirectory));

            return archiveDirectory
                .EnumerateFiles()
                .Where(fi => fi.Length != 0)
                .Select(CreateClip);
        }

        public IEnumerable<Clip> GetUploadedClips()
        {
            var archiveDirectory = new DirectoryInfo(Path.Join(_options.DataDirectory, ArchiveDirectory));

            return archiveDirectory
                .EnumerateFiles()
                .Where(fi => fi.Length == 0)
                .Select(CreateClip);
        }

        private string GetClipArchivePath(Clip clip)
        {
            return Path.Join(_options.DataDirectory, ArchiveDirectory, GetArchiveFileName(clip));
        }

        private static string GetArchiveFileName(Clip clip)
        {
            var baseName = $"{clip.Date.UtcDateTime.ToString(ArchiveDateFormat)}_{clip.Type:D}_{clip.Camera:D}";

            return clip.Type != ClipType.Recent && clip.EventDate != null
                ? $"{baseName}_{clip.EventDate.Value.UtcDateTime.ToString(ArchiveDateFormat)}.mp4"
                : $"{baseName}.mp4";
        }

        private static Clip CreateClip(FileInfo fileInfo)
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
    }
}