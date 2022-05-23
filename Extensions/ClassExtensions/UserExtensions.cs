using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Extensions
{
	public static class UserExtensions
	{
		public static string GetAvatarOrDefault(this SocketUser User)
		{
			return User.GetAvatarUrl() ?? User.GetDefaultAvatarUrl();
		}

		public static string GetGuildAvatarGlobalOrDefault(this SocketUser User)
		{
			if (User is SocketGuildUser Target)
				return Target.GetGuildAvatarUrl() ?? Target.GetAvatarOrDefault();

			return User.GetAvatarOrDefault();
		}

		public static string GetUsernameOrNick(this SocketGuildUser User)
		{
			return User.Nickname ?? User.Username;
		}

		public static string GetFullUsername(this IUser User)
		{
			return $"{User.Username}#{User.Discriminator}";
		}

		public static string GetStatusString(this SocketUser User)
		{
			string OnlineStatus = "Unknown";

			switch (User.Status)
			{
				case UserStatus.DoNotDisturb: OnlineStatus = "Do Not Disturb"; break;
				case UserStatus.Idle: OnlineStatus = "Idle"; break;
				case UserStatus.Offline: OnlineStatus = "Offline"; break;
				case UserStatus.Online: OnlineStatus = "Online"; break;
			}

			return OnlineStatus;
		}

		public static async Task<Pronoun> GetUserPronouns(this SocketUser User)
		{
			using (PronounsDB PronounsDatabase = new PronounsDB())
			{
				List<Pronoun> PronounList = await PronounsDatabase.Pronouns.ToListAsync();
				Pronoun ChosenPronoun = PronounList.SingleOrDefault(y => y.UserId == User.Id);

				//Check for both null and default, .NET can be pretty fucky sometimes.
				if (ChosenPronoun != null && ChosenPronoun != default(Pronoun))
				{
					return ChosenPronoun;
				}
				else
					return new Pronoun()
					{
						Subject = "they",
						Object = "them",
						DependentPossessive = "their",
						IndependentPossessive = "theirs",
						ReflexiveSingular = "themself",
						ReflexivePlural = "themselves"
					};
			}
		}
	}
}
