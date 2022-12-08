﻿using Newtonsoft.Json;

namespace SammBot.Bot.Rest;

public class FoxImage
{
    [JsonProperty("image")]
    public string ImageUrl { get; set; }

    [JsonProperty("link")]
    public string Link { get; set; }
}