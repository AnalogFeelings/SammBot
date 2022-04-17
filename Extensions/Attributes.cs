using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SammBotNET.Extensions
{
    public class MustRunInGuild : PreconditionAttribute
    {
        public MustRunInGuild() { }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
                                                                        CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser)
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError("You must execute this command in a server!"));
        }
    }

    public class BotOwnerOnly : PreconditionAttribute
    {
        public BotOwnerOnly() { }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context,
                                                                        CommandInfo command, IServiceProvider services)
        {
            if (context.User.Id == Settings.Instance.LoadedConfig.AestheticalUid)
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError("You must be the bot owner to execute this command."));
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HideInHelp : Attribute
    {
        public HideInHelp() { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleEmoji : Attribute
    {
        public string Emoji = string.Empty;

        public ModuleEmoji(string emoji) => Emoji = emoji;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NotModifiable : Attribute
    {
        public NotModifiable() { }
    }
}
