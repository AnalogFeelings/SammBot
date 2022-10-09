using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace SammBot.Bot.Core
{
    public class CommandHandler
    {
        private DiscordShardedClient ShardedClient { get; set; }
        private IServiceProvider ServiceProvider { get; set; }
        private Logger BotLogger { get; set; }

        private AdminService AdminService { get; set; }
        private CommandService CommandsService { get; set; }

        public CommandHandler(DiscordShardedClient Client, CommandService Commands, IServiceProvider Services, Logger Logger)
        {
            CommandsService = Commands;
            ShardedClient = Client;
            ServiceProvider = Services;
            BotLogger = Logger;

            ShardedClient.MessageReceived += HandleCommandAsync;
            CommandsService.CommandExecuted += OnCommandExecutedAsync;

            AdminService = Services.GetRequiredService<AdminService>();
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> Command, ICommandContext Context, IResult Result)
        {
            try
            {
                MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                if (!Result.IsSuccess)
                {
                    string finalMessage = string.Empty;

                    EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed((SocketCommandContext)Context);
                    replyEmbed.Title = "\u26A0 An error has occurred.";
                    replyEmbed.Color = new Color(255, 204, 77);

                    switch (Result.ErrorReason)
                    {
                        case "Unknown command.":
                            finalMessage = $"Unknown command! Use the `{Settings.Instance.LoadedConfig.BotPrefix}help` command for a command list.";
                            break;
                        case "The input text has too few parameters.":
                            finalMessage = $"You didn't provide enough required parameters!\nUse the `{Settings.Instance.LoadedConfig.BotPrefix}help " +
                                $"{Command.Value.Module.Group} {Command.Value.Name}` command to see all of the required parameters.";
                            break;
                        case "The input text has too many parameters.":
                            finalMessage = $"You provided too many parameters!\nUse the `{Settings.Instance.LoadedConfig.BotPrefix}help " +
                                $"{Command.Value.Module.Group} {Command.Value.Name}` command to see all of the required parameters.";
                            break;
                        default:
                            finalMessage = Result.ErrorReason;
                            break;
                    }

                    replyEmbed.Description = finalMessage;

                    await Context.Channel.SendMessageAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
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

            SocketUserMessage targetMessage = ReceivedMessage as SocketUserMessage;
            if (targetMessage == null) return;
            if (targetMessage.Author.IsBot) return;

            ShardedCommandContext context = new ShardedCommandContext(ShardedClient, targetMessage);

            int argumentPosition = 0;
            if (targetMessage.Content.StartsWith($"<@{ShardedClient.CurrentUser.Id}>"))
            {
                MessageReference messageReference = new MessageReference(context.Message.Id, context.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                await context.Channel.SendMessageAsync($"Hi! I'm **{Settings.BOT_NAME}**!\n" +
                        $"My prefix is `{Settings.Instance.LoadedConfig.BotPrefix}`! " +
                        $"You can use `{Settings.Instance.LoadedConfig.BotPrefix}help` to see a list of my available commands!",
                        allowedMentions: allowedMentions, messageReference: messageReference);
            }
            else if (targetMessage.HasStringPrefix(Settings.Instance.LoadedConfig.BotPrefix, ref argumentPosition))
            {
                if (targetMessage.Content.Length == Settings.Instance.LoadedConfig.BotPrefix.Length) return;

                BotLogger.Log(string.Format(Settings.Instance.LoadedConfig.CommandLogFormat,
                                targetMessage.Content, targetMessage.Channel.Name, targetMessage.Author.Username), LogSeverity.Information);

                await CommandsService.ExecuteAsync(context, argumentPosition, ServiceProvider);
            }
        }
    }
}
