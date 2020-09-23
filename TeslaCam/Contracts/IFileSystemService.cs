using System.Collections.Generic;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IFileSystemService
    {
        IEnumerable<Clip> GetClips(ClipType clipType);
    }
}