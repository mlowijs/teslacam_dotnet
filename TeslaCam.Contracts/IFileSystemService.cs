using System.Collections.Generic;
using System.Threading;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IFileSystemService
    {
        IEnumerable<Clip> GetClips(ClipType clipType);
        void DeleteClips(IEnumerable<Clip> clips);
        void ArchiveClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);
        bool IsArchived(Clip clip);

        IEnumerable<Clip> GetArchivedClips();
    }
}