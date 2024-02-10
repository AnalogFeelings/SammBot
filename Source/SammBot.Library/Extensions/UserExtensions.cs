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

public static class UserExtensions
{
    public static string GetAvatarOrDefault(this SocketUser user, ushort size)
    {
        return user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();
    }

    public static string GetGuildOrGlobalAvatar(this SocketGuildUser user, ushort size)
    {
        return user.GetGuildAvatarUrl(size: size) ?? user.GetAvatarUrl(size: size);
    }

    public static string GetGuildGlobalOrDefaultAvatar(this SocketUser user, ushort size)
    {
        if (user is SocketGuildUser targetUser)
            return targetUser.GetGuildAvatarUrl(size: size) ?? targetUser.GetAvatarOrDefault(size);

        return user.GetAvatarOrDefault(size);
    }

    public static string GetUsernameOrNick(this SocketGuildUser user)
    {
        if (!user.HasPomelo())
            return user.Nickname ?? user.Username;

        return user.DisplayName;
    }

    public static string GetFullUsername(this IUser user)
    {
        if (!user.HasPomelo())
            return $"{user.Username}#{user.Discriminator}";

        return $"@{user.Username}";
    }

    public static bool HasPomelo(this IUser user)
    {
        return user.DiscriminatorValue == 0;
    }

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