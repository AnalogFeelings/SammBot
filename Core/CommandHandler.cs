using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

		private List<MessageHook> HookList = new List<MessageHook>();

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

			HookList = ReflectionEnumerator.GetChildrenOfType<MessageHook>()
				.Where(x => x.GetType().GetCustomAttribute(typeof(RegisterHook), false) != null).ToList();
		}

		public async Task OnCommandExecutedAsync(Optional<CommandInfo> Command, ICommandContext Context, IResult Result)
		{
			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

			if (!Result.IsSuccess)
			{
				if (Result.ErrorReason == "Unknown command.")
				{
					await Context.Channel.SendMessageAsync($"Unknown command! Use the `{Settings.Instance.LoadedConfig.BotPrefix}help` command.",
						allowedMentions: AllowedMentions, messageReference: Reference);
				}
				else
				{
					await Context.Channel.SendMessageAsync(":warning: **__Error executing command!__**\n" + Result.ErrorReason,
						allowedMentions: AllowedMentions, messageReference: Reference);
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
				MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
				AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

				await Context.Channel.SendMessageAsync($"Hi! I'm **{Settings.Instance.LoadedConfig.BotName}**!\n" +
						$"My prefix is `{Settings.Instance.LoadedConfig.BotPrefix}`! " +
						$"You can use `{Settings.Instance.LoadedConfig.BotPrefix}help` to see a list of my available commands!",
						allowedMentions: AllowedMentions, messageReference: Reference);
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

			if (!TargetMessage.Content.StartsWith(Settings.Instance.LoadedConfig.BotPrefix))
			{
				foreach (MessageHook Hook in HookList)
				{
					Hook.Message = TargetMessage;
					Hook.Context = Context;
					Hook.BotLogger = BotLogger;
					Hook.Client = DiscordClient;

					_ = Task.Run(() => Hook.ExecuteHook());
				}
			}
		}
	}
}
