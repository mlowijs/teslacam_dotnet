using System.Threading.Tasks;

namespace TeslaCam.Contracts
{
    public interface INotifier
    {
        string Name { get; }

        Task NotifyAsync(string message);
    }
}