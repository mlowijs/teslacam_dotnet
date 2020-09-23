using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TeslaCam.Contracts;
using TeslaCam.Model;

namespace TeslaCam.Services
{
    public class UploadService : IUploadService
    {
        private readonly Dictionary<string, IUploader> _uploaders;

        public UploadService(IEnumerable<IUploader> uploaders)
        {
            _uploaders = uploaders.ToDictionary(u => u.Name);
        }

        public Task UploadClipsAsync(IEnumerable<Clip> clips, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}