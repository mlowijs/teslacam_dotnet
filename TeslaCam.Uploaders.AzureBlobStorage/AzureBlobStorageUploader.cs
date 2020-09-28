using System.Threading.Tasks;
using TeslaCam.Contracts;
using TeslaCam.Model;

namespace TeslaCam.Uploaders.AzureBlobStorage
{
    public class AzureBlobStorageUploader : IUploader
    {
        public string Name => "azureBlob";
        
        public Task UploadClipAsync(Clip clip)
        {
            throw new System.NotImplementedException();
        }
    }
}