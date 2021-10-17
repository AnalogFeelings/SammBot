using Discord;
using Discord.Commands;
using SammBotNET.Extensions;
using SammBotNET.Services;
using System.Data;
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
            EmbedBuilder embed = new()
            {
                Color = Color.DarkPurple,
                Title = "SAMM-BOT HELP",
                Description = $"These are all of the modules available." +
                $"\n Use `s.help <Module Name>` to see its commands."
            };

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
                    embed.AddField(module.Name, string.IsNullOrEmpty(module.Summary) ? "No description." : $"`{module.Summary}`", true);
            }
            embed.WithAuthor(author => author.Name = "SAMM-BOT COMMANDS");
            embed.WithFooter(footer => footer.Text = "Samm-Bot");
            embed.WithCurrentTimestamp();

            await ReplyAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("help")]
        [HideInHelp]
        [Summary("Provides all commands and modules available.")]
        public async Task<RuntimeResult> HelpAsync([Remainder] string moduleName)
        {
            ModuleInfo moduleInfo = CommandService.Modules.Single(x => x.Name == moduleName);

            if (moduleInfo == null)
                return ExecutionResult.FromError($"The module \"{moduleName}\" doesn't exist.");

            EmbedBuilder embed = new()
            {
                Title = "SAMM-BOT HELP",
                Description = $"Syntax: `{GlobalConfig.Instance.LoadedConfig.BotPrefix}{moduleInfo.Name} <Command Name>`",
                Color = Color.DarkPurple
            };

            string description = string.Empty;
            foreach (CommandInfo match in moduleInfo.Commands)
            {
                if (match.Attributes.Any(x => x is HideInHelp)) continue;

                PreconditionResult result = await match.CheckPreconditionsAsync(Context);

                if (result.IsSuccess)
                    embed.AddField(match.Name, $"`{(string.IsNullOrWhiteSpace(match.Summary) ? "No description." : match.Summary)}`", true);
            }

            embed.WithAuthor(author => author.Name = "SAMM-BOT COMMANDS");
            embed.WithFooter(footer => footer.Text = "Samm-Bot");
            embed.WithCurrentTimestamp();

            await ReplyAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }
    }
}
