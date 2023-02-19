#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2023 AestheticalZ
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using Newtonsoft.Json;

namespace SammBot.Bot.Common.Rest.Rule34;

public class Rule34Post
{
    [JsonProperty("preview_url")]
    public string PreviewUrl { get; set; }
    [JsonProperty("sample_url")]
    public string SampleUrl { get; set; }
    [JsonProperty("file_url")]
    public string FileUrl { get; set; }
    [JsonProperty("image")]
    public string Image { get; set; }
    
    [JsonProperty("directory")]
    public int Directory { get; set; }
    [JsonProperty("hash")]
    public string Hash { get; set; }
    
    [JsonProperty("width")]
    public int Width { get; set; }
    [JsonProperty("height")]
    public int Height { get; set; }
    
    [JsonProperty("id")]
    public long Id { get; set; }
    [JsonProperty("parent_id")]
    public long OwnerId { get; set; }
    
    [JsonProperty("change")]
    public long Change { get; set; }
    
    [JsonProperty("owner")]
    public string Owner { get; set; }
    
    [JsonProperty("rating")]
    public string Rating { get; set; }
    [JsonProperty("score")]
    public int Score { get; set; }
    
    [JsonProperty("sample")]
    public int Sample { get; set; }
    [JsonProperty("sample_height")]
    public int SampleHeight { get; set; }
    [JsonProperty("sample_width")]
    public int SampleWidth { get; set; }
    
    [JsonProperty("tags")]
    public string Tags { get; set; }
}