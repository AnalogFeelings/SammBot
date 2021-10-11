using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SammBotNET.Database;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Administration")]
    [Group("admin")]
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private readonly BlacklistedUsersDB BlacklistDatabase;

        public AdminModule(IServiceProvider services)
        {
            BlacklistDatabase = services.GetRequiredService<BlacklistedUsersDB>();
        }

        [Command("say", RunMode = RunMode.Async)]
        public async Task SayMessageAsync([Remainder] string message)
        {
            if (Context.Message.Author.Id != 337950448130719754)
            {
                return;
            }

            await Context.Client.GetGuild(850875298290597898).GetTextChannel(850875298290597902).SendMessageAsync(message);
        }

        [Command("blacklist", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Blacklist user from using this bot. Can be a phrase blacklist or a total blacklist.")]
        public async Task BlacklistUserAsync(ulong userID, [Remainder] string type)
        {
            if (BlacklistDatabase.BlacklistedUser.Any(ba => ba.userID == userID))
            {
                BlacklistedUser blacklistedUser
                    = await BlacklistDatabase.BlacklistedUser.SingleOrDefaultAsync(b => b.userID == userID);
                blacklistedUser.banType = type;
                await BlacklistDatabase.SaveChangesAsync();
            }
            else
            {
                await BlacklistDatabase.AddAsync(new BlacklistedUser
                {
                    userID = userID,
                    banType = type
                });
                await BlacklistDatabase.SaveChangesAsync();
            }

            if (type == "phrase") await ReplyAsync($"The user {userID} won't be able to create phrases by chatting anymore.");
            else await ReplyAsync($"The user {userID} won't be able to use the bot anymore.");
        }

        [Command("whitelist", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        [Summary("Whitelist user from using this bot.")]
        public async Task WhitelistUserAsync(ulong userID)
        {
            if (BlacklistDatabase.BlacklistedUser.Any(ba => ba.userID == userID))
            {
                BlacklistedUser user = BlacklistDatabase.BlacklistedUser.First(x => x.userID == userID);
                BlacklistDatabase.BlacklistedUser.Remove(user);
                await BlacklistDatabase.SaveChangesAsync();
            }
            else
            {
                await ReplyAsync("That user isn't blacklisted!");
                return;
            }
            await ReplyAsync($"Done! I've whitelisted user {userID}.");
        }
    }
}