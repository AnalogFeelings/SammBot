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
using SammBot.Library;
using SammBot.Library.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using SammBot.Library.Services;

namespace SammBot.Services;

/// <summary>
/// Provides handling for incoming interactions.
/// </summary>
public class CommandService : ICommandService
{
    private readonly DiscordShardedClient _shardedClient;
    private readonly IServiceProvider _serviceProvider;
    private readonly MatchaLogger _logger;
    private readonly InteractionService _interactionService;
    private readonly EventLoggingService _eventLoggingService;
    private readonly SettingsService _settingsService;

    /// <summary>
    /// Creates a new <see cref="CommandService"/>.
    /// </summary>
    /// <param name="services">The current active service provider.</param>
    public CommandService(IServiceProvider services)
    {
        _serviceProvider = services;

        _interactionService = _serviceProvider.GetRequiredService<InteractionService>();
        _shardedClient = _serviceProvider.GetRequiredService<DiscordShardedClient>();
        _logger = _serviceProvider.GetRequiredService<MatchaLogger>();
        _eventLoggingService = _serviceProvider.GetRequiredService<EventLoggingService>();
        _settingsService = _serviceProvider.GetRequiredService<SettingsService>();
    }

    /// <summary>
    /// Initializes events and adds the assembly's modules to Discord.NET's registry.
    /// </summary>
    public async Task InitializeHandlerAsync()
    {
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        _shardedClient.InteractionCreated += HandleInteractionAsync;
        _interactionService.InteractionExecuted += OnInteractionExecutedAsync;

        _shardedClient.UserJoined += _eventLoggingService.OnUserJoinedAsync;
        _shardedClient.UserLeft += _eventLoggingService.OnUserLeftAsync;

        _shardedClient.MessageDeleted += _eventLoggingService.OnMessageDeleted;
        _shardedClient.MessagesBulkDeleted += _eventLoggingService.OnMessagesBulkDeleted;

        _shardedClient.RoleCreated += _eventLoggingService.OnRoleCreated;
        _shardedClient.RoleUpdated += _eventLoggingService.OnRoleUpdated;

        _shardedClient.UserBanned += _eventLoggingService.OnUserBanned;
        _shardedClient.UserUnbanned += _eventLoggingService.OnUserUnbanned;
    }

    /// <summary>
    /// Raised when an interaction finishes executing.
    /// Handles displaying errors to the user ephemerally.
    /// </summary>
    /// <param name="slashCommand">The information for the executed interaction.</param>
    /// <param name="context">The executed interaction's context.</param>
    /// <param name="result">The result of the executed interaction.</param>
    private async Task OnInteractionExecutedAsync(ICommandInfo slashCommand, IInteractionContext context, IResult result)
    {
        try
        {
            if (!result.IsSuccess)
            {
                EmbedBuilder replyEmbed = new EmbedBuilder().BuildErrorEmbed((ShardedInteractionContext)context);

                replyEmbed.Description = result.Error switch
                {
                    InteractionCommandError.BadArgs => $"You provided an incorrect number of parameters!\nUse the `/help " +
                                                       $"{slashCommand.Module.Name} {slashCommand.Name}` command to see all of the parameters.",
                    _ => result.ErrorReason
                };
                
                if (context.Interaction.HasResponded)
                    await context.Interaction.FollowupAsync(embed: replyEmbed.Build(), ephemeral: true, allowedMentions: Constants.AllowOnlyUsers);
                else
                    await context.Interaction.RespondAsync(embed: replyEmbed.Build(), ephemeral: true, allowedMentions: Constants.AllowOnlyUsers);
            }
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(LogSeverity.Error, "An exception occurred during post-execution handling: {0}", ex);
        }
    }

    /// <summary>
    /// Raised when an interaction is requested.
    /// Handles creating the interaction context and executing it.
    /// </summary>
    /// <param name="interaction">The interaction sent by Discord.</param>
    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        ShardedInteractionContext context = new ShardedInteractionContext(_shardedClient, interaction);

        if (_settingsService.Settings!.OnlyOwnerMode)
        {
            IApplication botApplication = await _shardedClient.GetApplicationInfoAsync();

            if (interaction.User.Id != botApplication.Owner.Id) return;
        }

#if DEBUG
        Dictionary<string, object?> template = new Dictionary<string, object?>()
        {
            ["username"] = interaction.User.GetFullUsername(),
            ["channelname"] = interaction.Channel.Name
        };
        string formattedLog = _settingsService.Settings!.CommandLogFormat.TemplateReplace(template);

        await _logger.LogAsync(LogSeverity.Debug, formattedLog);
#endif

        await _interactionService.ExecuteCommandAsync(context, _serviceProvider);
    }
}