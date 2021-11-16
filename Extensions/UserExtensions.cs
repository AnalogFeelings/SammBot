using Discord;
using Discord.WebSocket;

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
    }
}
