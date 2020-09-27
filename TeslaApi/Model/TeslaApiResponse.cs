using System.Text.Json.Serialization;

namespace TeslaApi.Model
{
    public class TeslaApiResponse<TResponse>
    {
        [JsonPropertyName("response")]
        public TResponse Response { get; set; }
        [JsonPropertyName("count")]
        public int? Count { get; set; }
    }
}