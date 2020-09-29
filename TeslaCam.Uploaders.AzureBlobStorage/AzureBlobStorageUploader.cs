using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Model;

namespace TeslaCam.Uploaders.AzureBlobStorage
{
    public class AzureBlobStorageUploader : IUploader
    {
        private readonly AzureBlobStorageOptions _options;
        private readonly BlobContainerClient _blobContainerClient;
        
        public AzureBlobStorageUploader(IOptions<AzureBlobStorageOptions> azureBlobStorageOptions)
        {
            _options = azureBlobStorageOptions.Value;

            _blobContainerClient = new BlobContainerClient(_options.ConnectionString, _options.ContainerName);
        }
        
        public string Name => "azureBlobStorage";
        
        public async Task UploadClipAsync(Clip clip, CancellationToken cancellationToken)
        {
            var blobName = $"{clip.Date:yyyy/MM/dd}/{clip.File.Name}";
            
            await using var fileStream = clip.File.OpenRead();
            await _blobContainerClient.UploadBlobAsync(blobName, fileStream, cancellationToken);
        }
    }
}