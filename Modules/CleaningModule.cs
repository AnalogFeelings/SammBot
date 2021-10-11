using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using SammBotNET.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Cleaning & Purging")]
    [Group("clean")]
    public class CleaningModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandDB CommandDatabase;
        private readonly PhrasesDB PhrasesDatabase;

        public CleaningModule(IServiceProvider services)
        {
            CommandDatabase = services.GetRequiredService<CommandDB>();
            PhrasesDatabase = services.GetRequiredService<PhrasesDB>();
        }

        [Command("customcmds")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes all custom commands.")]
        public async Task<RuntimeResult> FlushCMDsAsync()
        {
            await ReplyAsync("Flushing database...");
            await CommandDatabase.Database.ExecuteSqlRawAsync("delete from CustomCommand");
            await ReplyAsync("Success!");

            return ExecutionResult.Succesful();
        }

        [Command("phrases")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes all quotes.")]
        public async Task<RuntimeResult> FlushPhrasesAsync()
        {
            await ReplyAsync("Flushing database...");
            await PhrasesDatabase.Database.ExecuteSqlRawAsync("delete from Phrase");
            await ReplyAsync("Success!");

            return ExecutionResult.Succesful();
        }

        [Command("messages")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes (num) amount of messages.")]
        public async Task<RuntimeResult> FlushMessagesAsync(int num)
        {
            IEnumerable<IMessage> messages = await Context.Message.Channel.GetMessagesAsync(num + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            IUserMessage purgeMsg = await ReplyAsync($"Success! Cleared `{num}` message/s.");
            await Task.Delay(3000);
            await purgeMsg.DeleteAsync();

            return ExecutionResult.Succesful();
        }
    }
}
