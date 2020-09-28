using System.Threading.Tasks;
using TeslaCam.Model;

namespace TeslaCam.Contracts
{
    public interface IUploader
    {
        string Name { get; }

        Task UploadClipAsync(Clip clip);
    }
}