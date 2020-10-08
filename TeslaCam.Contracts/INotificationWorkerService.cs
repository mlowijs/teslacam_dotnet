using System.Threading;
using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface INotificationWorkerService
    {
        Task ProcessNotificationsAsync(CancellationToken cancellationToken);
    }
}