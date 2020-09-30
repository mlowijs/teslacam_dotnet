using System.Threading;
using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface INotifier
    {
        string Name { get; }

        Task NotifyAsync(string title, string message, CancellationToken cancellationToken);
    }
}