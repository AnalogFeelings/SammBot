using System.ComponentModel.DataAnnotations;
using SammBot.Bot.Attributes;

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

        [PrettyName("Warning Limit")]
        [DetailedDescription("The amount of warnings you can give to someone before they receive an automatic punishment.")]
        public int WarningLimit { get; set; } = 3;
        
        [PrettyName("Warning Limit Action")]
        [DetailedDescription("The automatic punishment to do when a user reaches the warning limit.\nValid values are `Kick`, `Ban` and `None`.")]
        public WarnLimitAction WarningLimitAction { get; set; } = WarnLimitAction.None;

        [PrettyName("Enable Logging")]
        [DetailedDescription("Enable server event logging?")]
        public bool EnableLogging { get; set; } = false;
        
        [PrettyName("Log Channel ID")]
        [DetailedDescription("The channel ID where logs will be sent to. The bot must have permission to write to that channel.")]
        public ulong LogChannel { get; set; }
        
        [PrettyName("Enable Welcome Message")]
        [DetailedDescription("Enable welcome message?")]
        public bool EnableWelcome { get; set; } = false;
        
        [PrettyName("Welcome Channel ID")]
        [DetailedDescription("The channel ID where welcome messages will be sent to. The bot must have permission to write to that channel.")]
        public ulong WelcomeChannel { get; set; }
        
        [PrettyName("Welcome Message Template")]
        [DetailedDescription("The template for the welcome message.\n{0} is the user mention, {1} is the server name.")]
        public string WelcomeMessage { get; set; } = "{0}, welcome to {1}! Remember to read the rules before chatting!";
    }
}
