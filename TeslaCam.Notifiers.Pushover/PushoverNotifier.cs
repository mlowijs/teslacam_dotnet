using System.Threading.Tasks;
using TeslaCam.Contracts;

namespace TeslaCam.Notifiers.Pushover
{
    public class PushoverNotifier : INotifier
    {
        public string Name => "pushover";
        
        public Task NotifyAsync(string message)
        {
            throw new System.NotImplementedException();
        }
    }
}