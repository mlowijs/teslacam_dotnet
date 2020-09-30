using Microsoft.Extensions.DependencyInjection;
using TeslaCam.Contracts;
using TeslaCam.Extensions;

namespace TeslaCam.Notifiers.Pushover
{
    public static class IServiceCollectionExtensions
    {
        public static void AddPushoverNotifier(this IServiceCollection services)
        {
            services.AddSingleton<INotifier, PushoverNotifier>();

            services.AddOptions<PushoverOptions>()
                .ConfigureSection();
        }
    }
}