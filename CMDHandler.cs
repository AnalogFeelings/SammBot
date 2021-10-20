using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET
{
    public partial class CMDHandler
    {
        private readonly DiscordSocketClient DiscordClient;
        private readonly CommandService CommandsService;
        private readonly IServiceProvider ServiceProvider;
        private readonly Logger BotLogger;

        //Jesus christ this is the ugliest solution i've ever made for custom commands.
        private string CommandName;
        private readonly PhrasesDB PhrasesDatabase;
        private readonly CommandDB CommandDatabase;
        private readonly AdminService AdminService;

        public CMDHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, Logger logger)
        {
            CommandsService = commands;
            DiscordClient = client;
            ServiceProvider = services;
            BotLogger = logger;
            DiscordClient.MessageReceived += HandleCommandAsync;
            CommandsService.CommandExecuted += OnCommandExecutedAsync;

            PhrasesDatabase = services.GetRequiredService<PhrasesDB>();
            CommandDatabase = services.GetRequiredService<CommandDB>();
            AdminService = services.GetRequiredService<AdminService>();
        }

        public async Task OnCommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            try
            {
                if (!string.IsNullOrEmpty(result?.ErrorReason))
                {
                    if (result.ErrorReason == "Unknown command.")
                    {
                        List<CustomCommand> cmds = await CommandDatabase.CustomCommand.ToListAsync();
                        foreach (CustomCommand cmd in cmds)
                        {
                            if (cmd.name == CommandName)
                            {
                                await context.Channel.SendMessageAsync(cmd.reply);
                                return;
                            }
                        }
                        await context.Channel.SendMessageAsync("Unknown command! Use the s.help command.");
                        return;
                    }
                    if (result.ErrorReason != "Execution succesful.")
                    {
                        await context.Channel.SendMessageAsync("**__Error executing command!__**\n"
                                                                + result.ErrorReason);
                    }
                }
            }
            catch (Exception ex)
            {
                BotLogger.LogException(ex);
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (AdminService.ChangingConfig) return;

            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return;

            SocketCommandContext context = new(DiscordClient, message);

            int argPos = 0;
            if (message.HasStringPrefix(GlobalConfig.Instance.LoadedConfig.BotPrefix, ref argPos))
            {
                CommandName = message.Content.Remove(0, GlobalConfig.Instance.LoadedConfig.BotPrefix.Length);
                CommandName = CommandName.Split()[0];

                BotLogger.Log(LogLevel.Message, string.Format(GlobalConfig.Instance.LoadedConfig.CommandLogFormat,
                                                message.Content, message.Channel.Name, message.Author.Username));

                await CommandsService.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: ServiceProvider);
            }
            else
            {
                try
                {
                    if (message.Content.Length < 20 || message.Content.Length > 64) return;
                    if (message.Attachments.Count > 0 && message.Content.Length == 0) return;
                    if (GlobalConfig.Instance.UrlRegex.IsMatch(message.Content)) return;
                    if (GlobalConfig.Instance.LoadedConfig.BannedPrefixes.Any(x => message.Content.StartsWith(x))) return;

                    List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();

                    foreach (Phrase phrase in phrases)
                    {
                        if (message.Content == phrase.content)
                        {
                            return;
                        }
                    }

                    await PhrasesDatabase.AddAsync(new Phrase
                    {
                        content = message.Content,
                        authorID = message.Author.Id,
                        serverID = context.Guild.Id
                    });
                    await PhrasesDatabase.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    BotLogger.LogException(ex);
                }
            }
        }
    }
}
