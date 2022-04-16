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

        public static string GetUsernameOrNick(this SocketGuildUser User)
        {
            return User.Nickname ?? User.Username;
        }

        public static string GetFullUsername(this SocketUser User)
        {
            return $"{User.Username}#{User.Discriminator}";
        }

        public static string GetStatusString(this SocketUser User)
        {
            string userStatus = "Unknown";

            switch (User.Status)
            {
                case UserStatus.DoNotDisturb: userStatus = "Do Not Disturb"; break;
                case UserStatus.Idle: userStatus = "Idle"; break;
                case UserStatus.Offline: userStatus = "Offline"; break;
                case UserStatus.Online: userStatus = "Online"; break;
            }

            return userStatus;
        }

        public static async Task<Pronoun> GetUserPronouns(this SocketUser User)
        {
            using (PronounsDB PronounsDatabase = new())
            {
                List<Pronoun> AllPronouns = await PronounsDatabase.Pronouns.ToListAsync();
                Pronoun pickedPronoun = AllPronouns.SingleOrDefault(y => y.UserId == User.Id);

                //Check for both null and default, .NET can be pretty fucky sometimes.
                if (pickedPronoun != null && pickedPronoun != default(Pronoun))
                {
                    return pickedPronoun;
                }
                else
                    return new()
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
