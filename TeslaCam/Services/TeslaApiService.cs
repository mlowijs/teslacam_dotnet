using System.Linq;
using System.Threading;
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
            
            _apiClient = new TeslaApiClient(_options.UserName, _options.Password);
        }

        public async Task EnableSentryModeAsync(CancellationToken cancellationToken)
        {
            await InitializeAsync(cancellationToken);

            if (!await WakeUpVehicle(cancellationToken))
                return;
            
            var succeeded = await _apiClient.SetSentryModeAsync(_vehicleId, true, cancellationToken);

            if (!succeeded)
            {
                _logger.LogWarning("Enabling Sentry Mode failed");
                return;
            }
            
            _logger.LogDebug($"Enabled Sentry Mode");
        }

        public async Task DisableSentryModeAsync(CancellationToken cancellationToken)
        {
            await InitializeAsync(cancellationToken);

            if (!await WakeUpVehicle(cancellationToken))
                return;
            
            var succeeded = await _apiClient.SetSentryModeAsync(_vehicleId, true, cancellationToken);

            if (!succeeded)
            {
                _logger.LogWarning("Disabling Sentry Mode failed");
                return;
            }
            
            _logger.LogDebug($"Disabled Sentry Mode");
        }
        
        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (_vehicleId != default)
                return;

            var vehicles = await _apiClient.ListVehiclesAsync(cancellationToken);
            var vehicle = vehicles.SingleOrDefault(v => v.VehicleIdentificationNumber == _options.Vin);

            if (vehicle == null)
            {
                _logger.LogError($"No vehicle found with VIN '{_options.Vin}' in the configured Tesla account");
                return;
            }

            _vehicleId = vehicle.Id;
            _logger.LogDebug($"Stored vehicle ID '{_vehicleId}'");
        }

        private async Task<bool> WakeUpVehicle(CancellationToken cancellationToken)
        {
            var vehicle = await _apiClient.WakeUpAsync(_vehicleId, cancellationToken);

            if (!vehicle.IsOnline)
            {
                _logger.LogWarning("Waking up vehicle failed");
                return false;
            }
            
            _logger.LogDebug($"Vehicle woken up");
            return true;
        }
    }
}