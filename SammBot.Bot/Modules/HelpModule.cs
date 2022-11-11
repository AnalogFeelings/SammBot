using System;
using Discord;
using SammBot.Bot.Classes;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;

namespace SammBot.Bot.Modules
{
    [FullName("Help")]
    public class HelpModule : InteractionModuleBase<ShardedInteractionContext>
    {
        public InteractionService InteractionService { get; set; }
        public IServiceProvider ServiceProvider { get; set; }

        [SlashCommand("help", "Provides all commands and modules available.")]
        [FullDescription("Provides a list of all the commands and modules available.")]
        [RateLimit(1, 3)]
        [HideInHelp]
        public async Task<RuntimeResult> HelpAsync([Summary(description: "Leave empty to list all modules. " +
                                                                         "Add a command name to list help for that command.")] string ModuleName = null)
        {
            // Hoh boy, this is QUITE a bumpy ride. Be ready!
            await DeferAsync();

            EmbedBuilder replyEmbed = null;

            // User passed a module name and probably command name too.
            if (ModuleName != null)
            {
                // Split the name.
                string[] splittedName = ModuleName.Split(' ');

                // If the splittedName array contains 1 element, the user is looking for a module.
                if (splittedName.Length == 1)
                {
                    ModuleInfo moduleInfo = InteractionService.Modules.SingleOrDefault(x => x.Name == ModuleName || x.SlashGroupName == ModuleName);

                    if (moduleInfo == default(ModuleInfo))
                        return ExecutionResult.FromError($"The module \"{ModuleName}\" doesn't exist.");

                    // Get the module emoji, if it has any.
                    ModuleEmoji moduleEmoji = moduleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
                    string stringifiedEmoji = moduleEmoji != default(ModuleEmoji) ? moduleEmoji.Emoji + " " : string.Empty;
                    
                    FullName moduleName = moduleInfo.Attributes.First(x => x is FullName) as FullName;

                    string moduleHeader = $"**{stringifiedEmoji}{moduleName.Name}**\n" +
                                          $"{moduleInfo.Description}\n" +
                                          $"**Syntax**: `/{moduleInfo.SlashGroupName} <Command Name>`";

                    replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Module Help", moduleHeader);

                    replyEmbed.Title = "\U0001f4c2 Module Help";
                    replyEmbed.Color = new Color(85, 172, 238);

                    // Check permissions of containing commands. If the command doesn't pass the check, the command
                    // doesn't get added to the output embed.
                    bool foundCommand = false;
                    foreach (SlashCommandInfo command in moduleInfo.SlashCommands)
                    {
                        if (command.Attributes.Any(x => x is HideInHelp)) continue;

                        PreconditionResult preconditionResult = await command.CheckPreconditionsAsync(Context, ServiceProvider);

                        if (!preconditionResult.IsSuccess) continue;

                        replyEmbed.AddField(command.Name, $"{(string.IsNullOrWhiteSpace(command.Description) ? "No summary." : command.Description)}", true);
                        foundCommand = true;
                    }

                    // Report an error if no command passed the check.
                    if (!foundCommand)
                        return ExecutionResult.FromError($"The module \"{moduleInfo.Name}\" has no commands, or you don't have enough permissions to see them.");
                }
                else // splittedName array contains more than 1 element, user is looking for a command.
                {
                    SlashCommandInfo searchResult = InteractionService.SlashCommands.FirstOrDefault(x => x.Module.SlashGroupName == splittedName[0] 
                                                                                                   && x.Name == splittedName[1]);

                    if (searchResult == null)
                        return ExecutionResult.FromError($"There is no command named \"{ModuleName}\". Check your spelling.");

                    replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Command Help");

                    replyEmbed.Title = "\U0001f4c4 Command Help";
                    replyEmbed.Color = new Color(204, 214, 221);

                    // Get command description, if any.
                    FullDescription commandDescription = searchResult.Attributes.FirstOrDefault(x => x is FullDescription) as FullDescription;
                    string formattedDescription = string.Empty;

                    if (commandDescription != default(FullDescription) && !string.IsNullOrEmpty(commandDescription.Description))
                        formattedDescription = commandDescription.Description;
                    else
                        formattedDescription = "No description.";

                    replyEmbed.AddField("🏷 Name", searchResult.Name, true);
                    replyEmbed.AddField("🗃 Group", searchResult.Module.SlashGroupName, true);
                    replyEmbed.AddField("\U0001f575\uFE0F Usable in DMs", searchResult.IsEnabledInDm.ToYesNo(), true);
                    replyEmbed.AddField("📋 Description", formattedDescription);

                    // Get command cooldown information.
                    RateLimit commandRateLimit = searchResult.Preconditions.FirstOrDefault(x => x is RateLimit) as RateLimit;
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
                    string commandParameters = "A parameter with a `*` symbol is optional.\n";
                    foreach (SlashCommandParameterInfo parameterInfo in searchResult.Parameters)
                    {
                        string typeName = parameterInfo.ParameterType.Name;
                        string additionalSymbols = string.Empty;
                        string defaultValue = "No default.";
                        string summaryString = "No summary.";

                        if (!parameterInfo.IsRequired) additionalSymbols += "*";
                        if (parameterInfo.DefaultValue != null) defaultValue = parameterInfo.DefaultValue.ToString();
                        if (!string.IsNullOrEmpty(parameterInfo.Description)) summaryString = parameterInfo.Description;

                        commandParameters += $"[**{typeName}**{additionalSymbols}] `{parameterInfo.Name}`\n";
                        commandParameters += $"• **Summary**: {summaryString}\n";
                        commandParameters += $"• **Default**: {defaultValue}\n";
                    }

                    replyEmbed.AddField("📃 Parameters", searchResult.Parameters.Count == 0 ? "No parameters." : commandParameters);
                }
            }
            else // ModuleName is null, user is asking to see all the available modules.
            {
                string replyDescription = $"These are all the modules available to you.\n" +
                                          $"Use `/help <Group Name>` to see its commands.";
                
                replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Module List", replyDescription);

                replyEmbed.Title = "\U0001f4c1 Module List";
                replyEmbed.Color = new Color(85, 172, 238);

                foreach (ModuleInfo moduleInfo in InteractionService.Modules)
                {
                    bool foundCommand = false;

                    // Check permissions of each command. If no commands pass this check, the module
                    // doesn't get added to the output embed.
                    foreach (SlashCommandInfo command in moduleInfo.SlashCommands)
                    {
                        PreconditionResult preconditionResult = await command.CheckPreconditionsAsync(Context, ServiceProvider);

                        if (command.Attributes.Any(x => x is HideInHelp)) continue;

                        if (preconditionResult.IsSuccess) foundCommand = true;
                    }

                    if (foundCommand)
                    {
                        // Get module emoji, if any.
                        ModuleEmoji moduleEmoji = moduleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
                        string stringifiedEmoji = moduleEmoji != null ? moduleEmoji.Emoji + " " : string.Empty;
                        
                        FullName moduleName = moduleInfo.Attributes.First(x => x is FullName) as FullName;

                        // Build the embed field.
                        string moduleHeader = $"{stringifiedEmoji}{moduleName.Name}\n" +
                                              $"(Group: `{moduleInfo.SlashGroupName}`)";
                        string moduleDescription = string.IsNullOrEmpty(moduleInfo.Description) ? "No description." : moduleInfo.Description;

                        replyEmbed.AddField(moduleHeader, moduleDescription, true);
                    }
                }
            }
            
            await FollowupAsync(null, embed: replyEmbed.Build(), allowedMentions: Settings.Instance.AllowOnlyUsers);

            return ExecutionResult.Succesful();
        }
    }
}
