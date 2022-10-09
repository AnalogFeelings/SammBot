using Discord;
using Discord.Commands;
using System;

namespace SammBotNET.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder BuildDefaultEmbed(this EmbedBuilder Builder, SocketCommandContext Context, string Title = "", string Description = "")
        {
            if (Context == null)
                throw new ArgumentNullException(nameof(Context));

            string botName = Settings.BOT_NAME;

            Builder.Color = Color.DarkPurple;
            Builder.Title = $"{botName.ToUpper()} {Title.ToUpper()}";
            Builder.Description = Description;

            Builder.WithFooter(footer => { footer.Text = $"Requested by {Context.Message.Author.GetFullUsername()}"; footer.IconUrl = Context.Message.Author.GetGuildGlobalOrDefaultAvatar(128); });
            Builder.WithCurrentTimestamp();

            return Builder;
        }

        public static EmbedBuilder ChangeTitle(this EmbedBuilder Builder, string Title, bool IncludeName = false)
        {
            string botName = Settings.BOT_NAME;

            if (IncludeName) Builder.WithTitle($"{botName.ToUpper()} {Title.ToUpper()}");
            else Builder.WithTitle(Title.ToUpper());

            return Builder;
        }

        public static EmbedBuilder ChangeFooter(this EmbedBuilder Builder, SocketCommandContext Context, string Text)
        {
            Builder.WithFooter(footer => { footer.Text = Text; footer.IconUrl = Context.Client.CurrentUser.GetAvatarUrl(); });

            return Builder;
        }
    }
}
