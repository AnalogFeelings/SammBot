using Discord;
using Discord.Commands;
using SammBotNET.Services;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("Module Administration")]
    [Group("modules")]
    public class ModuleAdmin : ModuleBase<SocketCommandContext>
    {
        public CustomCommandService CommandService { get; set; }
        public HelpService HelpService { get; set; }
        public MathService MathService { get; set; }
        public PhraseService PhraseService { get; set; }

        [Command("enable")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Enables a module. Requires permission ManageGuild.")]
        public async Task EnableModAsync([Remainder] string ModuleName)
        {
            switch (ModuleName) //Dirty way to do this, maybe we can use System.Reflection?
            {
                case "CustomCommands":
                    if (CommandService.IsDisabled == true)
                    {
                        CommandService.IsDisabled = false;
                        await ReplyAsync($"Enabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already enabled.");
                    }
                    break;
                case "Help":
                    if (HelpService.IsDisabled == true)
                    {
                        HelpService.IsDisabled = false;
                        await ReplyAsync($"Enabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already enabled.");
                    }
                    break;
                case "Math":
                    if (MathService.IsDisabled == true)
                    {
                        MathService.IsDisabled = false;
                        await ReplyAsync($"Enabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already enabled.");
                    }
                    break;
                case "Phrases":
                    if (PhraseService.IsDisabled == true)
                    {
                        PhraseService.IsDisabled = false;
                        await ReplyAsync($"Enabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already enabled.");
                    }
                    break;
            }
            return;
        }

        [Command("disable")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        [Summary("Disables a module. Requires permission ManageGuild")]
        public async Task DisableModAsync([Remainder] string ModuleName)
        {
            switch (ModuleName) //Dirty way to do this, maybe we can use System.Reflection?
            {
                case "CustomCommands":
                    if (CommandService.IsDisabled == false)
                    {
                        CommandService.IsDisabled = true;
                        await ReplyAsync($"Disabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already disabled.");
                    }
                    break;
                case "Help":
                    if (HelpService.IsDisabled == false)
                    {
                        HelpService.IsDisabled = true;
                        await ReplyAsync($"Disabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already disabled.");
                    }
                    break;
                case "Math":
                    if (MathService.IsDisabled == false)
                    {
                        MathService.IsDisabled = true;
                        await ReplyAsync($"Disabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already disabled.");
                    }
                    break;
                case "Phrases":
                    if (PhraseService.IsDisabled == false)
                    {
                        PhraseService.IsDisabled = true;
                        await ReplyAsync($"Disabled module \"{ModuleName}\".");
                    }
                    else
                    {
                        await ReplyAsync($"Module \"{ModuleName}\" is already disabled.");
                    }
                    break;
            }
            return;
        }
    }
}
