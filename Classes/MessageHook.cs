using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace SammBotNET.Classes
{
	public abstract class MessageHook
	{
		public SocketUserMessage Message { get; set; }
		public SocketCommandContext Context { get; set; }
		public Logger BotLogger { get; set; }
		public DiscordSocketClient Client { get; set; }

		public abstract Task ExecuteHook();

		protected async Task<IUserMessage> ReplyAsync(string message = null, bool isTTS = false, Embed embed = null,
														RequestOptions options = null, AllowedMentions allowedMentions = null,
														MessageReference messageReference = null, MessageComponent components = null,
														ISticker[] stickers = null, Embed[] embeds = null)
		{
			return await Message.Channel.SendMessageAsync(message, isTTS, embed,
															options, allowedMentions,
															messageReference, components,
															stickers, embeds).ConfigureAwait(continueOnCapturedContext: false);
		}
	}
}
