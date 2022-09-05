using Newtonsoft.Json;

namespace SammBotNET.RestDefinitions
{
    public class FoxImage
    {
        [JsonProperty("image")]
        public string ImageUrl { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}
