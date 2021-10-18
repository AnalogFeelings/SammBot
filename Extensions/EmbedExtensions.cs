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
                throw new ArgumentNullException("Context was null when building default embed.");

            string botName = GlobalConfig.Instance.LoadedConfig.BotName;

            builder.Color = Color.DarkPurple;
            builder.Title = $"{botName.ToUpper()} " + title.ToUpper();
            builder.Description = description;

            builder.WithAuthor(author => author.Name = $"{botName.ToUpper()} COMMANDS");
            builder.WithFooter(footer => { footer.Text = botName; footer.IconUrl = context.Client.CurrentUser.GetAvatarUrl(); });
            builder.WithCurrentTimestamp();

            return builder;
        }
    }
}
