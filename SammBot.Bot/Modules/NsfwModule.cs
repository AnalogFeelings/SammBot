#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2023 AestheticalZ
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using SammBot.Bot.Attributes;
using SammBot.Bot.Preconditions;

namespace SammBot.Bot.Modules;

[PrettyName("NSFW")]
[Group("nsfw", "NSFW commands such as rule34 search, and more. Requires NSFW channel.")]
[ModuleEmoji("\U0001f51e")]
public class NsfwModule : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("r34", "Gets a list of images from rule34.")]
    [DetailedDescription("Gets a list of images from rule34. Maximum amount is 1000 images per command.")]
    [RateLimit(3, 2)]
    [RequireContext(ContextType.Guild)]
    [RequireNsfw]
    public async Task<RuntimeResult> GetRule34Async()
    {
        ComponentBuilder testBuilder = new ComponentBuilder().WithButton("Test!", "test-id", ButtonStyle.Success, new Emoji("\U0001f44d"));

        await RespondAsync("Here's a button!", components: testBuilder.Build());

        return ExecutionResult.Succesful();
    }

    [ComponentInteraction("test-id", true)]
    public async Task ReplyTest()
    {
        IComponentInteraction interaction = Context.Interaction as IComponentInteraction;

        await interaction.UpdateAsync(x =>
        {
            x.Content = "Clicked!!";
            x.Components = null;
        });
    }
}