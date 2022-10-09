using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SammBot.Bot.Classes;
using SammBot.Bot.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules
{
    [Name("User Profiles")]
    [Group("profiles")]
    [Summary("Commands related to user profiles.")]
    [ModuleEmoji("\U0001f465")]
    public class ProfilesModule : ModuleBase<SocketCommandContext>
    {
        [Command("setpronouns")]
        [Summary("Set your pronouns with this command.")]
        [FullDescription("Set your pronouns with this command. Some commands use they/them by default unless you set something else.")]
        [RateLimit(3, 1)]
        public async Task<RuntimeResult> SetPronounsAsync([Summary("Self-explanatory.")] string Subject,
                                                          [Summary("Self-explanatory.")] string Object,
                                                          [Summary("Self-explanatory.")] string DependentPossessive, 
                                                          [Summary("Self-explanatory.")] string IndependentPossessive, 
                                                          [Summary("Self-explanatory.")] string ReflexiveSingular, 
                                                          [Summary("Self-explanatory.")] string ReflexivePlural)
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

            using (Context.Channel.EnterTypingState())
            {
                using (BotDatabase botDatabase = new BotDatabase())
                {
                    List<Pronoun> allPronouns = await botDatabase.Pronouns.ToListAsync();

                    if (allPronouns.Any(x => x.UserId == Context.Message.Author.Id))
                    {
                        Pronoun existingPronouns = allPronouns.Single(y => y.UserId == Context.Message.Author.Id);
                        
                        existingPronouns.Subject = Subject;
                        existingPronouns.Object = Object;
                        existingPronouns.DependentPossessive = DependentPossessive;
                        existingPronouns.IndependentPossessive = IndependentPossessive;
                        existingPronouns.ReflexiveSingular = ReflexiveSingular;
                        existingPronouns.ReflexivePlural = ReflexivePlural;
                    }
                    else
                    {
                        await botDatabase.Pronouns.AddAsync(new Pronoun
                        {
                            UserId = Context.Message.Author.Id,
                            Subject = Subject,
                            Object = Object,
                            DependentPossessive = DependentPossessive,
                            IndependentPossessive = IndependentPossessive,
                            ReflexiveSingular = ReflexiveSingular,
                            ReflexivePlural = ReflexivePlural
                        });
                    }

                    await botDatabase.SaveChangesAsync();
                }
            }

            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
            await ReplyAsync($"Done! Your new pronouns are: `{Subject}/{Object}`.", allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }

        [Command("getpronouns")]
        [Summary("Get the pronouns of a user!")]
        [FullDescription("Gets the pronoun information about a user!")]
        [RateLimit(3, 2)]
        public async Task<RuntimeResult> GetPronounsAsync([Summary("The user you want to get the pronouns of.")] SocketGuildUser User = null)
        {
            SocketGuildUser targetUser = User ?? Context.Message.Author as SocketGuildUser;

            using (Context.Channel.EnterTypingState())
            {
                using (BotDatabase botDatabase = new BotDatabase())
                {
                    List<Pronoun> allPronouns = await botDatabase.Pronouns.ToListAsync();

                    if (allPronouns.Any(x => x.UserId == targetUser.Id))
                    {
                        Pronoun existingPronouns = allPronouns.Single(y => y.UserId == targetUser.Id);
                        string formattedPronouns = $"{existingPronouns.Subject}/{existingPronouns.Object}";

                        MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                        AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
                        await ReplyAsync($"**{targetUser.GetUsernameOrNick()}**'s pronouns are: `{formattedPronouns}`.", allowedMentions: allowedMentions, messageReference: messageReference);
                    }
                    else
                        return ExecutionResult.FromError($"The user **{targetUser.GetUsernameOrNick()}** does not have any pronouns set!");
                }
            }

            return ExecutionResult.Succesful();
        }
    }
}
