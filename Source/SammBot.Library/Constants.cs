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
    /// The bot's name.
    /// </summary>
    public const string BOT_NAME = "Samm-Bot";

    /// <summary>
    /// The bot's config file's filename.
    /// </summary>
    public const string CONFIG_FILE = "config.json";
    
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
    /// Allows only users to be notified from a message.
    /// </summary>
    public static readonly AllowedMentions AllowOnlyUsers = new AllowedMentions(AllowedMentionTypes.Users);

    /// <summary>
    /// The path to the bot's data storage directory.
    /// </summary>
    public static readonly string BotDataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), BOT_NAME);
    
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

    /// <summary>
    /// A warning emoji.
    /// </summary>
    public const string WARNING_EMOJI = "\u26A0\uFE0F";

    /// <summary>
    /// An emoji for a white heavy checkmark.
    /// </summary>
    public const string WHITE_CHECKMARK_EMOJI = "\u2705";

    /// <summary>
    /// An emoji for a slot machine.
    /// </summary>
    public const string SLOT_MACHINE_EMOJI = "\U0001f3b0";

    /// <summary>
    /// An emoji for a cat face.
    /// </summary>
    public const string CAT_EMOJI = "\U0001f431";

    /// <summary>
    /// An emoji for a dog face.
    /// </summary>
    public const string DOG_EMOJI = "\U0001f436";

    /// <summary>
    /// An emoji for a fox face.
    /// </summary>
    public const string FOX_EMOJI = "\U0001f98a";

    /// <summary>
    /// An emoji for a duck.
    /// </summary>
    public const string DUCK_EMOJI = "\U0001f986";

    /// <summary>
    /// An emoji for a game die.
    /// </summary>
    public const string DIE_EMOJI = "\U0001f3b2";

    /// <summary>
    /// An emoji for a cross mark.
    /// </summary>
    public const string CROSS_MARK_EMOJI = "\u274C";

    /// <summary>
    /// An emoji for a broken heart.
    /// </summary>
    public const string BROKEN_HEART_EMOJI = "\U0001f494";

    /// <summary>
    /// An emoji for a red heart.
    /// </summary>
    public const string RED_HEART_EMOJI = "\u2764\uFE0F";

    /// <summary>
    /// An emoji for a heart with a ribbon.
    /// </summary>
    public const string RIBBON_HEART_EMOJI = "\U0001f49d";

    /// <summary>
    /// An emoji for a heart with sparkles.
    /// </summary>
    public const string SPARKLE_HEART_EMOJI = "\U0001f496";
}