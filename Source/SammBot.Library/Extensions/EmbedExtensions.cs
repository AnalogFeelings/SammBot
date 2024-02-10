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
using Discord.Interactions;

namespace SammBot.Library.Extensions;

/// <summary>
/// Extension methods for working with rich embeds.
/// </summary>
public static class EmbedExtensions
{
    /// <summary>
    /// Generates a generic embed.
    /// </summary>
    /// <param name="builder">The embed builder.</param>
    /// <param name="context">The interaction context.</param>
    /// <returns>The new embed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public static EmbedBuilder BuildDefaultEmbed(this EmbedBuilder builder, ShardedInteractionContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        builder.WithFooter(x =>
        {
            x.Text = $"Requested by {context.Interaction.User.GetFullUsername()}"; 
            x.IconUrl = context.Interaction.User.GetGuildGlobalOrDefaultAvatar(256);
        });
        builder.WithCurrentTimestamp();

        return builder;
    }

    /// <summary>
    /// Generates a generic embed that represents the success of an operation.
    /// </summary>
    /// <param name="builder">The embed builder.</param>
    /// <param name="context">The interaction context.</param>
    /// <returns>The new embed.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is null.</exception>
    public static EmbedBuilder BuildSuccessEmbed(this EmbedBuilder builder, ShardedInteractionContext context)
    {
        EmbedBuilder defaultEmbed = builder.BuildDefaultEmbed(context);
        
        defaultEmbed.Title = "\u2705 Success";
        defaultEmbed.Color = Constants.GoodColor;

        return defaultEmbed;
    }

    /// <summary>
    /// Generates a generic embed that represents the failure of an operation.
    /// </summary>
    /// <param name="builder">The embed builder.</param>
    /// <param name="context">The interaction context.</param>
    /// <returns>The new embed.</returns>
    public static EmbedBuilder BuildErrorEmbed(this EmbedBuilder builder, ShardedInteractionContext context)
    {
        EmbedBuilder defaultEmbed = builder.BuildDefaultEmbed(context);
        
        defaultEmbed.Title = "\u26A0\uFE0F An error has occurred";
        defaultEmbed.Color = Constants.BadColor;

        return defaultEmbed;
    }
}