using System.Collections.Generic;
using System.Threading;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IArchiveService
    {
        IEnumerable<Clip> GetArchivedClips();
        IEnumerable<Clip> GetUploadedClips();
        bool IsArchived(Clip clip);
        
        void ArchiveClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);
        void TouchClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);
        void TruncateClip(Clip clip);
    }
}