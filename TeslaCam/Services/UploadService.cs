using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeslaCam.Contracts;

namespace TeslaCam.Services
{
    public class UploadService : IUploadService
    {
        private readonly IFileSystemService _fileSystemService;
        
        private readonly Dictionary<string, IUploader> _uploaders;
        
        public UploadService(IEnumerable<IUploader> uploaders, IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
            
            _uploaders = uploaders.ToDictionary(u => u.Name);
        }

        public Task UploadClipsAsync(CancellationToken cancellationToken)
        {
            var clipsToUpload = _fileSystemService.GetArchivedClips();

            return Task.CompletedTask;
        }
    }
}