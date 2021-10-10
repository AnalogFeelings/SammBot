using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Pastel;
using SammBotNET.Database;
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
        private readonly BlacklistedUsersDB BlacklistDatabase;
        private readonly CommandDB CommandDatabase;

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
            BlacklistDatabase = services.GetRequiredService<BlacklistedUsersDB>();
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
                    }
                    else await context.Channel.SendMessageAsync("**__Woops! There has been an error!__**\n"
                                                                + result.ErrorReason);
                }
            }
            catch (Exception ex)
            {
                BotLogger.Log(ex.Message.Pastel(System.Drawing.Color.IndianRed) + ex.StackTrace.Pastel(System.Drawing.Color.DarkRed));
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            SocketCommandContext context = new SocketCommandContext(DiscordClient, message);

            int argPos = 0;
            if (message.HasStringPrefix(GlobalConfig.Instance.LoadedConfig.BotPrefix, ref argPos) && !message.Author.IsBot)
            {
                if (BlacklistDatabase.BlacklistedUser.Any(x => x.userID == message.Author.Id))
                {
                    BlacklistedUser user = BlacklistDatabase.BlacklistedUser.SingleOrDefault(wew => wew.userID == message.Author.Id);
                    if (user.banType == "total")
                    {
                        await context.Channel.SendMessageAsync($"<@{message.Author.Id}>, you are blacklisted from using my commands!");
                        return;
                    }
                }
                CommandName = message.Content.Remove(0, GlobalConfig.Instance.LoadedConfig.BotPrefix.Length);
                BotLogger.Log($"Executing command \"{message.Content.Pastel("#bde0ff")}\". Channel: {message.Channel.Name.Pastel("#bde0ff")}. User: {message.Author.Username.Pastel("#bde0ff")}.".Pastel("#64daed"));
                await CommandsService.ExecuteAsync(
                    context: context,
                    argPos: argPos,
                    services: ServiceProvider);
            }
            else if (!message.HasStringPrefix(GlobalConfig.Instance.LoadedConfig.BotPrefix, ref argPos) && !message.Author.IsBot)
            {
                try
                {
                    if (BlacklistDatabase.BlacklistedUser.Any(x => x.userID == message.Author.Id)) return;

                    if (message.Content.Length < 15) return;

                    List<Phrase> phrases = await PhrasesDatabase.Phrase.ToListAsync();
                    if (message.Attachments.Count > 0 && message.Content.Length == 0) return;

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
                    BotLogger.Log(ex.Message.Pastel(System.Drawing.Color.IndianRed) + ex.StackTrace.Pastel(System.Drawing.Color.DarkRed));
                }
            }
        }
    }
}
