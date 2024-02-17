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

using Discord;
using Discord.WebSocket;

namespace SammBot.Library.Extensions;

/// <summary>
/// Contains extension methods for Discord users.
/// </summary>
public static class UserExtensions
{
    /// <summary>
    /// Returns the user's formatted username.
    /// </summary>
    /// <param name="user">The target user.</param>
    /// <returns>The user's formatted username.</returns>
    public static string GetFullUsername(this IUser user)
    {
        string formattedUsername = user.ToString()!;

        if (user.HasPomelo())
            return $"@{formattedUsername}";

        return formattedUsername;
    }

    /// <summary>
    /// Checks if the user has the new username system.
    /// </summary>
    /// <param name="user">The target user.</param>
    /// <returns><see langword="true"/> if the user has it.</returns>
    public static bool HasPomelo(this IUser user)
    {
        return user.DiscriminatorValue == 0;
    }

    /// <summary>
    /// Gets the user's online status as a string.
    /// </summary>
    /// <param name="user">The target user.</param>
    /// <returns>The user's online status.</returns>
    public static string GetStatusString(this SocketUser user)
    {
        string onlineStatus = user.Status switch
        {
            UserStatus.DoNotDisturb => "Do Not Disturb",
            UserStatus.Idle => "Idle",
            UserStatus.Offline => "Offline",
            UserStatus.Online => "Online",
            _ => "Unknown"
        };

        return onlineStatus;
    }
}