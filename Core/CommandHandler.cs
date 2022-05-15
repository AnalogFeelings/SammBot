using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Core
{
	public partial class CommandHandler
	{
		public DiscordSocketClient DiscordClient { get; set; }
		public IServiceProvider ServiceProvider { get; set; }
		public Logger BotLogger { get; set; }

		public AdminService AdminService { get; set; }
		public CommandService CommandsService { get; set; }

		private ConcurrentQueue<SocketMessage> MessageQueue = new ConcurrentQueue<SocketMessage>();
		private bool ExecutingCommand = false;

		public CommandHandler(DiscordSocketClient Client, CommandService Commands, IServiceProvider Services, Logger Logger)
		{
			CommandsService = Commands;
			DiscordClient = Client;
			ServiceProvider = Services;
			BotLogger = Logger;

			DiscordClient.MessageReceived += HandleCommandAsync;
			CommandsService.CommandExecuted += OnCommandExecutedAsync;

			AdminService = Services.GetRequiredService<AdminService>();
		}

		public async Task OnCommandExecutedAsync(Optional<CommandInfo> Command, ICommandContext Context, IResult Result)
		{
			if (!Result.IsSuccess)
			{
				if (Result.ErrorReason == "Unknown command.")
				{
					await Context.Channel.SendMessageAsync($"Unknown command! Use the `{Settings.Instance.LoadedConfig.BotPrefix}help` command.");
				}
				else
				{
					await Context.Channel.SendMessageAsync(":warning: **__Error executing command!__**\n" + Result.ErrorReason);
				}
			}

			await Task.Delay(Settings.Instance.LoadedConfig.QueueWaitTime);

			MessageQueue.TryDequeue(out SocketMessage dequeuedMessage);
			ExecutingCommand = false;

			await HandleCommandAsync(dequeuedMessage);
		}

		public async Task HandleCommandAsync(SocketMessage ReceivedMessage)
		{
			if (AdminService.ChangingConfig) return;

			SocketUserMessage TargetMessage = ReceivedMessage as SocketUserMessage;
			if (TargetMessage == null) return;
			if (TargetMessage.Author.IsBot) return;

			SocketCommandContext Context = new SocketCommandContext(DiscordClient, TargetMessage);

			int ArgumentPosition = 0;
			if (TargetMessage.Content.StartsWith($"<@{DiscordClient.CurrentUser.Id}>"))
			{
				await Context.Channel.SendMessageAsync($"Hi! I'm **{Settings.Instance.LoadedConfig.BotName}**!\n" +
						$"My prefix is `{Settings.Instance.LoadedConfig.BotPrefix}`! " +
						$"You can use `{Settings.Instance.LoadedConfig.BotPrefix}help` to see a list of my available commands!");
			}
			else if (TargetMessage.HasStringPrefix(Settings.Instance.LoadedConfig.BotPrefix, ref ArgumentPosition))
			{
				if (TargetMessage.Content.Length == Settings.Instance.LoadedConfig.BotPrefix.Length) return;
				if (ExecutingCommand)
				{
					MessageQueue.Enqueue(ReceivedMessage);
					await TargetMessage.AddReactionAsync(new Emoji("⌛"));
					return;
				}

				ExecutingCommand = true;

				BotLogger.Log(string.Format(Settings.Instance.LoadedConfig.CommandLogFormat,
								TargetMessage.Content, TargetMessage.Channel.Name, TargetMessage.Author.Username), LogSeverity.Information);

				await CommandsService.ExecuteAsync(Context, ArgumentPosition, ServiceProvider);
			}
			else await CreateQuoteAsync(TargetMessage, Context);
		}

		public async Task CreateQuoteAsync(SocketMessage Message, SocketCommandContext Context)
		{
			try
			{
				if (Message.Content.Length < 20 || Message.Content.Length > 64) return;
				if (Message.Attachments.Count > 0 && Message.Content.Length == 0) return;
				if (Message.MentionedUsers.Count > 0) return;
				if (Settings.Instance.UrlRegex.IsMatch(Message.Content)) return;
				if (Settings.Instance.LoadedConfig.BannedPrefixes.Any(x => Message.Content.StartsWith(x))) return;

				using (PhrasesDB PhrasesDatabase = new PhrasesDB())
				{
					List<Phrase> QuoteList = await PhrasesDatabase.Phrase.ToListAsync();
					foreach (Phrase Quote in QuoteList)
					{
						if (Message.Content == Quote.Content) return;
					}

					await PhrasesDatabase.AddAsync(new Phrase
					{
						Content = Message.Content,
						AuthorId = Message.Author.Id,
						ServerId = Context.Guild.Id,
						CreatedAt = Message.Timestamp.ToUnixTimeSeconds()
					});
					await PhrasesDatabase.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				BotLogger.LogException(ex);
			}
		}
	}
}
