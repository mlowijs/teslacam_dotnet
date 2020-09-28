using System.Threading;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface ITeslaCamService
    {
        void ArchiveRecentClips(CancellationToken cancellationToken);
        void ArchiveEventClips(ClipType clipType, CancellationToken cancellationToken);
    }
}