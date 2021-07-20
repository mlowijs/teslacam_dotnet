using System;
using System.Threading.Tasks;
using CommandLine;
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
using TeslaCam.Options;
using TeslaCam.Services;
using TeslaCam.Uploaders.AzureBlobStorage;

namespace TeslaCam
{
    public class Program
    {
        private const string DefaultConfigurationFilePath = "/etc/teslacam.json";
        private const string RootUserName = "root";
        
        private static Task Main(string[] args) =>
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsedAsync(cliOptions =>
                {
                    var hostBuilder = GetHostBuilder(cliOptions)
                        .Build();

                    if (cliOptions.SendTestNotification)
                    {
                        var notifier = hostBuilder.Services.GetRequiredService<INotifier>();
                        return notifier.NotifyAsync("TeslaCam Test", "Test notification from TeslaCam!", default);
                    }
                    
                    if (Environment.OSVersion.Platform == PlatformID.Unix && Environment.UserName != RootUserName)
                    {
                        Console.Error.WriteLine("Must be run as root.");
                        return Task.CompletedTask;
                    }

                    return hostBuilder.RunAsync();
                });

        private static IHostBuilder GetHostBuilder(CommandLineOptions cliOptions)
        {
            return new HostBuilder()
                .ConfigureAppConfiguration(builder =>
                {
                    builder.AddJsonFile(cliOptions.ConfigurationFilePath ?? DefaultConfigurationFilePath, true)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging(builder =>
                {
                    builder
                        .SetMinimumLevel(cliOptions.Quiet ? LogLevel.Information : LogLevel.Debug)
                        .AddConsole(options =>
                        {
                            options.FormatterName = SystemdHelpers.IsSystemdService()
                                ? ConsoleFormatterNames.Systemd
                                : ConsoleFormatterNames.Simple;
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
                    
                    services.AddAzureBlobStorageUploader();

                    services.AddHostedService<ArchiveWorker>();
                    services.AddHostedService<UploadWorker>();
                    services.AddHostedService<CleanWorker>();
                    services.AddHostedService<NotificationWorker>();
                })
                .UseSystemd();
        }
    }
}