using System.Threading;
using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface INotificationService
    {
        Task NotifyAsync(string title, string message, CancellationToken cancellationToken);
    }
}