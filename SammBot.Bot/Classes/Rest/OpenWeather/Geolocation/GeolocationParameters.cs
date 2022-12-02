using SammBot.Bot.Attributes;

namespace SammBot.Bot.Rest.OpenWeather;

public class GeolocationParameters
{
    [UglyName("q")]
    public string Location { get; set; }
    [UglyName("appid")]
    public string AppId { get; set; }
    [UglyName("limit")]
    public int Limit { get; set; }
}