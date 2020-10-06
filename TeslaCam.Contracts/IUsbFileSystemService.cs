using System.Collections.Generic;
using System.Threading;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IUsbFileSystemService
    {
        IUsbFileSystemContext AcquireContext();
        
        IEnumerable<Clip> GetClips(ClipType clipType);
    }
}