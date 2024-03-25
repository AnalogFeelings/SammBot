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

namespace SammBot.Library.Models.EsixFurry;

/// <summary>
/// A class that contains a post from the e621 API.
/// </summary>
public record EsixFurryPost
{
    [JsonPropertyName("id")]
    public long Id;
    
    [JsonPropertyName("created_at")]
    public required string CreatedAt;
    [JsonPropertyName("updated_at")]
    public string? UpdatedAt;
    
    [JsonPropertyName("file")]
    public required EsixFurryFile File;
    
    [JsonPropertyName("score")]
    public required EsixFurryScore Score;
    [JsonPropertyName("tags")]
    public required EsixFurryTags Tags;
    
    [JsonPropertyName("rating")]
    public required string Rating;
    [JsonPropertyName("fav_count")]
    public long FavoriteCount;
    
    [JsonPropertyName("description")]
    public required string Description;
    [JsonPropertyName("comment_count")]
    public int CommentCount;
}