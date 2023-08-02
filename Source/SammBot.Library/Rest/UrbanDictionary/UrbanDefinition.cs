#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
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

using System.Text.Json.Serialization;

namespace SammBot.Library.Rest.UrbanDictionary;

/// <summary>
/// A class that contains a word definition from the Urban Dictionary API.
/// </summary>
public class UrbanDefinition
{
    [JsonPropertyName("definition")]
    public string Definition { get; set; }
    [JsonPropertyName("permalink")]
    public string Permalink { get; set; }
    [JsonPropertyName("thumbs_up")]
    public int ThumbsUp { get; set; }
    [JsonPropertyName("thumbs_down")]
    public int ThumbsDown { get; set; }
    [JsonPropertyName("sound_urls")]
    public List<string> SoundUrls { get; set; }
    [JsonPropertyName("author")]
    public string Author { get; set; }
    [JsonPropertyName("word")]
    public string Word { get; set; }
    [JsonPropertyName("defid")]
    public long DefinitionId { get; set; }
    [JsonPropertyName("current_vote")]
    public string CurrentVote { get; set; }
    [JsonPropertyName("written_on")]
    public string WrittenOn { get; set; }
    [JsonPropertyName("example")]
    public string Example { get; set; }
}