using Discord;
using Discord.Commands;
using SammBot.Bot.Core;
using System;
using Discord.Interactions;

namespace SammBot.Bot.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder BuildDefaultEmbed(this EmbedBuilder Builder, ShardedInteractionContext Context, string Title = "", string Description = "")
        {
            if (Context == null)
                throw new ArgumentNullException(nameof(Context));

            string botName = Settings.BOT_NAME;

            Builder.Color = Color.DarkPurple;
            Builder.Title = $"{botName.ToUpper()} {Title.ToUpper()}";
            Builder.Description = Description;

            Builder.WithFooter(x =>
            {
                x.Text = $"Requested by {Context.Interaction.User.GetFullUsername()}"; 
                x.IconUrl = Context.Interaction.User.GetGuildGlobalOrDefaultAvatar(256);
            });
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
            Builder.WithFooter(x =>
            {
                x.Text = Text; 
                x.IconUrl = Context.Client.CurrentUser.GetAvatarUrl();
            });

            return Builder;
        }
    }
}
