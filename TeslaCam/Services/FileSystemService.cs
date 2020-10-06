using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using TeslaCam.Contracts;
using TeslaCam.Model;

namespace TeslaCam.Services
{
    public class FileSystemService : IFileSystemService
    {
        private readonly ILogger<FileSystemService> _logger;

        public FileSystemService(ILogger<FileSystemService> logger)
        {
            _logger = logger;
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
    }
}