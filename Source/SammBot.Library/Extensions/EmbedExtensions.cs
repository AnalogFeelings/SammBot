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
using Discord.Interactions;

namespace SammBot.Library.Extensions;

public static class EmbedExtensions
{
    public static EmbedBuilder BuildDefaultEmbed(this EmbedBuilder Builder, ShardedInteractionContext Context)
    {
        if (Context == null)
            throw new ArgumentNullException(nameof(Context));

        Builder.WithFooter(x =>
        {
            x.Text = $"Requested by {Context.Interaction.User.GetFullUsername()}"; 
            x.IconUrl = Context.Interaction.User.GetGuildGlobalOrDefaultAvatar(256);
        });
        Builder.WithCurrentTimestamp();

        return Builder;
    }
}