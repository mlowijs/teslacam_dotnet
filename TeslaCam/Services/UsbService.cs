using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class UsbService : IUsbService
    {
        private readonly TeslaCamOptions _options;
        private readonly ILogger<UsbService> _logger;

        private readonly Mutex _mutex;

        public UsbService(IOptions<TeslaCamOptions> teslaCamOptions, ILogger<UsbService> logger)
        {
            _logger = logger;
            _options = teslaCamOptions.Value;
            
            _mutex = new Mutex();
        }
        
        public void ExecuteWithMountedFileSystem(Action codeToExecute, bool readWrite = false)
        {
            if (!_options.MountingRequired)
            {
                codeToExecute();
                return;
            }
            
            _mutex.WaitOne();

            try
            {
                Mount(readWrite);

                codeToExecute();
            }
            finally
            {
                Unmount();
                _mutex.ReleaseMutex();    
            }
        }

        private void Mount(bool readWrite)
        {
            _logger.LogDebug($"Mounting '{_options.MountPoint}'");

            var startInfo = new ProcessStartInfo("/usr/bin/mount");
            
            if (readWrite)
                startInfo.ArgumentList.Add("-orw");
            
            startInfo.ArgumentList.Add(_options.MountPoint);

            Process.Start(startInfo).WaitForExit();
        }

        private void Unmount()
        {
            _logger.LogDebug($"Unmounting '{_options.MountPoint}'");

            Process.Start("/usr/bin/umount", _options.MountPoint).WaitForExit();
        }
    }
}