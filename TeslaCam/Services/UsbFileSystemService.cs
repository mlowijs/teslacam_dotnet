using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
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
    }
}