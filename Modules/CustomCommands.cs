using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Custom Commands")]
    [Group("customcmds")]
    public class CustomCommands : ModuleBase<SocketCommandContext>
    {
        public CustomCommandService CustomCommandService { get; set; }
        public StartupService StartupService { get; set; }

        private readonly CommandDB CommandDatabase;
        private readonly CommandService CommandService;

        public CustomCommands(IServiceProvider services, CommandService cmds)
        {
            CommandDatabase = services.GetRequiredService<CommandDB>();
            CommandService = cmds;
        }

        [Command("list", RunMode = RunMode.Async)]
        [Summary("Lists all the custom commands that have been created.")]
        public async Task CustomsAsync()
        {
            if (CustomCommandService.IsDisabled)
            {
                await ReplyAsync($"The module \"{nameof(CustomCommands)}\" is disabled.");
                return;
            }

            List<CustomCommand> commands = await CommandDatabase.CustomCommand.ToListAsync();

            EmbedBuilder embed = new()
            {
                Title = "SAMM-BOT CUSTOM COMMANDS",
                Description = "All custom commands that have been created.",
                Color = Color.DarkPurple
            };

            foreach (CustomCommand cmd in commands)
            {
                embed.AddField($"{GlobalConfig.Instance.LoadedConfig.BotPrefix}{cmd.name}", $"By <@{cmd.authorID}>");
            }
            if (commands.Count == 0)
            {
                embed.AddField("Wow...", "There are no custom commands yet.");
            }
            embed.WithAuthor(author => author.Name = "SAMM-BOT COMMANDS");
            embed.WithFooter(footer => footer.Text = "Samm-Bot");
            embed.WithCurrentTimestamp();

            await Context.Channel.SendMessageAsync("", false, embed.Build());
        }

        [Command("create", RunMode = RunMode.Async)]
        [Summary("Creates a custom command.")]
        public async Task CreateCMDAsync(string Name, string Response)
        {
            if (CustomCommandService.IsDisabled)
            {
                await ReplyAsync($"The module \"{this.GetType().Name}\" is disabled.");
                return;
            }

            if (CustomCommandService.IsCreatingCommand == true)
            {
                await ReplyAsync("A command is already being created! Please wait for a bit.");
                return;
            }

            #region Validity Checks
            if (Name.Length > 15)
            {
                await ReplyAsync("Please make the command name shorter than 15 characters!");
                return;
            }
            else if (Name.StartsWith(GlobalConfig.Instance.LoadedConfig.BotPrefix))
            {
                await ReplyAsync("Custom command names can't begin with my prefix!");
                return;
            }
            else if (Context.Message.MentionedUsers.Count > 0)
            {
                await ReplyAsync("Custom command names or replies cannot contain pings!");
                return;
            }
            else if (Name.Contains(" "))
            {
                await ReplyAsync("Custom command names cannot contain spaces!");
                return;
            }

            foreach (CommandInfo cmdInf in CommandService.Commands)
            {
                if (Name == cmdInf.Name)
                {
                    await ReplyAsync("That command name already exists!");
                    return;
                }
            }

            List<CustomCommand> dbCommands = await CommandDatabase.CustomCommand.ToListAsync();
            foreach (CustomCommand ccmd in dbCommands)
            {
                if (Name == ccmd.name)
                {
                    await ReplyAsync("That command name already exists!");
                    return;
                }
            }
            #endregion

            CustomCommandService.IsCreatingCommand = true;
            await ReplyAsync($"Creating command \"{GlobalConfig.Instance.LoadedConfig.BotPrefix}{Name}\"...");

            await CommandDatabase.AddAsync(new CustomCommand
            {
                name = Name,
                authorID = Context.Message.Author.Id,
                reply = Response
            });
            await CommandDatabase.SaveChangesAsync();

            await ReplyAsync("Command created succesfully!");
            CustomCommandService.IsCreatingCommand = false;
        }
    }
}
