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
using SammBot.Bot.Core;
using System;
using Discord.Interactions;

namespace SammBot.Bot.Extensions;

public static class EmbedExtensions
{
    public static EmbedBuilder BuildDefaultEmbed(this EmbedBuilder Builder, ShardedInteractionContext Context, string Title = "", string Description = "")
    {
        if (Context == null)
            throw new ArgumentNullException(nameof(Context));

        string botName = SettingsManager.BOT_NAME;

        Builder.Color = Color.DarkPurple;
        Builder.Title = $"{botName.ToUpper()} {Title.ToUpper()}";
        Builder.Description = Description;

        Builder.WithFooter(x =>
        {
            x.Text = $"Requested by {Context.Interaction.User.GetFullUsername()}"; 
            x.IconUrl = Context.Interaction.User.GetGuildGlobalOrDefaultAvatar(256);
        });
        Builder.WithCurrentTimestamp();

        return Builder;
    }

    public static EmbedBuilder ChangeTitle(this EmbedBuilder Builder, string Title, bool IncludeName = false)
    {
        string botName = SettingsManager.BOT_NAME;

        if (IncludeName) Builder.WithTitle($"{botName.ToUpper()} {Title.ToUpper()}");
        else Builder.WithTitle(Title.ToUpper());

        return Builder;
    }

    public static EmbedBuilder ChangeFooter(this EmbedBuilder Builder, ShardedInteractionContext Context, string Text)
    {
        Builder.WithFooter(x =>
        {
            x.Text = Text; 
            x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
        });

        return Builder;
    }
}