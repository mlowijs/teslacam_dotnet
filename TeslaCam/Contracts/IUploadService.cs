using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IUploadService
    {
        Task UploadClipsAsync(IEnumerable<Clip> clips, CancellationToken cancellationToken);
    }
}