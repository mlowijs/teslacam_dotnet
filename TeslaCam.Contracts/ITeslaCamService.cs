using System.Threading;
using System.Threading.Tasks;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface ITeslaCamService
    {
        Task ArchiveClipsAsync(ClipType clipType, CancellationToken cancellationToken);
        Task UploadClipsAsync(CancellationToken cancellationToken);
        void CleanUsbFileSystem(CancellationToken cancellationToken);
    }
}