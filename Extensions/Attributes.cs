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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class HideInHelp : Attribute
    {
        public HideInHelp() { }
    }
}
