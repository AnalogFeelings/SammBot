#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Text.Json.Serialization;

namespace SammBot.Library.Rest.Rule34;

/// <summary>
/// A class that contains a post from the Rule34 API.
/// </summary>
public class Rule34Post
{
    [JsonPropertyName("preview_url")]
    public string PreviewUrl { get; set; }
    [JsonPropertyName("sample_url")]
    public string SampleUrl { get; set; }
    [JsonPropertyName("file_url")]
    public string FileUrl { get; set; }
    [JsonPropertyName("image")]
    public string Image { get; set; }
    
    [JsonPropertyName("directory")]
    public int Directory { get; set; }
    [JsonPropertyName("hash")]
    public string Hash { get; set; }
    
    [JsonPropertyName("width")]
    public int Width { get; set; }
    [JsonPropertyName("height")]
    public int Height { get; set; }
    
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("parent_id")]
    public long OwnerId { get; set; }
    
    [JsonPropertyName("change")]
    public long Change { get; set; }
    
    [JsonPropertyName("owner")]
    public string Owner { get; set; }
    
    [JsonPropertyName("rating")]
    public string Rating { get; set; }
    [JsonPropertyName("score")]
    public int Score { get; set; }
    
    [JsonPropertyName("sample")]
    public int Sample { get; set; }
    [JsonPropertyName("sample_height")]
    public int SampleHeight { get; set; }
    [JsonPropertyName("sample_width")]
    public int SampleWidth { get; set; }
    
    [JsonPropertyName("tags")]
    public string Tags { get; set; }
}