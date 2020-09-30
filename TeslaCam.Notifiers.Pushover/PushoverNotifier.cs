using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TeslaCam.Contracts;

namespace TeslaCam.Notifiers.Pushover
{
    public class PushoverNotifier : INotifier
    {
        public PushoverNotifier(IOptions<PushoverOptions> pushoverOptions)
        {
                
        }
        
        public string Name => "pushover";
        
        public Task NotifyAsync(string message)
        {
            throw new System.NotImplementedException();
        }
    }
}