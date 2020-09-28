using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface ITeslaApiService
    {
        Task EnableSentryModeAsync();
        Task DisableSentryModeAsync();
    }
}