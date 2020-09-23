using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TeslaCam.Contracts;
using TeslaCam.Model;

namespace TeslaCam.Services
{
    public class UploadService : IUploadService
    {
        private readonly IUploader _uploader;

        public UploadService(IUploader uploader)
        {
            _uploader = uploader;
        }

        public Task UploadClipsAsync(IEnumerable<Clip> clips, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}