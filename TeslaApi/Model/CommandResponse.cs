using System.Text.Json.Serialization;

namespace TeslaApi.Model
{
    public class CommandResponse<TResult>
    {
        [JsonPropertyName("reason")]
        public string Reason { get; set; }
        [JsonPropertyName("result")]
        public TResult Result { get; set; }
    }
}