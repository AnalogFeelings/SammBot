using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("User Profiles")]
	[Group("profiles")]
	[Summary("Commands related to user profiles.")]
	[ModuleEmoji("🚻")]
	public class ProfilesModule : ModuleBase<SocketCommandContext>
	{
		[Command("setpronouns")]
		[Summary("Set your pronouns with this command.")]
		public async Task<RuntimeResult> SetPronounsAsync(string Subject, string Object,
			string DependentPossessive, string IndependentPossessive, string ReflexiveSingular, string ReflexivePlural)
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
				using (PronounsDB PronounsDatabase = new PronounsDB())
				{
					List<Pronoun> AllPronouns = await PronounsDatabase.Pronouns.ToListAsync();

					if (AllPronouns.Any(x => x.UserId == Context.Message.Author.Id))
					{
						Pronoun existingPronouns = AllPronouns.Single(y => y.UserId == Context.Message.Author.Id);
						existingPronouns.Subject = Subject;
						existingPronouns.Object = Object;
						existingPronouns.DependentPossessive = DependentPossessive;
						existingPronouns.IndependentPossessive = IndependentPossessive;
						existingPronouns.ReflexiveSingular = ReflexiveSingular;
						existingPronouns.ReflexivePlural = ReflexivePlural;
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
							ReflexiveSingular = ReflexiveSingular,
							ReflexivePlural = ReflexivePlural
						});
					}

					await PronounsDatabase.SaveChangesAsync();
				}
			}

			await ReplyAsync($"Done! Your new pronouns are: `{Subject}/{Object}`.");

			return ExecutionResult.Succesful();
		}

		[Command("getpronouns")]
		[Summary("Get the pronouns of a user!")]
		public async Task<RuntimeResult> GetPronounsAsync(SocketGuildUser User = null)
		{
			SocketGuildUser userHolder = User ?? Context.Message.Author as SocketGuildUser;

			using (Context.Channel.EnterTypingState())
			{
				using (PronounsDB PronounsDatabase = new PronounsDB())
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
			}

			return ExecutionResult.Succesful();
		}
	}
}
