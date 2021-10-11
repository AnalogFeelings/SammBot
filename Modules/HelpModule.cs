using Discord;
using Discord.Commands;
using SammBotNET.Services;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService CommandService;
        public HelpService HelpService { get; set; }

        public HelpModule(CommandService service)
        {
            CommandService = service;
        }

        [Command("help")]
        [Summary("Provides all commands available.")]
        public async Task HelpAsync([Remainder] string CommandName = null)
        {
            if (CommandName == null)
            {
                EmbedBuilder embed = new EmbedBuilder()
                {
                    Color = Color.DarkPurple,
                    Title = "SAMM-BOT HELP",
                    Description = $"These are all the commands available." +
                    $"\n Syntax: {GlobalConfig.Instance.LoadedConfig.BotPrefix}<module (sometimes)> <cmdname>"
                };

                foreach (ModuleInfo module in CommandService.Modules)
                {
                    string description = null;
                    foreach (CommandInfo cmd in module.Commands)
                    {
                        var result = await cmd.CheckPreconditionsAsync(Context);
                        if (result.IsSuccess)
                            description += $"{GlobalConfig.Instance.LoadedConfig.BotPrefix}{cmd.Aliases[0]}\n";
                    }

                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        embed.AddField(module.Name, description, false);
                    }
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
                {
                    await ReplyAsync($"The command \"{CommandName}\" doesn't exist.");
                    return;
                }
                EmbedBuilder embed = new EmbedBuilder()
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
        }
    }
}
