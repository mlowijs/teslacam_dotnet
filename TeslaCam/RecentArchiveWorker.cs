using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace TeslaCam
{
    public class RecentArchiveWorker : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new System.NotImplementedException();
        }
    }
}