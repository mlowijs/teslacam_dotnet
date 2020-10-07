using System.Collections.Generic;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IUsbFileSystemService
    {
        IUsbFileSystemContext AcquireContext();
        
        IEnumerable<Clip> GetClips(ClipType clipType);
    }
}