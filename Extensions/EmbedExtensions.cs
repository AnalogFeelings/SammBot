using Discord;
using Discord.Commands;
using System;

namespace SammBotNET.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder BuildDefaultEmbed(this EmbedBuilder builder, SocketCommandContext context, string title = "", string description = "")
        {
            if(context == null)
                throw new ArgumentNullException(nameof(context));

            string botName = GlobalConfig.Instance.LoadedConfig.BotName;

            builder.Color = Color.DarkPurple;
            builder.Title = $"{botName.ToUpper()} {title.ToUpper()}";
            builder.Description = description;

            builder.WithAuthor(author => author.Name = $"{botName.ToUpper()} COMMANDS");
            builder.WithFooter(footer => { footer.Text = botName; footer.IconUrl = context.Client.CurrentUser.GetAvatarUrl(); });
            builder.WithCurrentTimestamp();

            return builder;
        }

        public static EmbedBuilder ChangeTitle(this EmbedBuilder builder, string title, bool includeName = false)
        {
            string botName = GlobalConfig.Instance.LoadedConfig.BotName;
            if (includeName) builder.WithTitle($"{botName.ToUpper()} {title.ToUpper()}");
            else builder.WithTitle(title.ToUpper());

            return builder;
        }

        public static EmbedBuilder ChangeFooter(this EmbedBuilder builder, SocketCommandContext context, string text)
        {
            builder.WithFooter(footer => { footer.Text = text; footer.IconUrl = context.Client.CurrentUser.GetAvatarUrl(); });

            return builder;
        }
    }
}
