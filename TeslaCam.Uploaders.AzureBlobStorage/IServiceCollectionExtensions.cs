using Microsoft.Extensions.DependencyInjection;
using TeslaCam.Contracts;
using TeslaCam.Extensions;

namespace TeslaCam.Uploaders.AzureBlobStorage
{
    public static class IServiceCollectionExtensions
    {
        public static void AddAzureBlobStorageUploader(this IServiceCollection services)
        {
            services.AddSingleton<IUploader, AzureBlobStorageUploader>();

            services.AddOptions<AzureBlobStorageOptions>()
                .ConfigureSection();
        }
    }
}