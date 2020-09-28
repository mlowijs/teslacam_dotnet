using TeslaCam.Contracts;

namespace TeslaCam.Uploaders.AzureBlobStorage
{
    public class AzureBlobStorageUploader : IUploader
    {
        public string Name => "azureBlob";
    }
}