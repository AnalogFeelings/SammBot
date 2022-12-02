using Newtonsoft.Json;

namespace SammBot.Bot.Rest.OpenWeather.Geolocation;

public class GeolocationLocation
{
    [JsonProperty("name")]
    public string Name { get; set; }
    [JsonProperty("lat")]
    public float Latitude { get; set; }
    [JsonProperty("lon")]
    public float Longitude { get; set; }
    [JsonProperty("country")]
    public string Country { get; set; }
    [JsonProperty("state")]
    public string State { get; set; }
}