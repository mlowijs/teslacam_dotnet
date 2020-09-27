using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface ITeslaCamService
    {
        void ArchiveRecentClips();
        void ArchiveEventClips(ClipType clipType);
    }
}