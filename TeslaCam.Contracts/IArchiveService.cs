using System.Collections.Generic;
using System.Threading;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IArchiveService
    {
        IEnumerable<Clip> GetClips();
        IEnumerable<Clip> GetUploadedClips();
        bool IsArchived(Clip clip);
        
        void ArchiveClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);
        void CreateClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);
        void TruncateClip(Clip clip);
    }
}