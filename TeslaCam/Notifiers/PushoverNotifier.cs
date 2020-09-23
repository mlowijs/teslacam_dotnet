using TeslaCam.Contracts;

namespace TeslaCam.Notifiers
{
    public class PushoverNotifier : INotifier
    {
        public string Name => "pushover";
    }
}