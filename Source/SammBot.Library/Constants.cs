#region License Information (GPLv3)
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

using Discord;

namespace SammBot.Library;

/// <summary>
/// Class that contains some constants.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Color to represent a success.
    /// </summary>
    public static readonly Color GoodColor = new Color(119, 178, 85);
    
    /// <summary>
    /// Color to represent a warning.
    /// </summary>
    public static readonly Color BadColor = new Color(255, 205, 77);
    
    /// <summary>
    /// Color to represent an error.
    /// </summary>
    public static readonly Color VeryBadColor = new Color(221, 46, 68);
}