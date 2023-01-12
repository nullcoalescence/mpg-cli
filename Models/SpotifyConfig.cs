
using Newtonsoft.Json;

namespace mpg_cli.Models
{
    internal class SpotifyConfig
    {
        [JsonProperty("credentials_path")]
        public string? CredentialsPath { get; set; }

        [JsonProperty("client_id")]
        public string? ClientId { get; set; }

        [JsonProperty("callback_url")]
        public string? CallbackUrl { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }
    }
}
