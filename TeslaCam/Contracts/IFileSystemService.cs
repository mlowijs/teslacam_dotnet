using System.Collections.Generic;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IFileSystemService
    {
        IEnumerable<Clip> GetClips(ClipType clipType);
        void DeleteClips(IEnumerable<Clip> clips);
        void ArchiveClips(IEnumerable<Clip> clips);
        bool IsArchived(Clip clip);
    }
}