using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class CloudForecast
{
    [JsonProperty("all")]
    public float Cloudiness { get; set; }
}