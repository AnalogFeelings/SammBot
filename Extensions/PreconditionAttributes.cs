using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SammBotNET.Extensions
{
	public class MustRunInGuild : PreconditionAttribute
	{
		public MustRunInGuild() { }

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context,
																		CommandInfo Command, IServiceProvider Services)
		{
			if (Context.User is SocketGuildUser)
				return Task.FromResult(PreconditionResult.FromSuccess());

			return Task.FromResult(PreconditionResult.FromError("You must execute this command in a server!"));
		}
	}

	public class BotOwnerOnly : PreconditionAttribute
	{
		public BotOwnerOnly() { }

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext Context,
																		CommandInfo Command, IServiceProvider Services)
		{
			if (Context.User.Id == Settings.Instance.LoadedConfig.OwnerUserId)
				return Task.FromResult(PreconditionResult.FromSuccess());

			return Task.FromResult(PreconditionResult.FromError("You must be the bot owner to execute this command."));
		}
	}
}
