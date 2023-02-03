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

using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Attributes;
using SammBot.Bot.Core;
using SammBot.Bot.Database;
using SammBot.Bot.Extensions;
using SammBot.Bot.Preconditions;

namespace SammBot.Bot.Modules;

[PrettyName("User Profiles")]
[Group("profiles", "Commands related to user profiles.")]
[ModuleEmoji("\U0001f465")]
public class ProfilesModule : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("setpronouns", "Set your pronouns with this command.")]
    [DetailedDescription("Set your pronouns with this command. Some commands use they/them by default unless you set something else.")]
    [RateLimit(3, 1)]
    public async Task<RuntimeResult> SetPronounsAsync([Summary(description: "Self-explanatory.")] string Subject,
        [Summary(description: "Self-explanatory.")] string Object,
        [Summary(description: "Self-explanatory.")] string DependentPossessive, 
        [Summary(description: "Self-explanatory.")] string IndependentPossessive, 
        [Summary(description: "Self-explanatory.")] string ReflexiveSingular, 
        [Summary(description: "Self-explanatory.")] string ReflexivePlural)
    {
        if (Subject.Length > 8)
            return ExecutionResult.FromError("The subject is too long! Must be less than 9 characters.");
        if (Object.Length > 8)
            return ExecutionResult.FromError("The object is too long! Must be less than 9 characters.");
        if (DependentPossessive.Length > 9)
            return ExecutionResult.FromError("The dependent possessive is too long! Must be less than 10 characters.");
        if (IndependentPossessive.Length > 10)
            return ExecutionResult.FromError("The independent possessive is too long! Must be less than 11 characters.");
        if (ReflexiveSingular.Length > 15)
            return ExecutionResult.FromError("The singular reflexive is too long! Must be less than 16 characters.");
        if (ReflexivePlural.Length > 15)
            return ExecutionResult.FromError("The plural reflexive is too long! Must be less than 16 characters.");

        await DeferAsync(true);

        using (BotDatabase botDatabase = new BotDatabase())
        {
            if (botDatabase.Pronouns.Any(x => x.UserId == Context.Interaction.User.Id))
            {
                Pronoun existingPronouns = await botDatabase.Pronouns.SingleAsync(x => x.UserId == Context.Interaction.User.Id);
                        
                existingPronouns.Subject = Subject;
                existingPronouns.Object = Object;
                existingPronouns.DependentPossessive = DependentPossessive;
                existingPronouns.IndependentPossessive = IndependentPossessive;
                existingPronouns.ReflexiveSingular = ReflexiveSingular;
                existingPronouns.ReflexivePlural = ReflexivePlural;
            }
            else
            {
                Pronoun newPronoun = new Pronoun
                {
                    UserId = Context.Interaction.User.Id,
                    Subject = Subject,
                    Object = Object,
                    DependentPossessive = DependentPossessive,
                    IndependentPossessive = IndependentPossessive,
                    ReflexiveSingular = ReflexiveSingular,
                    ReflexivePlural = ReflexivePlural
                };
                
                await botDatabase.Pronouns.AddAsync(newPronoun);
            }

            await botDatabase.SaveChangesAsync();
        }

        await FollowupAsync($"Done! Your new pronouns are: `{Subject}/{Object}`.", allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("getpronouns", "Get the pronouns of a user!")]
    [DetailedDescription("Gets the pronoun information about a user!")]
    [RateLimit(3, 2)]
    [RequireContext(ContextType.Guild)]
    public async Task<RuntimeResult> GetPronounsAsync([Summary(description: "The user you want to get the pronouns of.")] SocketGuildUser? User = null)
    {
        SocketGuildUser targetUser = User ?? (Context.Interaction.User as SocketGuildUser)!;

        await DeferAsync();

        using (BotDatabase botDatabase = new BotDatabase())
        {
            if (botDatabase.Pronouns.Any(x => x.UserId == targetUser.Id))
            {
                Pronoun existingPronouns = await botDatabase.Pronouns.SingleAsync(x => x.UserId == targetUser.Id);
                string formattedPronouns = $"{existingPronouns.Subject}/{existingPronouns.Object}";

                await FollowupAsync($"**{targetUser.GetUsernameOrNick()}**'s pronouns are: `{formattedPronouns}`.",
                    allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
            }
            else
            {
                return ExecutionResult.FromError($"The user **{targetUser.GetUsernameOrNick()}** does not have any pronouns set!");
            }
        }

        return ExecutionResult.Succesful();
    }
}