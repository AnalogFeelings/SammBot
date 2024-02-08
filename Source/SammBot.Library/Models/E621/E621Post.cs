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

namespace SammBot.Library.Models.E621;

/// <summary>
/// A class that contains a post from the e621 API.
/// </summary>
public class E621Post
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; }
    [JsonPropertyName("updated_at")]
    public string UpdatedAt { get; set; }
    
    [JsonPropertyName("file")]
    public E621File File { get; set; }
    
    [JsonPropertyName("score")]
    public E621Score Score { get; set; }
    [JsonPropertyName("tags")]
    public E621Tags Tags { get; set; }
    
    [JsonPropertyName("rating")]
    public string Rating { get; set; }
    [JsonPropertyName("fav_count")]
    public long FavoriteCount { get; set; }
    
    [JsonPropertyName("description")]
    public string Description { get; set; }
    [JsonPropertyName("comment_count")]
    public int CommentCount { get; set; }
}