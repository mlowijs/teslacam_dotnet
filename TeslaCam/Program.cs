using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using TeslaCam.Contracts;
using TeslaCam.Extensions;
using TeslaCam.HostedServices;
using TeslaCam.Notifiers;
using TeslaCam.Options;
using TeslaCam.Services;
using TeslaCam.Uploaders;

namespace TeslaCam
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var hostBuilder = GetHostBuilder()
                .Build();
            
            await hostBuilder.RunAsync();
        }

        private static IHostBuilder GetHostBuilder()
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile("appsettings.json", false);
                })
                .ConfigureLogging(builder =>
                {
                    builder.AddConsole(options =>
                    {
                        options.Format = ConsoleLoggerFormat.Systemd;
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddOptions<TeslaCamOptions>()
                        .ConfigureSection();

                    services.AddSingleton<ITeslaCamService, TeslaCamService>();
                    services.AddSingleton<IUploadService, UploadService>();
                    services.AddSingleton<IFileSystemService, FileSystemService>();
                    services.AddSingleton<IKernelService, KernelService>();

                    services.AddSingleton<IUploader, AzureBlobStorageUploader>();

                    services.AddSingleton<INotifier, PushoverNotifier>();

                    // services.AddHostedService<TeslaCamWorker>();
                    services.AddHostedService<ArchiveWorker>();
                    services.AddHostedService<CleanWorker>();
                })
                .UseSystemd();
        }
    }
}