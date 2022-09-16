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
        public static string GetAvatarOrDefault(this SocketUser User, ushort Size)
        {
            return User.GetAvatarUrl(size: Size) ?? User.GetDefaultAvatarUrl();
        }

        public static string GetGuildOrGlobalAvatar(this SocketGuildUser User, ushort Size)
        {
            return User.GetGuildAvatarUrl(size: Size) ?? User.GetAvatarUrl(size: Size);
        }

        public static string GetGuildGlobalOrDefaultAvatar(this SocketUser User, ushort Size)
        {
            if (User is SocketGuildUser Target)
                return Target.GetGuildAvatarUrl(size: Size) ?? Target.GetAvatarOrDefault(Size);

            return User.GetAvatarOrDefault(Size);
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
            using (BotDatabase BotDatabase = new BotDatabase())
            {
                List<Pronoun> PronounList = await BotDatabase.Pronouns.ToListAsync();
                Pronoun ChosenPronoun = PronounList.SingleOrDefault(y => y.UserId == User.Id);

                if (ChosenPronoun != default(Pronoun)) return ChosenPronoun;
                
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
