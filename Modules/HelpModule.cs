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
        public async Task<RuntimeResult> HelpAsync([Remainder] string CommandName = null)
        {
            if (CommandName == null)
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

                    if(foundCommand)
                        embed.AddField(module.Name, string.IsNullOrEmpty(module.Summary) ? "No description." : $"`{module.Summary}`", true);
                }
                embed.WithAuthor(author => author.Name = "SAMM-BOT COMMANDS");
                embed.WithFooter(footer => footer.Text = "Samm-Bot");
                embed.WithCurrentTimestamp();

                await ReplyAsync("", false, embed.Build());
            }
            else
            {
                SearchResult result = CommandService.Search(Context, CommandName);

                if (!result.IsSuccess)
                    return ExecutionResult.FromError($"The command \"{CommandName}\" doesn't exist.");

                EmbedBuilder embed = new()
                {
                    Title = "SAMM-BOT HELP",
                    Description = $"All commands that are called \"{CommandName}\":",
                    Color = Color.DarkPurple,
                };

                foreach (CommandMatch match in result.Commands)
                {
                    CommandInfo cmd = match.Command;

                    string parameters = string.Join(", ", cmd.Parameters.Select(p => p.Name));
                    if (parameters == string.Empty) parameters = "no parameters.";

                    string description = cmd.Summary ?? (description = "No description provided.");

                    embed.AddField(string.Join(", ", cmd.Aliases), $"Parameters: {parameters}\nDescription: {cmd.Summary}", false);
                }
                embed.WithAuthor(author => author.Name = "SAMM-BOT COMMANDS");
                embed.WithFooter(footer => footer.Text = "Samm-Bot");
                embed.WithCurrentTimestamp();

                await ReplyAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }
    }
}
