using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using TeslaCam.Contracts;
using TeslaCam.Extensions;
using TeslaCam.HostedServices;
using TeslaCam.Notifiers.Pushover;
using TeslaCam.Options;
using TeslaCam.Services;
using TeslaCam.Uploaders.AzureBlobStorage;

namespace TeslaCam
{
    public class Program
    {
        private const string ConfigurationFilePath = "/etc/teslacam.json";
        
        private static async Task Main(string[] args)
        {
            var configurationFilePath = args.Length > 0
                ? args[0]
                : ConfigurationFilePath;

            var hostBuilder = GetHostBuilder(configurationFilePath)
                .Build();

            await hostBuilder.RunAsync();
        }

        private static IHostBuilder GetHostBuilder(string configurationFilePath)
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile(configurationFilePath, false)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Debug)
                        .AddConsole(options =>
                        {
                            options.Format = ConsoleLoggerFormat.Systemd;
                        });
                })
                .ConfigureServices(services =>
                {
                    services.AddOptions<TeslaCamOptions>()
                        .ConfigureSection();
                    
                    services.AddOptions<TeslaApiOptions>()
                        .ConfigureSection();

                    services.AddSingleton<ITeslaCamService, TeslaCamService>();
                    services.AddSingleton<IFileSystemService, FileSystemService>();
                    services.AddSingleton<INetworkService, NetworkService>();
                    services.AddSingleton<IKernelService, KernelService>();
                    services.AddSingleton<ITeslaApiService, TeslaApiService>();
                    services.AddSingleton<IUsbService, UsbService>();
                    
                    services.AddSingleton<IUploadService, UploadService>();
                    services.AddAzureBlobStorageUploader();

                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddPushoverNotifier();

                    services.AddHostedService<ArchiveWorker>();
                    services.AddHostedService<UploadWorker>();
                    
                    // services.AddHostedService<CleanWorker>();
                    // services.AddHostedService<TestWorker>();
                })
                .UseSystemd();
        }
    }
}