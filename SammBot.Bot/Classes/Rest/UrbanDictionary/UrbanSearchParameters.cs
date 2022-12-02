using SammBot.Bot.Attributes;

namespace SammBot.Bot.Rest.UrbanDictionary;

public class UrbanSearchParameters
{
    [UglyName("term")]
    public string Term { get; set; }
}