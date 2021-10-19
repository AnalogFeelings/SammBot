using Discord;
using Discord.Commands;
using SammBotNET.Extensions;
using SammBotNET.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        public readonly CommandService CommandService;
        public HelpService HelpService { get; set; }

        public HelpModule(CommandService service)
        {
            CommandService = service;
        }

        [Command("help")]
        [HideInHelp]
        [Summary("Provides all commands and modules available.")]
        public async Task<RuntimeResult> HelpAsync()
        {
            string prefix = GlobalConfig.Instance.LoadedConfig.BotPrefix;

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Help", $"These are all of the modules available." +
                                                                    $"\n Use `{prefix}.help <Module/Group Name>` to see its commands.");

            foreach (ModuleInfo module in CommandService.Modules)
            {
                bool foundCommand = false;

                foreach (CommandInfo cmd in module.Commands)
                {
                    PreconditionResult result = await cmd.CheckPreconditionsAsync(Context);

                    if (cmd.Attributes.Any(x => x is HideInHelp)) continue;

                    if (result.IsSuccess) foundCommand = true;
                }

                if (foundCommand)
                    embed.AddField($"{module.Name} (Group: `{module.Group}`)",
                        string.IsNullOrEmpty(module.Summary) ? "No description." : module.Summary, true);
            }

            await ReplyAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("help")]
        [HideInHelp]
        [Summary("Provides all commands and modules available.")]
        public async Task<RuntimeResult> HelpAsync([Remainder] string moduleName)
        {
            string prefix = GlobalConfig.Instance.LoadedConfig.BotPrefix;
            string[] splittedModuleName = moduleName.Split(' ');

            if (splittedModuleName.Length == 1)
            {
                ModuleInfo moduleInfo = CommandService.Modules.Single(x => x.Name == moduleName || x.Group == moduleName);

                if (moduleInfo == null)
                    return ExecutionResult.FromError($"The module \"{moduleName}\" doesn't exist.");

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context,
                    "Help", $"Syntax: `{prefix}{moduleInfo.Group} <Command Name>`");

                string description = string.Empty;
                foreach (CommandInfo match in moduleInfo.Commands)
                {
                    if (match.Attributes.Any(x => x is HideInHelp)) continue;

                    PreconditionResult result = await match.CheckPreconditionsAsync(Context);

                    if (result.IsSuccess)
                        embed.AddField(match.Name, $"{(string.IsNullOrWhiteSpace(match.Summary) ? "No description." : match.Summary)}", true);
                }

                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                string actualName = splittedModuleName.Last();

                SearchResult result = CommandService.Search(Context, moduleName);

                if (!result.IsSuccess)
                    return ExecutionResult.FromError($"There is no command named \"{moduleName}\". Check your spelling.");

                CommandMatch match = result.Commands.SingleOrDefault(x => x.Command.Name == actualName);

                if (match.Command == null)
                    return ExecutionResult.FromError($"There is no command named \"{moduleName}\". Check your spelling.");

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Help");

                CommandInfo command = match.Command;

                List<string> processedAliases = command.Aliases.ToList();
                processedAliases.RemoveAt(0); //Remove the command name itself from the aliases list.

                //Remove group name from aliases list.
                for (int i = 0; i < processedAliases.Count; i++)
                {
                    processedAliases[i] = processedAliases[i].Split(' ').Last();
                }

                embed.AddField("Command Name", command.Name);
                embed.AddField("Command Aliases", processedAliases.Count == 0 ? "No aliases." : string.Join(", ", processedAliases.ToArray()));
                embed.AddField("Command Summary", string.IsNullOrWhiteSpace(command.Summary) ? "No summary." : command.Summary);

                string commandParameters = string.Empty;
                foreach (ParameterInfo parameterInfo in command.Parameters)
                {
                    commandParameters += $"[**{parameterInfo.Type.Name}**] `{parameterInfo.Name}`\n";
                }

                embed.AddField("Command Parameters", string.IsNullOrEmpty(commandParameters) ? "No parameters." : commandParameters);

                await ReplyAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }
    }
}
