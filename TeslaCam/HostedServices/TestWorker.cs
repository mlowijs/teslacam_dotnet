using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using TeslaCam.Contracts;

namespace TeslaCam.HostedServices
{
    public class TestWorker : BackgroundService
    {
        private readonly ITeslaApiService _apiService;

        public TestWorker(ITeslaApiService apiService)
        {
            _apiService = apiService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _apiService.EnableSentryMode();
        }
    }
}