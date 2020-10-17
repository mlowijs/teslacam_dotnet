using System.Text.Json.Serialization;

namespace TeslaApi.Model
{
    public class TokenInformation
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = "";
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = "";
        [JsonPropertyName("created_at")]
        public long CreatedAt { get; set; }
    }
}