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

namespace SammBot.Library.Models.Data;

public record BotConfig
{
    // Lists
    public List<string> HugKaomojis { get; set; } = new List<string>();
    public List<string> KillMessages { get; set; } = new List<string>();
    public List<BotStatus> StatusList { get; set; } = new List<BotStatus>();
    public List<string> Plugins { get; set; } = new List<string>();

    // Emojis
    public string ShipBarStartEmpty { get; set; } = string.Empty;
    public string ShipBarStartFull { get; set; } = string.Empty;
    public string ShipBarHalfEmpty { get; set; } = string.Empty;
    public string ShipBarHalfFull { get; set; } = string.Empty;
    public string ShipBarEndEmpty { get; set; } = string.Empty;
    public string ShipBarEndFull { get; set; } = string.Empty;

    // Behavior
    public int MessageCacheSize { get; set; } = 2000;
    public int TagDistance { get; set; } = 3;
    public bool OnlyOwnerMode { get; set; } = false;
    public bool RotatingStatus { get; set; } = false;
    public bool WaitForDebugger { get; set; } = false;
    public string TwitchUrl { get; set; } = "https://www.twitch.tv/coreaesthetics";
    public string CommandLogFormat { get; set; } = "Executing command \"{0}\". Channel: #{1}. User: @{2}.";
    public string HttpUserAgent { get; set; } = "Placeholder User Agent (.NET Application)";

    // API Tokens
    public string BotToken { get; set; } = string.Empty;
}