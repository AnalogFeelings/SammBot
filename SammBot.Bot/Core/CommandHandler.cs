#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Interactions;
using SammBot.Bot.Extensions;
using SammBot.Bot.Services;

namespace SammBot.Bot.Core;

public class CommandHandler
{
    private DiscordShardedClient ShardedClient { get; set; }
    private IServiceProvider ServiceProvider { get; set; }
    private Logger BotLogger { get; set; }

    private AdminService AdminService { get; set; }
    private InteractionService InteractionService { get; set; }
    private EventLoggingService EventLoggingService { get; set; }

    public CommandHandler(DiscordShardedClient Client, InteractionService InteractionService, IServiceProvider Services, Logger Logger)
    {
        this.InteractionService = InteractionService;
        ShardedClient = Client;
        ServiceProvider = Services;
        BotLogger = Logger;
            
        AdminService = ServiceProvider.GetRequiredService<AdminService>();
        EventLoggingService = ServiceProvider.GetRequiredService<EventLoggingService>();
    }

    public async Task InitializeHandlerAsync()
    {
        await InteractionService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);

        ShardedClient.MessageReceived += OnMessageReceivedAsync;
        ShardedClient.InteractionCreated += HandleInteractionAsync;
            
        InteractionService.InteractionExecuted += OnInteractionExecutedAsync;

        AddEventHandlersAsync();
    }

    private async Task OnInteractionExecutedAsync(ICommandInfo SlashCommand, IInteractionContext Context, IResult Result)
    {
        try
        {
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

            if (!Result.IsSuccess)
            {
                string finalMessage = string.Empty;

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed((ShardedInteractionContext)Context);
                replyEmbed.Title = "\u26A0 An error has occurred.";
                replyEmbed.Color = new Color(255, 204, 77);

                switch (Result.Error)
                {
                    case InteractionCommandError.UnknownCommand:
                        replyEmbed.Title = "\u2139\uFE0F I didn't quite understand that...";
                        replyEmbed.Color = new Color(59, 136, 195);
                            
                        finalMessage = $"There is no command named like that!\nUse the `/help` command for a command list.";
                        break;
                    case InteractionCommandError.BadArgs:
                        finalMessage = $"You provided an incorrect number of parameters!\nUse the `/help " +
                                       $"{SlashCommand.Module.Name} {SlashCommand.Name}` command to see all of the parameters.";
                        break;
                    default:
                        finalMessage = Result.ErrorReason;
                        break;
                }

                replyEmbed.Description = finalMessage;

                await Context.Interaction.FollowupAsync(null, embed: replyEmbed.Build(), allowedMentions: allowedMentions);
            }
        }
        catch (Exception ex)
        {
            BotLogger.LogException(ex);
        }
    }
        
    private async Task OnMessageReceivedAsync(SocketMessage ReceivedMessage)
    {
        try
        {
            if (ReceivedMessage.Content.StartsWith($"<@{ShardedClient.CurrentUser.Id}>"))
            {
                MessageReference messageReference = new MessageReference(ReceivedMessage.Id, ReceivedMessage.Channel.Id, null, false);
                AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

                await ReceivedMessage.Channel.SendMessageAsync($"Hi! I'm **{SettingsManager.BOT_NAME}**!\n" + 
                                                               $"You can use `/help` to see a list of my available commands!", 
                    allowedMentions: allowedMentions, messageReference: messageReference);
            }
        }
        catch (Exception ex)
        {
            BotLogger.LogException(ex);
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction Interaction)
    {
        if (AdminService.ChangingConfig) return;

        ShardedInteractionContext context = new ShardedInteractionContext(ShardedClient, Interaction);

        if (SettingsManager.Instance.LoadedConfig.OnlyOwnerMode)
        {
            IApplication botApplication = await ShardedClient.GetApplicationInfoAsync();

            if (Interaction.User.Id != botApplication.Owner.Id) return;
        }
            
        BotLogger.Log(string.Format(SettingsManager.Instance.LoadedConfig.CommandLogFormat,
            Interaction.User.GetFullUsername(), Interaction.Channel.Name), LogSeverity.Debug);

        await InteractionService.ExecuteCommandAsync(context, ServiceProvider);
    }
        
    private void AddEventHandlersAsync()
    {
        ShardedClient.UserJoined += EventLoggingService.OnUserJoinedAsync;
        ShardedClient.UserLeft += EventLoggingService.OnUserLeftAsync;
            
        ShardedClient.MessageDeleted += EventLoggingService.OnMessageDeleted;
        ShardedClient.MessagesBulkDeleted += EventLoggingService.OnMessagesBulkDeleted;
        ShardedClient.MessageUpdated += EventLoggingService.OnMessageUpdated;
            
        ShardedClient.RoleCreated += EventLoggingService.OnRoleCreated;
        ShardedClient.RoleUpdated += EventLoggingService.OnRoleUpdated;

        ShardedClient.UserBanned += EventLoggingService.OnUserBanned;
        ShardedClient.UserUnbanned += EventLoggingService.OnUserUnbanned;
    }
}