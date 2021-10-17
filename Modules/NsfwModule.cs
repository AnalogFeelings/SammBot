using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using SammBotNET.Extensions;
using SammBotNET.RestDefinitions;
using SammBotNET.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Group("nsfw")]
    [Summary("NSFW commands. Require a channel to be marked as NSFW.")]
    public class NsfwModule : ModuleBase<SocketCommandContext>
    {
        public NsfwService NsfwService { get; set; }

        [Command("r34")]
        [RequireNsfw]
        [Summary("Searches for posts in rule34.xxx")]
        public async Task<RuntimeResult> SearchR34Async([Remainder] string tags)
        {
            Rule34SearchParams searchParams = new()
            {
                limit = 750,
                tags = tags,
                json = 1
            };

            List<Rule34Post> nsfwPosts = null;
            using (Context.Channel.EnterTypingState()) nsfwPosts = await GetRule34PostsAsync(searchParams);

            if (nsfwPosts == null || nsfwPosts.Count == 0)
                return ExecutionResult.FromError("Rule34 returned no posts! Maybe one of your tags doesn't exist!");

            Rule34Post chosenPost = nsfwPosts.Where(x => x.Score >= GlobalConfig.Instance.LoadedConfig.Rule34Threshold).ToList().PickRandom();

            EmbedBuilder embed = new()
            {
                Color = new Color(36, 119, 0),
                Title = "RULE34 SEARCH"
            };

            string embedDescription = $"**Tags** : `{chosenPost.Tags.Truncate(512)}`\n";
            embedDescription += $"**Author** : `{chosenPost.Owner}`\n";
            embedDescription += $"**Score** : `{chosenPost.Score}`";

            embed.WithDescription(embedDescription);
            embed.WithImageUrl(chosenPost.FileUrl);
            embed.WithAuthor(author => author.Name = "SAMM-BOT COMMANDS");
            embed.WithFooter(footer => footer.Text = "Samm-Bot");
            embed.WithCurrentTimestamp();

            await Context.Channel.SendMessageAsync("", false, embed.Build());

            return ExecutionResult.Succesful();
        }

        public async Task<List<Rule34Post>> GetRule34PostsAsync(Rule34SearchParams searchParams)
        {
            string queryString = searchParams.ToQueryString();
            string jsonReply = string.Empty;

            using (HttpResponseMessage response = await NsfwService.NsfwClient.GetAsync($"index.php?page=dapi&s=post&q=index&{queryString}"))
            {
                jsonReply = await response.Content.ReadAsStringAsync();
            }

            List<Rule34Post> postsReply = JsonConvert.DeserializeObject<List<Rule34Post>>(jsonReply);

            return postsReply;
        }
    }
}
