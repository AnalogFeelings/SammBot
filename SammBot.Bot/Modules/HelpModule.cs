using Discord;
using Discord.Commands;
using SammBot.Bot.Classes;
using SammBot.Bot.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBot.Bot.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        public CommandService CommandService { get; set; }

        [Command("help")]
        [Summary("Provides all commands and modules available.")]
        [FullDescription("Provides a list of all the commands and modules available.")]
        [RateLimit(1, 3)]
        [HideInHelp]
        public async Task<RuntimeResult> HelpAsync([Remainder] string ModuleName = null)
        {
            // Hoh boy, this is QUITE a bumpy ride. Be ready!
            string botPrefix = Settings.Instance.LoadedConfig.BotPrefix;
            
            MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
            AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

            EmbedBuilder replyEmbed = null;

            // User passed a module name and probably command name too.
            if (ModuleName != null)
            {
                // Split the name.
                string[] splittedName = ModuleName.Split(' ');

                // If the splittedName array contains 1 element, the user is looking for a module.
                if (splittedName.Length == 1)
                {
                    ModuleInfo moduleInfo = CommandService.Modules.SingleOrDefault(x => x.Name == ModuleName || x.Group == ModuleName);

                    if (moduleInfo == default(ModuleInfo))
                        return ExecutionResult.FromError($"The module \"{ModuleName}\" doesn't exist.");

                    // Get the module emoji, if it has any.
                    ModuleEmoji moduleEmoji = moduleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
                    string stringifiedEmoji = moduleEmoji != default(ModuleEmoji) ? moduleEmoji.Emoji + " " : string.Empty;

                    string moduleHeader = $"**{stringifiedEmoji}{moduleInfo.Name}**\n" +
                                          $"{moduleInfo.Summary}\n" +
                                          $"**Syntax**: `{botPrefix}{moduleInfo.Group} <Command Name>`";

                    replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Module Help", moduleHeader);

                    // Check permissions of containing commands. If the command doesn't pass the check, the command
                    // doesn't get added to the output embed.
                    bool foundCommand = false;
                    foreach (CommandInfo command in moduleInfo.Commands)
                    {
                        if (command.Attributes.Any(x => x is HideInHelp)) continue;

                        PreconditionResult preconditionResult = await command.CheckPreconditionsAsync(Context);

                        if (!preconditionResult.IsSuccess) continue;

                        replyEmbed.AddField(command.Name, $"{(string.IsNullOrWhiteSpace(command.Summary) ? "No summary." : command.Summary)}", true);
                        foundCommand = true;
                    }

                    // Report an error if no command passed the check.
                    if (!foundCommand)
                        return ExecutionResult.FromError($"The module \"{moduleInfo.Name}\" has no commands, or you don't have enough permissions to see them.");
                }
                else // splittedName array contains more than 1 element, user is looking for a command.
                {
                    SearchResult searchResult = CommandService.Search(Context, ModuleName);

                    if (!searchResult.IsSuccess)
                        return ExecutionResult.FromError($"There is no command named \"{ModuleName}\". Check your spelling.");

                    CommandMatch commandMatch = searchResult.Commands.SingleOrDefault(x => x.Command.Aliases.Contains(ModuleName));

                    // Very ugly double check.
                    if (commandMatch.Command == null)
                        return ExecutionResult.FromError($"There is no command named \"{ModuleName}\". Check your spelling.");

                    CommandInfo command = commandMatch.Command;

                    List<string> processedAliases = command.Aliases.ToList();
                    processedAliases.RemoveAt(0); // Remove the command name itself from the aliases list.

                    // Remove group name from aliases list.
                    for (int i = 0; i < processedAliases.Count; i++)
                    {
                        processedAliases[i] = processedAliases[i].Split(' ').Last();
                    }

                    replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Command Help");

                    // Get command description, if any.
                    FullDescription commandDescription = command.Attributes.FirstOrDefault(x => x is FullDescription) as FullDescription;
                    string formattedDescription = string.Empty;

                    if (commandDescription != default(FullDescription) && !string.IsNullOrEmpty(commandDescription.Description))
                        formattedDescription = commandDescription.Description;
                    else
                        formattedDescription = "No description.";

                    replyEmbed.AddField("🏷 Name", command.Name, true);
                    replyEmbed.AddField("🗃 Group", command.Module.Group, true);
                    replyEmbed.AddField("🎭 Aliases", processedAliases.Count == 0 ? "No aliases." : string.Join(", ", processedAliases.ToArray()), true);
                    replyEmbed.AddField("📋 Description", formattedDescription);

                    // Get command cooldown information.
                    RateLimit commandRateLimit = command.Preconditions.FirstOrDefault(x => x is RateLimit) as RateLimit;
                    string rateLimitString = string.Empty;

                    if (commandRateLimit == default(RateLimit))
                    {
                        rateLimitString = "This command has no cooldown.";
                    }
                    else
                    {
                        rateLimitString = $"Cooldown of **{commandRateLimit.Seconds}** second(s).\n" +
                                          $"Triggered after using the command **{commandRateLimit.Requests}** time(s).";
                    }

                    replyEmbed.AddField("⏱ Cooldown", rateLimitString);

                    // Append all parameters to output embed.
                    string commandParameters = "`*` = Optional • `^` = No quote marks needed if it contains spaces.\n";
                    foreach (ParameterInfo parameterInfo in command.Parameters)
                    {
                        string typeName = parameterInfo.Type.Name;
                        string additionalSymbols = string.Empty;
                        string defaultValue = "No default.";
                        string summaryString = "No summary.";

                        if (parameterInfo.IsOptional) additionalSymbols += "*";
                        if (parameterInfo.IsRemainder) additionalSymbols += "^";
                        if (parameterInfo.DefaultValue != null) defaultValue = parameterInfo.DefaultValue.ToString();
                        if (!string.IsNullOrEmpty(parameterInfo.Summary)) summaryString = parameterInfo.Summary;

                        commandParameters += $"[**{typeName}**{additionalSymbols}] `{parameterInfo.Name}`\n";
                        commandParameters += $"• **Summary**: {summaryString}\n";
                        commandParameters += $"• **Default**: {defaultValue}\n";
                    }

                    replyEmbed.AddField("📃 Parameters", command.Parameters.Count == 0 ? "No parameters." : commandParameters);
                }
            }
            else // ModuleName is null, user is asking to see all the available modules.
            {
                string replyDescription = $"These are all the modules available to you.\n" +
                                          $"Use `{botPrefix}help <Group Name>` to see its commands.";
                
                replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Module List", replyDescription);

                foreach (ModuleInfo moduleInfo in CommandService.Modules)
                {
                    bool foundCommand = false;

                    // Check permissions of each command. If no commands pass this check, the module
                    // doesn't get added to the output embed.
                    foreach (CommandInfo command in moduleInfo.Commands)
                    {
                        PreconditionResult preconditionResult = await command.CheckPreconditionsAsync(Context);

                        if (command.Attributes.Any(x => x is HideInHelp)) continue;

                        if (preconditionResult.IsSuccess) foundCommand = true;
                    }

                    if (foundCommand)
                    {
                        // Get module emoji, if any.
                        ModuleEmoji moduleEmoji = moduleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
                        string stringifiedEmoji = moduleEmoji != null ? moduleEmoji.Emoji + " " : string.Empty;

                        // Build the embed field.
                        string moduleHeader = $"{stringifiedEmoji}{moduleInfo.Name}\n" +
                                              $"(Group: `{moduleInfo.Group}`)";
                        string moduleDescription = string.IsNullOrEmpty(moduleInfo.Summary) ? "No description." : moduleInfo.Summary;

                        replyEmbed.AddField(moduleHeader, moduleDescription, true);
                    }
                }
            }
            
            await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);

            return ExecutionResult.Succesful();
        }
    }
}
