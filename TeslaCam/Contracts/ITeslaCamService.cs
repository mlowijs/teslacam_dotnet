using System.Threading;
using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface ITeslaCamService
    {
        void ArchiveRecentClips();
    }
}