using System.Text.Json.Serialization;
using TeslaApi.Converters;

namespace TeslaApi.Model
{
    public class TeslaVehicle
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("vin")]
        public string VehicleIdentificationNumber { get; set; }
        [JsonPropertyName("state")]
        [JsonConverter(typeof(VehicleStateConverter))]
        public bool IsOnline { get; set; }
    }
}