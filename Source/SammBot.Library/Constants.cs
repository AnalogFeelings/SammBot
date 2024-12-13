#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021 Analog Feelings
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

using Discord;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SammBot.Library;

/// <summary>
/// Container class for constant or readonly fields.
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
    
    /// <summary>
    /// The bot's name.
    /// </summary>
    public const string BOT_NAME = "Samm-Bot";

    /// <summary>
    /// The bot's config storage folder.
    /// </summary>
    public const string CONFIG_FOLDER = "Config";
    
    /// <summary>
    /// Allows only users to be notified from a message.
    /// </summary>
    public static readonly AllowedMentions AllowOnlyUsers = new AllowedMentions(AllowedMentionTypes.Users);

    /// <summary>
    /// The path to the bot's data storage directory.
    /// </summary>
    public static readonly string BotDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), BOT_NAME);

    /// <summary>
    /// The path to the bot's config storage directory.
    /// </summary>
    public static readonly string BotConfigPath = Path.Combine(BotDataDirectory, CONFIG_FOLDER);
    
    /// <summary>
    /// Default JSON serialization settings.
    /// </summary>
    public static readonly JsonSerializerOptions JsonSettings = new JsonSerializerOptions()
    {
        WriteIndented = true,
        IncludeFields = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}