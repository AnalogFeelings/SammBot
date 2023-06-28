#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
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

using Discord;
using Discord.WebSocket;

namespace SammBot.Library.Extensions;

public static class UserExtensions
{
    public static string GetAvatarOrDefault(this SocketUser User, ushort Size)
    {
        return User.GetAvatarUrl(size: Size) ?? User.GetDefaultAvatarUrl();
    }

    public static string GetGuildOrGlobalAvatar(this SocketGuildUser User, ushort Size)
    {
        return User.GetGuildAvatarUrl(size: Size) ?? User.GetAvatarUrl(size: Size);
    }

    public static string GetGuildGlobalOrDefaultAvatar(this SocketUser User, ushort Size)
    {
        if (User is SocketGuildUser targetUser)
            return targetUser.GetGuildAvatarUrl(size: Size) ?? targetUser.GetAvatarOrDefault(Size);

        return User.GetAvatarOrDefault(Size);
    }

    public static string GetUsernameOrNick(this SocketGuildUser User)
    {
        if (!User.HasPomelo())
            return User.Nickname ?? User.Username;

        return User.DisplayName;
    }

    public static string GetFullUsername(this IUser User)
    {
        if (!User.HasPomelo())
            return $"{User.Username}#{User.Discriminator}";

        return $"@{User.Username}";
    }

    public static bool HasPomelo(this IUser User)
    {
        return User.DiscriminatorValue == 0;
    }

    public static string GetStatusString(this SocketUser User)
    {
        string onlineStatus = User.Status switch
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