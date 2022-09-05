using Newtonsoft.Json;

namespace SammBotNET.RestDefinitions
{
    public class DuckImage
    {
        [JsonProperty("url")]
        public string ImageUrl { get; set; }

        [JsonProperty("Message")]
        public string Message { get; set; }
    }
}