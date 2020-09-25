using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TeslaCam.Contracts;

namespace TeslaCam.HostedServices
{
    public class TeslaCamWorker : BackgroundService
    {
        private readonly ITeslaCamService _teslaCamService;

        public TeslaCamWorker(ITeslaCamService teslaCamService)
        {
            _teslaCamService = teslaCamService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _teslaCamService.StartAsync(stoppingToken);
        }
    }
}