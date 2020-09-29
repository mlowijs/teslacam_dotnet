using System.Threading;
using System.Threading.Tasks;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IUploader
    {
        string Name { get; }
        bool RequiresInternet { get; }

        Task<bool> UploadClipAsync(Clip clip, CancellationToken cancellationToken);
    }
}