using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeslaApi;
using TeslaCam.Contracts;
using TeslaCam.Options;

namespace TeslaCam.Services
{
    public class TeslaApiService : ITeslaApiService
    {
        private readonly ILogger<TeslaApiService> _logger;
        private readonly TeslaApiOptions _options;
        private readonly TeslaApiClient _apiClient;

        private long _vehicleId;

        public TeslaApiService(IOptions<TeslaApiOptions> teslaApiOptions, ILogger<TeslaApiService> logger)
        {
            _logger = logger;
            _options = teslaApiOptions.Value;
            
            // _apiClient = new TeslaApiClient(_options.UserName, _options.Password);
            _apiClient = new TeslaApiClient("qts-1bdc45280fe8fcc95efefad0678de7e035eb0335983e150bc111b21d1126754b");
        }

        public async Task EnableSentryModeAsync()
        {
            await InitializeAsync();
            
            await _apiClient.WakeUp(_vehicleId);
            await _apiClient.SetSentryMode(_vehicleId, true);
        }

        public async Task DisableSentryModeAsync()
        {
            await InitializeAsync();
            
            await _apiClient.WakeUp(_vehicleId);
            await _apiClient.SetSentryMode(_vehicleId, false);
        }
        
        private async Task InitializeAsync()
        {
            if (_vehicleId != default)
                return;

            var vehicles = await _apiClient.ListVehiclesAsync();
            var vehicle = vehicles.SingleOrDefault(v => v.VehicleIdentificationNumber == _options.Vin);

            if (vehicle == null)
            {
                _logger.LogError("No vehicle found with the configured VIN in the configured Tesla account");
                return;
            }

            _vehicleId = vehicle.Id;
        }
    }
}