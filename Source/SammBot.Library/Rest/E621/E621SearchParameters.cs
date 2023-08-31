﻿#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2023 Analog Feelings
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

using SammBot.Library.Attributes;

namespace SammBot.Library.Rest.E621;

/// <summary>
/// A class used to pass search parameters to the e621
/// API.
/// </summary>
public class E621SearchParameters
{
    [UglyName("limit")]
    public int Limit { get; set; }
    [UglyName("tags")]
    public string Tags { get; set; }
    [UglyName("page")]
    public string PageNumber { get; set; }
}