using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SammBotNET.Core
{
    public class CommandHandler
    {
        private DiscordSocketClient DiscordClient { get; set; }
        private IServiceProvider ServiceProvider { get; set; }
        private Logger BotLogger { get; set; }

        private AdminService AdminService { get; set; }
        private CommandService CommandsService { get; set; }

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
            try
            {
                MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                if (!Result.IsSuccess)
                {
                    string FinalMessage = string.Empty;

                    EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed((SocketCommandContext)Context);
                    ReplyEmbed.Title = "\u26A0 An error has occurred.";
                    ReplyEmbed.Color = new Color(255, 204, 77);

                    switch (Result.ErrorReason)
                    {
                        case "Unknown command.":
                            FinalMessage = $"Unknown command! Use the `{Settings.Instance.LoadedConfig.BotPrefix}help` command for a command list.";
                            break;
                        case "The input text has too few parameters.":
                            FinalMessage = $"You didn't provide enough required parameters!\nUse the `{Settings.Instance.LoadedConfig.BotPrefix}help " +
                                $"{Command.Value.Module.Group} {Command.Value.Name}` command to see all of the required parameters.";
                            break;
                        case "The input text has too many parameters.":
                            FinalMessage = $"You provided too many parameters!\nUse the `{Settings.Instance.LoadedConfig.BotPrefix}help " +
                                $"{Command.Value.Module.Group} {Command.Value.Name}` command to see all of the required parameters.";
                            break;
                        default:
                            FinalMessage = Result.ErrorReason;
                            break;
                    }

                    ReplyEmbed.Description = FinalMessage;

                    await Context.Channel.SendMessageAsync(null, embed: ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
                }
            }
            catch (Exception ex)
            {
                BotLogger.LogException(ex);
            }
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

                await Context.Channel.SendMessageAsync($"Hi! I'm **{Settings.BOT_NAME}**!\n" +
                        $"My prefix is `{Settings.Instance.LoadedConfig.BotPrefix}`! " +
                        $"You can use `{Settings.Instance.LoadedConfig.BotPrefix}help` to see a list of my available commands!",
                        allowedMentions: AllowedMentions, messageReference: Reference);
            }
            else if (TargetMessage.HasStringPrefix(Settings.Instance.LoadedConfig.BotPrefix, ref ArgumentPosition))
            {
                if (TargetMessage.Content.Length == Settings.Instance.LoadedConfig.BotPrefix.Length) return;

                BotLogger.Log(string.Format(Settings.Instance.LoadedConfig.CommandLogFormat,
                                TargetMessage.Content, TargetMessage.Channel.Name, TargetMessage.Author.Username), LogSeverity.Information);

                await CommandsService.ExecuteAsync(Context, ArgumentPosition, ServiceProvider);
            }
        }
    }
}
