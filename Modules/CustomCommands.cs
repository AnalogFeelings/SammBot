using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
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
        public DiscordSocketClient Client { get; set; }

        private readonly CommandService CommandService;

        public CustomCommands(IServiceProvider services, CommandService cmds) => CommandService = cmds;

        [Command("list", RunMode = RunMode.Async)]
        [Alias("all")]
        [MustRunInGuild]
        [Summary("Lists all the custom commands that have been created.")]
        public async Task<RuntimeResult> CustomsAsync()
        {
            if (CustomCommandService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(CustomCommands)}\" is disabled.");

            using (CommandDB CommandDatabase = new())
            {
                List<CustomCommand> customCommands = await CommandDatabase.CustomCommand.ToListAsync();
                customCommands = customCommands.Where(x => x.ServerId == Context.Guild.Id).ToList();

                EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, "Custom Commands", "All of the custom commands that have been created.");

                if (customCommands.Count > 0)
                {
                    foreach (CustomCommand customCommand in customCommands)
                    {
                        RestUser globalUser = await Client.Rest.GetUserAsync(customCommand.AuthorId);
                        string assembledAuthor = $"{globalUser.Username}#{globalUser.Discriminator}";

                        embed.AddField($"{GlobalConfig.Instance.LoadedConfig.BotPrefix}{customCommand.Name}",
                            $"By **{assembledAuthor}**, <t:{customCommand.CreatedAt}>");
                    }
                }
                else embed.AddField("Wow...", "There are no custom commands yet.");

                await Context.Channel.SendMessageAsync("", false, embed.Build());
            }

            return ExecutionResult.Succesful();
        }

        [Command("delete")]
        [Alias("remove", "destroy")]
        [MustRunInGuild]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes a custom command.")]
        public async Task<RuntimeResult> DeleteCMDAsync(string name)
        {
            using (CommandDB CommandDatabase = new())
            {
                List<CustomCommand> customCommands = await CommandDatabase.CustomCommand.ToListAsync();
                CustomCommand customCommand = customCommands.SingleOrDefault(x => x.Name == name && x.AuthorId == Context.User.Id
                    || x.AuthorId == GlobalConfig.Instance.LoadedConfig.AestheticalUid);

                if (customCommand == null)
                    return ExecutionResult.FromError("That custom command does not exist!");

                CommandDatabase.Remove(customCommand);
                await CommandDatabase.SaveChangesAsync();
            }
            await ReplyAsync("Success!");

            return ExecutionResult.Succesful();
        }

        [Command("create", RunMode = RunMode.Async)]
        [Alias("new")]
        [MustRunInGuild]
        [Summary("Creates a custom command.")]
        public async Task<RuntimeResult> CreateCMDAsync(string name, string reply)
        {
            if (CustomCommandService.IsDisabled)
                return ExecutionResult.FromError($"The module \"{nameof(CustomCommands)}\" is disabled.");

            if (name.Length > 15)
                return ExecutionResult.FromError("Please make the command name shorter than 15 characters!");
            else if (name.StartsWith(GlobalConfig.Instance.LoadedConfig.BotPrefix))
                return ExecutionResult.FromError("Custom command names can't begin with my prefix!");
            else if (Context.Message.MentionedUsers.Count > 0)
                return ExecutionResult.FromError("Custom command names or replies cannot contain pings!");
            else if (name.Contains(" "))
                return ExecutionResult.FromError("Custom command names cannot contain spaces!");

            foreach (CommandInfo commandInfo in CommandService.Commands)
            {
                if (name == commandInfo.Name) return ExecutionResult.FromError("That command name already exists!");
            }

            using (CommandDB CommandDatabase = new())
            {
                List<CustomCommand> dbCommands = await CommandDatabase.CustomCommand.ToListAsync();
                dbCommands = dbCommands.Where(x => x.ServerId == Context.Guild.Id).ToList();

                foreach (CustomCommand customCommand in dbCommands)
                {
                    if (name == customCommand.Name) return ExecutionResult.FromError("That command name already exists!");
                }

                await ReplyAsync($"Creating command \"{GlobalConfig.Instance.LoadedConfig.BotPrefix}{name}\"...");

                int nextId = 0;
                if (dbCommands.Count > 0)
                {
                    nextId = dbCommands.Max(x => x.Id) + 1;
                }

                await CommandDatabase.AddAsync(new CustomCommand
                {
                    Id = nextId,
                    Name = name,
                    AuthorId = Context.Message.Author.Id,
                    ServerId = Context.Guild.Id,
                    Reply = reply,
                    CreatedAt = Context.Message.Timestamp.ToUnixTimeSeconds()
                });
                await CommandDatabase.SaveChangesAsync();
            }

            await ReplyAsync("Command created succesfully!");

            return ExecutionResult.Succesful();
        }
    }
}
