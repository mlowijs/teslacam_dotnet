using System.Collections.Generic;
using System.Threading;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IFileSystemService
    {
        void DeleteClips(IEnumerable<Clip> clips, CancellationToken cancellationToken);
    }
}