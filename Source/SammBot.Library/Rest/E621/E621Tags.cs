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

namespace SammBot.Library.Rest.E621;

/// <summary>
/// A class that contains a post's tags on e621.
/// </summary>
public class E621Tags
{
    [JsonPropertyName("general")]
    public List<string> General { get; set; }
    [JsonPropertyName("species")]
    public List<string> Species { get; set; }
    [JsonPropertyName("character")]
    public List<string> Character { get; set; }
    [JsonPropertyName("artist")]
    public List<string> Artist { get; set; }
    [JsonPropertyName("invalid")]
    public List<string> Invalid { get; set; }
    [JsonPropertyName("lore")]
    public List<string> Lore { get; set; }
    [JsonPropertyName("meta")]
    public List<string> Meta { get; set; }
}