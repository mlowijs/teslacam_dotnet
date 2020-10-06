using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class UsbFileSystemService : IUsbFileSystemService, IDisposable
    {
        private class UsbFileSystemContext : IUsbFileSystemContext
        {
            private readonly UsbFileSystemService _usbFileSystemService;

            public UsbFileSystemContext(UsbFileSystemService usbFileSystemService)
            {
                _usbFileSystemService = usbFileSystemService;
            }

            public void Dispose()
            {
                _usbFileSystemService.ReleaseContext();
            }

            public void Mount(bool readWrite)
            {
                _usbFileSystemService.MountUsbFileSystem(readWrite);
            }

            public void Unmount()
            {
                _usbFileSystemService.UnmountUsbFileSystem();
            }
        }
        
        private const string TeslaCamDirectory = "TeslaCam";
        private const string RecentClipsDirectory = "RecentClips";
        private const string SavedClipsDirectory = "SavedClips";
        private const string SentryClipsDirectory = "SentryClips";
        private const string TeslaCamDateTimeFormat = "yyyy-MM-dd_HH-mm-ss";
        private const string FileSearchPattern = "*.mp4";
        
        private static readonly Regex TeslaCamDateTimeCameraRegex =
            new Regex(@"^(\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})-(\w+)\.mp4$", RegexOptions.Compiled);

        private readonly TeslaCamOptions _options;
        private readonly ILogger<UsbFileSystemService> _logger;

        private readonly Mutex _mutex;

        private bool _isMounted;

        public UsbFileSystemService(IOptions<TeslaCamOptions> teslaCamOptions, ILogger<UsbFileSystemService> logger)
        {
            _logger = logger;
            _options = teslaCamOptions.Value;
            
            _mutex = new Mutex();
        }

        public void Dispose()
        {
            _mutex.Dispose();
        }

        public IUsbFileSystemContext AcquireContext()
        {
            _mutex.WaitOne();
            
            return new UsbFileSystemContext(this);
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
                return clipDirectory.EnumerateFiles(FileSearchPattern)
                    .Select(fileInfo => CreateClip(fileInfo, clipType))
                    .ToArray();
            }

            return clipDirectory.EnumerateDirectories()
                .SelectMany(dirInfo => dirInfo.EnumerateFiles(FileSearchPattern)
                    .Select(fileInfo => CreateClip(fileInfo, clipType, dirInfo.Name)))
                .ToArray();
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

        private void MountUsbFileSystem(bool readWrite)
        {
            if (_isMounted)
                return;

            _logger.LogDebug($"Mounting '{_options.MountPoint}'");

            var startInfo = new ProcessStartInfo("/usr/bin/mount");
            
            if (readWrite)
                startInfo.ArgumentList.Add("-orw");
            
            startInfo.ArgumentList.Add(_options.MountPoint);

            Process.Start(startInfo).WaitForExit();

            _isMounted = true;
        }

        private void UnmountUsbFileSystem()
        {
            if (!_isMounted)
                return;
            
            _logger.LogDebug($"Unmounting '{_options.MountPoint}'");

            Process.Start("/usr/bin/umount", _options.MountPoint).WaitForExit();
            
            _isMounted = false;
        }
        
        private void ReleaseContext()
        {
            UnmountUsbFileSystem();
            
            _mutex.ReleaseMutex();
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
    }
}