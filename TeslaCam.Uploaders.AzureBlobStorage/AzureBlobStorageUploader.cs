using System;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;
using TeslaCam.Model;

namespace TeslaCam.Uploaders.AzureBlobStorage
{
    public class AzureBlobStorageUploader : IUploader
    {
        private readonly BlobContainerClient _blobContainerClient;
        private readonly ILogger<AzureBlobStorageUploader> _logger;
        
        public AzureBlobStorageUploader(IOptions<AzureBlobStorageOptions> azureBlobStorageOptions, ILogger<AzureBlobStorageUploader> logger)
        {
            _logger = logger;
            var options = azureBlobStorageOptions.Value;

            _blobContainerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);
        }
        
        public string Name => "AzureBlobStorage";
        public bool RequiresInternet => true;

        public async Task<bool> UploadClipAsync(Clip clip, CancellationToken cancellationToken)
        {
            var blobClient = _blobContainerClient.GetBlobClient(clip.File.Name);

            if (await blobClient.ExistsAsync(cancellationToken))
                return true;
            
            await using var fileStream = clip.File.OpenRead();

            try
            {
                await blobClient.UploadAsync(fileStream, cancellationToken);
            }
            catch (RequestFailedException requestFailedException)
            {
                _logger.LogError(requestFailedException, "Uploading failed:");
                return false;
            }
            
            return true;
        }
    }
}