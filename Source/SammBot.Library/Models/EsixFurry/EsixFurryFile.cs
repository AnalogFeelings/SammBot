﻿#region License Information (GPLv3)
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
/// A class that contains a file from the e621 CDN.
/// </summary>
public record EsixFurryFile
{
    [JsonPropertyName("width")]
    public int Width;
    [JsonPropertyName("height")]
    public int Height;
    
    [JsonPropertyName("ext")]
    public required string Extension;
    [JsonPropertyName("size")]
    public ulong Size;
    
    [JsonPropertyName("md5")]
    public required string HashMd5;
    [JsonPropertyName("url")]
    public required string FileUrl;
}