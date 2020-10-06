using System.Threading;
using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface ITeslaApiService
    {
        Task EnableSentryModeAsync(CancellationToken cancellationToken);
        Task DisableSentryModeAsync(CancellationToken cancellationToken);
    }
}