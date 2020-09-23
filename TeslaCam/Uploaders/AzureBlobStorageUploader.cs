using TeslaCam.Contracts;

namespace TeslaCam.Uploaders
{
    public class AzureBlobStorageUploader : IUploader
    {
        public string Name => "blobStorage";
    }
}