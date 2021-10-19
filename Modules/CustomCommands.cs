using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Extensions;
using SammBotNET.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Custom Commands")]
    [Summary("Commands with a custom reply.")]
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
        [Alias("all")]
        [Summary("Lists all the custom commands that have been created.")]
        public async Task<RuntimeResult> CustomsAsync()
        {
            if (CustomCommandService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(CustomCommands)}\" is disabled.");

            List<CustomCommand> commands = await CommandDatabase.CustomCommand.ToListAsync();

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Custom Commands", "All of the custom commands that have been created.");

            if (commands.Count > 0)
            {
                foreach (CustomCommand cmd in commands)
                {
                    embed.AddField($"{GlobalConfig.Instance.LoadedConfig.BotPrefix}{cmd.name}", $"By <@{cmd.authorID}>");
                }
            }
            else embed.AddField("Wow...", "There are no custom commands yet.");

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        [Command("create", RunMode = RunMode.Async)]
        [Alias("new")]
        [Summary("Creates a custom command.")]
        public async Task<RuntimeResult> CreateCMDAsync(string name, string reply)
        {
            if (CustomCommandService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(CustomCommands)}\" is disabled.");

            if (CustomCommandService.IsCreatingCommand == true)
                return ExecutionResult.FromError("A command is already being created! Please wait for a bit.");

            #region Validity Checks

            if (name.Length > 15)
                return ExecutionResult.FromError("Please make the command name shorter than 15 characters!");
            else if (name.StartsWith(GlobalConfig.Instance.LoadedConfig.BotPrefix))
                return ExecutionResult.FromError("Custom command names can't begin with my prefix!");
            else if (Context.Message.MentionedUsers.Count > 0)
                return ExecutionResult.FromError("Custom command names or replies cannot contain pings!");
            else if (name.Contains(" "))
                return ExecutionResult.FromError("Custom command names cannot contain spaces!");

            foreach (CommandInfo cmdInf in CommandService.Commands)
            {
                if (name == cmdInf.Name) return ExecutionResult.FromError("That command name already exists!");
            }

            List<CustomCommand> dbCommands = await CommandDatabase.CustomCommand.ToListAsync();
            foreach (CustomCommand ccmd in dbCommands)
            {
                if (name == ccmd.name) return ExecutionResult.FromError("That command name already exists!");
            }

            #endregion

            CustomCommandService.IsCreatingCommand = true;
            await ReplyAsync($"Creating command \"{GlobalConfig.Instance.LoadedConfig.BotPrefix}{name}\"...");

            await CommandDatabase.AddAsync(new CustomCommand
            {
                name = name,
                authorID = Context.Message.Author.Id,
                reply = reply
            });
            await CommandDatabase.SaveChangesAsync();

            await ReplyAsync("Command created succesfully!");
            CustomCommandService.IsCreatingCommand = false;

            return ExecutionResult.Succesful();
        }
    }
}
