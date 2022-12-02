using SammBot.Bot.Attributes;

namespace SammBot.Bot.Rest.OpenWeather.Forecast;

public class WeatherParameters
{
    [UglyName("lat")]
    public float Latitude { get; set; }
    [UglyName("lon")]
    public float Longitude { get; set; }
    [UglyName("appid")]
    public string AppId { get; set; }
    [UglyName("units")]
    public string Units { get; set; }
}