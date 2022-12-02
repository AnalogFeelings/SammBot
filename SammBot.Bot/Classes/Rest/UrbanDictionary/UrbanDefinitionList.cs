using System.Collections.Generic;
using Newtonsoft.Json;

namespace SammBot.Bot.Rest.UrbanDictionary;

public class UrbanDefinitionList
{
    [JsonProperty("list")]
    public List<UrbanDefinition> List { get; set; }
}