#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using System.ComponentModel.DataAnnotations;
using SammBot.Bot.Common.Attributes;

namespace SammBot.Bot.Database.Models;

public enum WarnLimitAction
{
    Kick,
    Ban,
    None
}

// REMINDER: Set these default values in BotDatabase -> OnModelCreating too!
public class GuildConfig
{
    [Key]
    public ulong GuildId { get; set; }

    [PrettyName("Warning Limit")]
    [DetailedDescription("The amount of warnings you can give to someone before they receive an automatic punishment.")]
    public int WarningLimit { get; set; } = 3;
        
    [PrettyName("Warning Limit Action")]
    [DetailedDescription("The automatic punishment to do when a user reaches the warning limit.\nValid values are `Kick`, `Ban` and `None`.")]
    public WarnLimitAction WarningLimitAction { get; set; } = WarnLimitAction.None;

    [PrettyName("Enable Logging")]
    [DetailedDescription("Enable server event logging?")]
    public bool EnableLogging { get; set; } = false;
        
    [PrettyName("Log Channel ID")]
    [DetailedDescription("The channel ID where logs will be sent to. The bot must have permission to write to that channel.")]
    public ulong LogChannel { get; set; }
        
    [PrettyName("Enable Welcome Message")]
    [DetailedDescription("Enable welcome message?")]
    public bool EnableWelcome { get; set; } = false;
        
    [PrettyName("Welcome Channel ID")]
    [DetailedDescription("The channel ID where welcome messages will be sent to. The bot must have permission to write to that channel.")]
    public ulong WelcomeChannel { get; set; }
        
    [PrettyName("Welcome Message Template")]
    [DetailedDescription("The template for the welcome message.")]
    public string WelcomeMessage { get; set; } = "%usermention%, welcome to %servername%! Remember to read the rules before chatting!";
}