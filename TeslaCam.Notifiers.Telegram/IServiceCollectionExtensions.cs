using Microsoft.Extensions.DependencyInjection;
using TeslaCam.Contracts;
using TeslaCam.Extensions;

namespace TeslaCam.Notifiers.Telegram
{
    public static class IServiceCollectionExtensions
    {
        public static void AddTelegramNotifier(this IServiceCollection services)
        {
            services.AddSingleton<INotifier, TelegramNotifier>();

            services.AddOptions<TelegramOptions>()
                .ConfigureSection();
        }
    }
}