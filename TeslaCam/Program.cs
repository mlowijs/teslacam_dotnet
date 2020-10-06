using System;
using System.Linq;
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
        private const string DefaultConfigurationFilePath = "/etc/teslacam.json";
        private const string RootUserName = "root";
        
        private static async Task Main(string[] args)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix && Environment.UserName != RootUserName)
            {
                Console.WriteLine("Must be run as root.");
                return;
            }
            
            var hostBuilder = GetHostBuilder(DefaultConfigurationFilePath, args.Contains("-q"))
                .Build();

            await hostBuilder.RunAsync();
        }

        private static IHostBuilder GetHostBuilder(string configurationFilePath, bool quiet)
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
                        .SetMinimumLevel(quiet ? LogLevel.Information : LogLevel.Debug)
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

                    services.AddSingleton<IArchiveService, ArchiveService>();
                    services.AddSingleton<IFileSystemService, FileSystemService>();
                    services.AddSingleton<IKernelService, KernelService>();
                    services.AddSingleton<INetworkService, NetworkService>();
                    services.AddSingleton<ITeslaApiService, TeslaApiService>();
                    services.AddSingleton<ITeslaCamService, TeslaCamService>();
                    services.AddSingleton<IUsbFileSystemService, UsbFileSystemService>();

                    services.AddSingleton<INotificationService, NotificationService>();
                    services.AddPushoverNotifier();
                    
                    services.AddAzureBlobStorageUploader();

                    services.AddHostedService<ArchiveWorker>();
                    services.AddHostedService<UploadWorker>();
                    services.AddHostedService<CleanWorker>();
                })
                .UseSystemd();
        }
    }
}