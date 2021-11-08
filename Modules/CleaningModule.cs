﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SammBotNET.Database;
using SammBotNET.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Janitoring")]
    [Summary("Clearing and Purging.")]
    [Group("clean")]
    public class CleaningModule : ModuleBase<SocketCommandContext>
    {
        [Command("customcmds")]
        [Alias("customs", "commands")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes all custom commands.")]
        public async Task<RuntimeResult> FlushCMDsAsync()
        {
            using (CommandDB CommandDatabase = new())
            {
                await ReplyAsync("Flushing database...");
                int rows = await CommandDatabase.Database.ExecuteSqlRawAsync("delete from CustomCommand");
                await ReplyAsync($"Success! `{rows}` rows affected.");
            }

            return ExecutionResult.Succesful();
        }

        [Command("quotes")]
        [Alias("phrases")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes all quotes.")]
        public async Task<RuntimeResult> FlushQuotesAsync()
        {
            using (PhrasesDB PhrasesDatabase = new())
            {
                await ReplyAsync("Flushing database...");
                int rows = await PhrasesDatabase.Database.ExecuteSqlRawAsync("delete from Phrase");
                await ReplyAsync($"Success! `{rows}` rows affected.");
            }

            return ExecutionResult.Succesful();
        }

        [Command("messages")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [Summary("Deletes an amount of messages.")]
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
