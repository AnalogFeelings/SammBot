using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SammBot.Bot.Database;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBot.Bot.Extensions;

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
        if (User is SocketGuildUser targetUser)
            return targetUser.GetGuildAvatarUrl(size: Size) ?? targetUser.GetAvatarOrDefault(Size);

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
        string onlineStatus = User.Status switch
        {
            UserStatus.DoNotDisturb => "Do Not Disturb",
            UserStatus.Idle => "Idle",
            UserStatus.Offline => "Offline",
            UserStatus.Online => "Online",
            _ => "Unknown"
        };

        return onlineStatus;
    }

    public static async Task<Pronoun> GetUserPronouns(this SocketUser User)
    {
        using (BotDatabase botDatabase = new BotDatabase())
        {
            Pronoun chosenPronoun = await botDatabase.Pronouns.SingleOrDefaultAsync(x => x.UserId == User.Id);

            if (chosenPronoun != default(Pronoun)) return chosenPronoun;
                
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