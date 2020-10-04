using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class UsbService : IUsbService, IDisposable
    {
        private class UsbContext : IUsbContext
        {
            private readonly UsbService _usbService;
            private readonly TeslaCamOptions _options;
            private readonly ILogger<UsbContext> _logger;

            private bool _isDisposed = false;

            public UsbContext(UsbService usbService, TeslaCamOptions options, ILoggerFactory loggerFactory)
            {
                _usbService = usbService;
                _options = options;
                _logger = loggerFactory.CreateLogger<UsbContext>();
            }

            public void Dispose()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
                
                _isDisposed = true;
                
                _logger.LogDebug($"Unmounting '{_options.MountPoint}'");

                Process.Start("/usr/bin/umount", _options.MountPoint).WaitForExit();
                
                _usbService.ReleaseUsbContext();
            }

            public void Mount(bool readWrite)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
                
                _logger.LogDebug($"Mounting '{_options.MountPoint}'");

                var startInfo = new ProcessStartInfo("/usr/bin/mount");
            
                if (readWrite)
                    startInfo.ArgumentList.Add("-orw");
            
                startInfo.ArgumentList.Add(_options.MountPoint);

                Process.Start(startInfo).WaitForExit();
            }
        }

        private readonly TeslaCamOptions _options;
        private readonly ILoggerFactory _loggerFactory;

        private readonly Mutex _mutex;

        public UsbService(IOptions<TeslaCamOptions> teslaCamOptions, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _options = teslaCamOptions.Value;
            
            _mutex = new Mutex();
        }

        public void Dispose()
        {
            _mutex.Dispose();
        }

        public IUsbContext AcquireUsbContext()
        {
            _mutex.WaitOne();
            
            return new UsbContext(this, _options, _loggerFactory);
        }

        private void ReleaseUsbContext()
        {
            _mutex.ReleaseMutex();
        }
    }
}