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

using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Library;
using SammBot.Library.Attributes;
using SammBot.Library.Extensions;
using SammBot.Library.Models;
using SammBot.Library.Preconditions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules;

[PrettyName("Help")]
public class HelpModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly InteractionService _interactionService;

    public HelpModule(IServiceProvider provider)
    {
        _interactionService = provider.GetRequiredService<InteractionService>();
    }

    [SlashCommand("help", "Provides all commands and modules available.")]
    [DetailedDescription("Provides a list of all the commands and modules available.")]
    [RateLimit(1, 3)]
    [HideInHelp]
    public async Task<RuntimeResult> HelpAsync
    (
        [Summary("Module", "Leave empty to list all modules. Add a command name to list help for that command.")]
        string? module = null
    )
    {
        // Hoh boy, this is QUITE a bumpy ride. Be ready!
        await DeferAsync();

        EmbedBuilder replyEmbed;

        // User passed a module name and probably command name too.
        if (module != null)
        {
            // Split the name.
            string[] splittedName = module.Split(' ');

            // If the splittedName array contains 1 element, the user is looking for a module.
            if (splittedName.Length == 1)
            {
                ModuleInfo? moduleInfo = _interactionService.Modules.SingleOrDefault(x => x.Name == module || x.SlashGroupName == module);

                if (moduleInfo == default(ModuleInfo))
                    return ExecutionResult.FromError($"The module \"{module}\" doesn't exist.");

                // Get the module emoji, if it has any.
                ModuleEmoji? moduleEmoji = moduleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
                string stringifiedEmoji = moduleEmoji != default(ModuleEmoji) ? moduleEmoji.Emoji + " " : string.Empty;

                // It's impossible to have more than one PrettyName attribute, so use Single().
                PrettyName moduleName = moduleInfo.Attributes.OfType<PrettyName>().Single();

                string moduleHeader = $"**{stringifiedEmoji}{moduleName.Name}**\n" +
                                      $"{moduleInfo.Description}\n" +
                                      $"**Syntax**: `/{moduleInfo.SlashGroupName} <Command Name>`";

                replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

                replyEmbed.Title = "\U0001f4c2 Module Help";
                replyEmbed.Description = moduleHeader;
                replyEmbed.Color = new Color(85, 172, 238);

                // Check attributes of containing commands. If the command doesn't pass the check, the command
                // doesn't get added to the output embed.
                bool foundCommand = false;
                foreach (SlashCommandInfo command in moduleInfo.SlashCommands)
                {
                    if (command.Attributes.Any(x => x is HideInHelp)) continue;

                    replyEmbed.AddField(command.Name, $"{(string.IsNullOrWhiteSpace(command.Description) ? "No summary." : command.Description)}", true);
                    foundCommand = true;
                }

                // Report an error if no command passed the check.
                if (!foundCommand)
                    return ExecutionResult.FromError($"The module \"{moduleInfo.Name}\" has no commands, or you don't have enough permissions to see them.");
            }
            else // splittedName array contains more than 1 element, user is looking for a command.
            {
                SlashCommandInfo? searchResult = _interactionService.SlashCommands.FirstOrDefault(x => x.Module.SlashGroupName == splittedName[0]
                                                                                                       && x.Name == splittedName[1]);

                if (searchResult == null)
                    return ExecutionResult.FromError($"There is no command named \"{module}\". Check your spelling.");

                replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

                replyEmbed.Title = "\U0001f4c4 Command Help";
                replyEmbed.Color = new Color(204, 214, 221);

                // Get command description, if any.
                DetailedDescription? commandDescription = searchResult.Attributes.FirstOrDefault(x => x is DetailedDescription) as DetailedDescription;
                string formattedDescription;

                if (commandDescription != default(DetailedDescription) && !string.IsNullOrEmpty(commandDescription.Description))
                    formattedDescription = commandDescription.Description;
                else
                    formattedDescription = "No description.";

                // Check if the command needs to be executed in guild or not.
                RequireContextAttribute? requiredContexts = searchResult.Preconditions.FirstOrDefault(x => x is RequireContextAttribute) as RequireContextAttribute;

                bool requiresGuild = requiredContexts != default(RequireContextAttribute) &&
                                     requiredContexts.Contexts.HasFlag(ContextType.Guild) &&
                                     !requiredContexts.Contexts.HasFlag(ContextType.DM);

                replyEmbed.AddField("\U0001f3f7\uFE0F Name", searchResult.Name, true);
                replyEmbed.AddField("\U0001f5c3\uFE0F Group", searchResult.Module.SlashGroupName, true);
                replyEmbed.AddField("\U0001f575\uFE0F Usable in DMs", (!requiresGuild).ToYesNo(), true);
                replyEmbed.AddField("\U0001f4cb Description", formattedDescription);

                // Get command cooldown information.
                RateLimit? commandRateLimit = searchResult.Preconditions.FirstOrDefault(x => x is RateLimit) as RateLimit;
                string rateLimitString;

                if (commandRateLimit == default(RateLimit))
                {
                    rateLimitString = "This command has no cooldown.";
                }
                else
                {
                    rateLimitString = $"Cooldown of **{commandRateLimit.TimeoutDuration}** second(s).\n" +
                                      $"Triggered after using the command **{commandRateLimit.RequestLimit}** time(s).";
                }

                replyEmbed.AddField("\u23F1\uFE0FCooldown", rateLimitString);

                // Append all parameters to output embed.
                string commandParameters = "A parameter with a `*` symbol is optional.\n";
                foreach (SlashCommandParameterInfo parameterInfo in searchResult.Parameters)
                {
                    string typeName = parameterInfo.ParameterType.Name;
                    string additionalSymbols = string.Empty;
                    string defaultValue = "No default.";
                    string summaryString = "No summary.";

                    if (!parameterInfo.IsRequired) additionalSymbols += "*";
                    if (parameterInfo.DefaultValue != null) defaultValue = parameterInfo.DefaultValue.ToString()!;
                    if (!string.IsNullOrEmpty(parameterInfo.Description)) summaryString = parameterInfo.Description;

                    commandParameters += $"[**{typeName}**{additionalSymbols}] `{parameterInfo.Name}`\n";
                    commandParameters += $"• **Summary**: {summaryString}\n";
                    commandParameters += $"• **Default**: {defaultValue}\n";
                }

                replyEmbed.AddField("\U0001f4c3 Parameters", searchResult.Parameters.Count == 0 ? "No parameters." : commandParameters);
            }
        }
        else // ModuleName is null, user is asking to see all the available modules.
        {
            string replyDescription = $"These are all the modules available to you.\n" +
                                      $"Use `/help <Group Name>` to see its commands.";

            replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

            replyEmbed.Title = "\U0001f4c1 Module List";
            replyEmbed.Description = replyDescription;
            replyEmbed.Color = new Color(85, 172, 238);

            foreach (ModuleInfo moduleInfo in _interactionService.Modules)
            {
                bool foundCommand = false;

                // Check attributes of each command. If no commands pass this check, the module
                // doesn't get added to the output embed.
                foreach (SlashCommandInfo command in moduleInfo.SlashCommands)
                {
                    if (command.Attributes.Any(x => x is HideInHelp)) continue;

                    foundCommand = true;
                }

                if (foundCommand)
                {
                    // Get module emoji, if any.
                    ModuleEmoji? moduleEmoji = moduleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
                    string stringifiedEmoji = moduleEmoji != null ? moduleEmoji.Emoji + " " : string.Empty;

                    // It's impossible to have more than one PrettyName attribute, so use Single().
                    PrettyName moduleName = moduleInfo.Attributes.OfType<PrettyName>().Single();

                    // Build the embed field.
                    string moduleHeader = $"{stringifiedEmoji}{moduleName.Name}\n" +
                                          $"(Group: `{moduleInfo.SlashGroupName}`)";
                    string moduleDescription = string.IsNullOrEmpty(moduleInfo.Description) ? "No description." : moduleInfo.Description;

                    replyEmbed.AddField(moduleHeader, moduleDescription, true);
                }
            }
        }

        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: Constants.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }
}