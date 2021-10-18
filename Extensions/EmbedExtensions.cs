using Discord;

namespace SammBotNET.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder BuildDefaultSamm(this EmbedBuilder builder, string title = "", string description = "")
        {
            string botName = GlobalConfig.Instance.LoadedConfig.BotName;

            builder.Color = Color.DarkPurple;
            builder.Title = $"{botName.ToUpper()} " + title.ToUpper();
            builder.Description = description;

            builder.WithAuthor(author => author.Name = $"{botName.ToUpper()} COMMANDS");
            builder.WithFooter(footer => footer.Text = botName);
            builder.WithCurrentTimestamp();

            return builder;
        }
    }
}
