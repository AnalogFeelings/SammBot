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

		public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, Logger logger)
		{
			CommandsService = commands;
			DiscordClient = client;
			ServiceProvider = services;
			BotLogger = logger;

			DiscordClient.MessageReceived += HandleCommandAsync;
			CommandsService.CommandExecuted += OnCommandExecutedAsync;

			AdminService = services.GetRequiredService<AdminService>();
		}

		public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
		{
			if (!result.IsSuccess)
			{
				if (result.ErrorReason == "Unknown command.")
				{
					await context.Channel.SendMessageAsync($"Unknown command! Use the `{Settings.Instance.LoadedConfig.BotPrefix}help` command.");
				}
				else
				{
					await context.Channel.SendMessageAsync(":warning: **__Error executing command!__**\n" + result.ErrorReason);
				}
			}

			await Task.Delay(Settings.Instance.LoadedConfig.QueueWaitTime);

			MessageQueue.TryDequeue(out SocketMessage dequeuedMessage);
			ExecutingCommand = false;

			await HandleCommandAsync(dequeuedMessage);
		}

		public async Task HandleCommandAsync(SocketMessage messageParam)
		{
			if (AdminService.ChangingConfig) return;

			SocketUserMessage message = messageParam as SocketUserMessage;
			if (message == null) return;
			if (message.Author.IsBot) return;

			SocketCommandContext context = new SocketCommandContext(DiscordClient, message);

			int argPos = 0;
			if (message.Content.StartsWith($"<@{DiscordClient.CurrentUser.Id}>"))
			{
				await context.Channel.SendMessageAsync($"Hi! I'm **{Settings.Instance.LoadedConfig.BotName}**!\n" +
					$"My prefix is `{Settings.Instance.LoadedConfig.BotPrefix}`! " +
					$"You can use `{Settings.Instance.LoadedConfig.BotPrefix}help` to see a list of my available commands!");
			}
			else if (message.HasStringPrefix(Settings.Instance.LoadedConfig.BotPrefix, ref argPos))
			{
				if (message.Content.Length == Settings.Instance.LoadedConfig.BotPrefix.Length) return;
				if (ExecutingCommand)
				{
					MessageQueue.Enqueue(messageParam);
					await message.AddReactionAsync(new Emoji("⌛"));
					return;
				}

				ExecutingCommand = true;

				BotLogger.Log(string.Format(Settings.Instance.LoadedConfig.CommandLogFormat,
								message.Content, message.Channel.Name, message.Author.Username), LogLevel.Message);

				await CommandsService.ExecuteAsync(context, argPos, ServiceProvider);
			}
			else await CreateQuoteAsync(message, context);
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
					List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
					foreach (Phrase phrase in phrases)
					{
						if (Message.Content == phrase.Content) return;
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
