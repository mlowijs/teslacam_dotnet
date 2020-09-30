using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface INotificationService
    {
        Task NotifyAsync(string message);
    }
}