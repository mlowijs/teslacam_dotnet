using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TeslaApi;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class TeslaApiService : ITeslaApiService
    {
        private readonly TeslaApiOptions _options;
        private readonly TeslaApiClient _apiClient;

        public TeslaApiService(IOptions<TeslaApiOptions> teslaApiOptions)
        {
            _options = teslaApiOptions.Value;
            
            // _apiClient = new TeslaApiClient(_options.UserName, _options.Password);
            _apiClient = new TeslaApiClient("qts-1bdc45280fe8fcc95efefad0678de7e035eb0335983e150bc111b21d1126754b");
        }

        public async Task EnableSentryMode()
        {
            var vehicles = await _apiClient.ListVehicles();

            var vehicle = vehicles.SingleOrDefault(v => v.VehicleIdentificationNumber == _options.Vin);

            await _apiClient.WakeUp(vehicle.Id);
        }

        public Task DisableSentryMode()
        {
            throw new System.NotImplementedException();
        }
    }
}