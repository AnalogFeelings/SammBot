#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using System.Collections.Generic;
using Newtonsoft.Json;

namespace SammBot.Bot.Common.Rest.UrbanDictionary;

public class UrbanDefinition
{
    [JsonProperty("definition")]
    public string Definition { get; set; }
    [JsonProperty("permalink")]
    public string Permalink { get; set; }
    [JsonProperty("thumbs_up")]
    public int ThumbsUp { get; set; }
    [JsonProperty("thumbs_down")]
    public int ThumbsDown { get; set; }
    [JsonProperty("sound_urls")]
    public List<string> SoundUrls { get; set; }
    [JsonProperty("author")]
    public string Author { get; set; }
    [JsonProperty("word")]
    public string Word { get; set; }
    [JsonProperty("defid")]
    public long DefinitionId { get; set; }
    [JsonProperty("current_vote")]
    public string CurrentVote { get; set; }
    [JsonProperty("written_on")]
    public string WrittenOn { get; set; }
    [JsonProperty("example")]
    public string Example { get; set; }
}