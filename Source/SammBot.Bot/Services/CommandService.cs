#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using AnalogFeelings.Matcha;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Bot.Settings;
using SammBot.Library;
using SammBot.Library.Extensions;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace SammBot.Bot.Services;

public class CommandService
{
    private DiscordShardedClient ShardedClient { get; }
    private IServiceProvider ServiceProvider { get; }
    private MatchaLogger Logger { get; }

    private InteractionService InteractionService { get; }
    private EventLoggingService EventLoggingService { get; }

    public CommandService(IServiceProvider services)
    {
        ServiceProvider = services;

        InteractionService = ServiceProvider.GetRequiredService<InteractionService>();
        ShardedClient = ServiceProvider.GetRequiredService<DiscordShardedClient>();
        Logger = ServiceProvider.GetRequiredService<MatchaLogger>();
        EventLoggingService = ServiceProvider.GetRequiredService<EventLoggingService>();
    }

    public async Task InitializeHandlerAsync()
    {
        await InteractionService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);

        ShardedClient.InteractionCreated += HandleInteractionAsync;

        InteractionService.InteractionExecuted += OnInteractionExecutedAsync;

        AddEventHandlersAsync();
    }

    private async Task OnInteractionExecutedAsync(ICommandInfo slashCommand, IInteractionContext context, IResult result)
    {
        try
        {
            if (!result.IsSuccess)
            {
                string finalMessage;

                EmbedBuilder replyEmbed = new EmbedBuilder().BuildErrorEmbed((ShardedInteractionContext)context);

                switch (result.Error)
                {
                    case InteractionCommandError.BadArgs:
                        finalMessage = $"You provided an incorrect number of parameters!\nUse the `/help " +
                                       $"{slashCommand.Module.Name} {slashCommand.Name}` command to see all of the parameters.";
                        break;
                    default:
                        finalMessage = result.ErrorReason;
                        break;
                }

                replyEmbed.Description = finalMessage;

                if (context.Interaction.HasResponded)
                    await context.Interaction.FollowupAsync(embed: replyEmbed.Build(), ephemeral: true, allowedMentions: Constants.AllowOnlyUsers);
                else
                    await context.Interaction.RespondAsync(embed: replyEmbed.Build(), ephemeral: true, allowedMentions: Constants.AllowOnlyUsers);
            }
        }
        catch (Exception ex)
        {
            await Logger.LogAsync(LogSeverity.Error, "An exception occurred during post-execution handling: {0}", ex);
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        ShardedInteractionContext context = new ShardedInteractionContext(ShardedClient, interaction);

        if (SettingsManager.Instance.LoadedConfig.OnlyOwnerMode)
        {
            IApplication botApplication = await ShardedClient.GetApplicationInfoAsync();

            if (interaction.User.Id != botApplication.Owner.Id) return;
        }

        string formattedLog = SettingsManager.Instance.LoadedConfig.CommandLogFormat.Replace("%username%", interaction.User.GetFullUsername())
                                             .Replace("%channelname%", interaction.Channel.Name);

        await Logger.LogAsync(LogSeverity.Debug, formattedLog);

        await InteractionService.ExecuteCommandAsync(context, ServiceProvider);
    }

    private void AddEventHandlersAsync()
    {
        ShardedClient.UserJoined += EventLoggingService.OnUserJoinedAsync;
        ShardedClient.UserLeft += EventLoggingService.OnUserLeftAsync;

        ShardedClient.MessageDeleted += EventLoggingService.OnMessageDeleted;
        ShardedClient.MessagesBulkDeleted += EventLoggingService.OnMessagesBulkDeleted;

        ShardedClient.RoleCreated += EventLoggingService.OnRoleCreated;
        ShardedClient.RoleUpdated += EventLoggingService.OnRoleUpdated;

        ShardedClient.UserBanned += EventLoggingService.OnUserBanned;
        ShardedClient.UserUnbanned += EventLoggingService.OnUserUnbanned;
    }
}