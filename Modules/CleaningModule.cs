using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SammBotNET.Core;
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
        [Command("tags")]
        [Alias("usertags")]
        [Summary("Deletes all tags.")]
        public async Task<RuntimeResult> FlushTagsAsync()
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

            using (TagDB CommandDatabase = new())
            {
                await ReplyAsync("Flushing database...");
                int rows = await CommandDatabase.Database.ExecuteSqlRawAsync("delete from UserTag");
                await ReplyAsync($"Success! `{rows}` rows affected.");
            }

            return ExecutionResult.Succesful();
        }

        [Command("quotes")]
        [Alias("phrases")]
        [Summary("Deletes all quotes.")]
        public async Task<RuntimeResult> FlushQuotesAsync()
        {
            if (Context.User.Id != GlobalConfig.Instance.LoadedConfig.AestheticalUid)
                return ExecutionResult.FromError("You are not allowed to execute this command.");

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
        public async Task<RuntimeResult> FlushMessagesAsync(int Count)
        {
            IEnumerable<IMessage> messages = await Context.Message.Channel.GetMessagesAsync(Count + 1).FlattenAsync();
            await (Context.Channel as SocketTextChannel).DeleteMessagesAsync(messages);
            IUserMessage purgeMsg = await ReplyAsync($"Success! Cleared `{Count}` message/s.");
            await Task.Delay(3000);
            await purgeMsg.DeleteAsync();

            return ExecutionResult.Succesful();
        }
    }
}
