using System.ComponentModel.DataAnnotations;
using SammBot.Bot.Classes;

namespace SammBot.Bot.Database
{
    public enum WarnLimitAction
    {
        Kick,
        Ban,
        None
    }

    // REMINDER: Set these default values in BotDatabase -> OnModelCreating too!
    public class GuildConfig
    {
        [Key]
        public ulong GuildId { get; set; }

        [FullName("Warning Limit")]
        [FullDescription("The amount of warnings you can give to someone before they receive an automatic punishment.")]
        public int WarningLimit { get; set; } = 3;
        
        [FullName("Warning Limit Action")]
        [FullDescription("The automatic punishment to do when a user reaches the warning limit.\nValid values are `Kick`, `Ban` and `None`.")]
        public WarnLimitAction WarningLimitAction { get; set; } = WarnLimitAction.None;

        [FullName("Enable Logging")]
        [FullDescription("Enable server event logging?")]
        public bool EnableLogging { get; set; } = false;
        
        [FullName("Log Channel ID")]
        [FullDescription("The channel ID where logs will be sent to. The bot must have permission to write to that channel.")]
        public ulong LogChannel { get; set; }
        
        [FullName("Enable Welcome Message")]
        [FullDescription("Enable welcome message?")]
        public bool EnableWelcome { get; set; } = false;
        
        [FullName("Welcome Channel ID")]
        [FullDescription("The channel ID where welcome messages will be sent to. The bot must have permission to write to that channel.")]
        public ulong WelcomeChannel { get; set; }
        
        [FullName("Welcome Message Template")]
        [FullDescription("The template for the welcome message.\n{0} is the user mention, {1} is the server name.")]
        public string WelcomeMessage { get; set; } = "{0}, welcome to {1}! Remember to read the rules before chatting!";
    }
}
