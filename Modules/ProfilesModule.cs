using Discord;
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
		[FullDescription("Set your pronouns with this command. Some commands use **they/them** by default unless you set something else.")]
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
						Pronoun ExistingPronouns = AllPronouns.Single(y => y.UserId == Context.Message.Author.Id);
						ExistingPronouns.Subject = Subject;
						ExistingPronouns.Object = Object;
						ExistingPronouns.DependentPossessive = DependentPossessive;
						ExistingPronouns.IndependentPossessive = IndependentPossessive;
						ExistingPronouns.ReflexiveSingular = ReflexiveSingular;
						ExistingPronouns.ReflexivePlural = ReflexivePlural;
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

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync($"Done! Your new pronouns are: `{Subject}/{Object}`.", allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("getpronouns")]
		[Summary("Get the pronouns of a user!")]
		[FullDescription("Gets the pronoun information about a user!")]
		public async Task<RuntimeResult> GetPronounsAsync(SocketGuildUser User = null)
		{
			SocketGuildUser TargetUser = User ?? Context.Message.Author as SocketGuildUser;

			using (Context.Channel.EnterTypingState())
			{
				using (PronounsDB PronounsDatabase = new PronounsDB())
				{
					List<Pronoun> AllPronouns = await PronounsDatabase.Pronouns.ToListAsync();

					if (AllPronouns.Any(x => x.UserId == TargetUser.Id))
					{
						Pronoun ExistingPronouns = AllPronouns.Single(y => y.UserId == TargetUser.Id);
						string FormattedPronouns = $"{ExistingPronouns.Subject}/{ExistingPronouns.Object}";

						MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
						AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
						await ReplyAsync($"**{TargetUser.GetUsernameOrNick()}**'s pronouns are: `{FormattedPronouns}`.", allowedMentions: AllowedMentions, messageReference: Reference);
					}
					else
						return ExecutionResult.FromError($"The user **{TargetUser.GetUsernameOrNick()}** does not have any pronouns set!");
				}
			}

			return ExecutionResult.Succesful();
		}
	}
}
