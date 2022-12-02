using Newtonsoft.Json;

namespace SammBot.Bot.Rest;

public class DuckImage
{
    [JsonProperty("url")]
    public string ImageUrl { get; set; }

    [JsonProperty("Message")]
    public string Message { get; set; }
}