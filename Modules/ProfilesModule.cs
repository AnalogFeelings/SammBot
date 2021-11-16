using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SammBotNET.Database;
using SammBotNET.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("User Profiles")]
    [Group("profiles")]
    [Summary("Commands related to user profiles.")]
    public class ProfilesModule : ModuleBase<SocketCommandContext>
    {
        [Command("setpronouns")]
        [Summary("Set your pronouns with this command.")]
        public async Task<RuntimeResult> SetPronounsAsync(string Subject, string Object,
            string DependentPossessive, string IndependentPossessive, string Reflexive)
        {
            if (Subject.Length > 8)
                return ExecutionResult.FromError("The subject is too long! Must be less than 9 characters.");
            if (Object.Length > 8)
                return ExecutionResult.FromError("The object is too long! Must be less than 9 characters.");
            if (DependentPossessive.Length > 9)
                return ExecutionResult.FromError("The dependent possessive is too long! Must be less than 10 characters.");
            if (IndependentPossessive.Length > 10)
                return ExecutionResult.FromError("The independent possessive is too long! Must be less than 11 characters.");
            if (IndependentPossessive.Length > 15)
                return ExecutionResult.FromError("The reflexive is too long! Must be less than 16 characters.");

            using(PronounsDB PronounsDatabase = new())
            {
                List<Pronoun> AllPronouns = await PronounsDatabase.Pronouns.ToListAsync();

                if(AllPronouns.Any(x => x.UserId == Context.Message.Author.Id))
                {
                    Pronoun existingPronouns = AllPronouns.Single(y => y.UserId == Context.Message.Author.Id);
                    existingPronouns.Subject = Subject;
                    existingPronouns.Object = Object;
                    existingPronouns.DependentPossessive = DependentPossessive;
                    existingPronouns.IndependentPossessive = IndependentPossessive;
                    existingPronouns.Reflexive = Reflexive;
                }
                else
                {
                    await PronounsDatabase.AddAsync(new Pronoun
                    {
                        UserId = Context.Message.Author.Id,
                        Subject = Subject,
                        Object = Object,
                        DependentPossessive = DependentPossessive,
                        IndependentPossessive = IndependentPossessive,
                        Reflexive = Reflexive
                    });
                }

                await PronounsDatabase.SaveChangesAsync();
            }

            await ReplyAsync($"Done! Your new pronouns are: `{Subject}/{Object}`.");

            return ExecutionResult.Succesful();
        }

        [Command("getpronouns")]
        [Summary("Get the pronouns of a user!")]
        public async Task<RuntimeResult> GetPronounsAsync(SocketGuildUser User = null)
        {
            SocketGuildUser userHolder = User ?? Context.Message.Author as SocketGuildUser;

            using(PronounsDB PronounsDatabase = new())
            {
                List<Pronoun> AllPronouns = await PronounsDatabase.Pronouns.ToListAsync();

                if (AllPronouns.Any(x => x.UserId == userHolder.Id))
                {
                    Pronoun existingPronouns = AllPronouns.Single(y => y.UserId == userHolder.Id);
                    string concatenatedPronouns = $"{existingPronouns.Subject}/{existingPronouns.Object}";

                    await ReplyAsync($"**{userHolder.GetUsernameOrNick()}**'s pronouns are: `{concatenatedPronouns}`.");
                }
                else
                    return ExecutionResult.FromError($"The user **{userHolder.GetUsernameOrNick()}** does not have any pronouns set!");
            }

            return ExecutionResult.Succesful();
        }
    }
}
