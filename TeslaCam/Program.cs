using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Systemd;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using TeslaCam.Contracts;
using TeslaCam.Extensions;
using TeslaCam.HostedServices;
using TeslaCam.Notifiers.Pushover;
using TeslaCam.Notifiers.Telegram;
using TeslaCam.Options;
using TeslaCam.Services;
using TeslaCam.Uploaders.AzureBlobStorage;

namespace TeslaCam
{
    public class Program
    {
        private const string ConfigurationFilePath = "/etc/teslacam.json";
        private const string RootUserName = "root";
        
        private static async Task Main(string[] args)
        {
            // if (Environment.OSVersion.Platform == PlatformID.Unix && Environment.UserName != RootUserName)
            // {
            //     Console.WriteLine("Must be run as root.");
            //     return;
            // }

            var hostBuilder = GetHostBuilder(args.Contains("-q"))
                .Build();

            await hostBuilder.RunAsync();
        }

        private static IHostBuilder GetHostBuilder(bool quiet)
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile(ConfigurationFilePath, true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .SetMinimumLevel(quiet ? LogLevel.Information : LogLevel.Debug)
                        .AddConsole(options =>
                        {
                            options.Format = SystemdHelpers.IsSystemdService()
                                ? ConsoleLoggerFormat.Systemd
                                : ConsoleLoggerFormat.Default;
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

                    services.AddSingleton<NotificationService>();
                    services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<NotificationService>());
                    services.AddSingleton<INotificationWorkerService>(sp => sp.GetRequiredService<NotificationService>());
                    services.AddPushoverNotifier();
                    services.AddTelegramNotifier();
                    
                    services.AddAzureBlobStorageUploader();

                    // services.AddHostedService<ArchiveWorker>();
                    // services.AddHostedService<UploadWorker>();
                    // services.AddHostedService<CleanWorker>();
                    services.AddHostedService<NotificationWorker>();
                    services.AddHostedService<TestWorker>();
                })
                .UseSystemd();
        }
    }
}