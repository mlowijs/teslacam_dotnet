using System.Threading;
using System.Threading.Tasks;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface ITeslaCamService
    {
        void ArchiveClips(ClipType clipType, CancellationToken cancellationToken);
        Task UploadClipsAsync(CancellationToken cancellationToken);
        void CleanUsbDrive(CancellationToken cancellationToken);
    }
}