using Discord;
using Discord.Commands;
using Discord.Interactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
    [Name("NSFW")]
    [Group("nsfw")]
    [Summary("NSFW commands. Only visible in NSFW channels.")]
    [ModuleEmoji("🔞")]
    public class NsfwModule : ModuleBase<SocketCommandContext>
    {
        public NsfwService NsfwService { get; set; }
        public Logger BotLogger { get; set; }

        [Command("r34")]
        [Alias("rule34")]
        [RequireNsfw]
        [Summary("Searches for posts in rule34.xxx")]
        public async Task<RuntimeResult> SearchR34Async([Remainder] string Tags)
        {
            Rule34SearchParams searchParams = new()
            {
                limit = 1000,
                tags = Tags,
                json = 1
            };

            List<Rule34Post> nsfwPosts = null;
            using (Context.Channel.EnterTypingState()) nsfwPosts = await GetRule34PostsAsync(searchParams);

            if (nsfwPosts == null || nsfwPosts.Count == 0)
                return ExecutionResult.FromError("Rule34 returned no posts! Maybe one of your tags doesn't exist!");

            List<Rule34Post> chosenPosts = nsfwPosts.Where(x => x.Score >= BotCore.Instance.LoadedConfig.Rule34Threshold
                                                            && !x.FileUrl.EndsWith(".mp4")).OrderByDescending(x => x.Score).ToList();

            string embedDescription = $"**Tags** : `{chosenPosts[0].Tags.Truncate(512)}`\n";
            embedDescription += $"**Author** : `{chosenPosts[0].Owner}`\n";
            embedDescription += $"**Score** : `{chosenPosts[0].Score}`";

            EmbedBuilder embed = new EmbedBuilder().BuildDefaultEmbed(Context, description: embedDescription);
            embed.ChangeTitle("Rule34 Search");
            embed.ChangeFooter(Context, $"Post 1/{chosenPosts.Count} | Requested by ${Context.Message.Author.GetFullUsername()}");
            embed.WithImageUrl(chosenPosts[0].FileUrl);

            ComponentBuilder componentBuilder = new ComponentBuilder()
                    .WithButton(emote: new Emoji("⏮"), customId: $"nsfw:start,{Context.Message.Id}");

            //My wish is that this code is so fucking horrendous that I dont have to touch paginated
            //embeds ever fucking again.
            IUserMessage message = await Context.Channel.SendMessageAsync("", false, embed.Build(), components: componentBuilder.Build());

            if (chosenPosts.Count > 1)
            {
                List<Emoji> emojiList = new() { new Emoji("⏮"), new Emoji("◀"), new Emoji("▶"), new Emoji("⏭"), new Emoji("❌") };

                await message.AddReactionsAsync(emojiList.ToArray());

                int page = 0;
                int pageMax = chosenPosts.Count;
                Stopwatch timer = new();

                timer.Start();
                while (timer.ElapsedMilliseconds <= 10000)
                {
                    try
                    {
                        foreach (Emoji emoji in emojiList)
                        {
                            IEnumerable<IUser> userEnumerable = await message.GetReactionUsersAsync(emoji, 4).FlattenAsync();
                            List<IUser> userList = userEnumerable.ToList();
                            if (userList.Count < 2) continue;

                            if (!userList.Any(x => x.Id == Context.Message.Author.Id))
                            {
                                foreach (IUser user in userList)
                                {
                                    await message.RemoveReactionAsync(emoji, user);
                                }
                                continue;
                            }
                            else
                            {
                                switch (emoji.Name)
                                {
                                    case "⏮":
                                        page = 0;
                                        break;
                                    case "◀":
                                        if (page != 0) page--;
                                        break;
                                    case "▶":
                                        if (page < pageMax) page++;
                                        break;
                                    case "⏭":
                                        page = pageMax - 1;
                                        break;
                                    case "❌":
                                        timer.Stop();
                                        await message.RemoveAllReactionsAsync();
                                        return ExecutionResult.Succesful();
                                    default:
                                        break;
                                }
                                embedDescription = $"**Tags** : `{chosenPosts[page].Tags.Truncate(512)}`\n";
                                embedDescription += $"**Author** : `{chosenPosts[page].Owner}`\n";
                                embedDescription += $"**Score** : `{chosenPosts[page].Score}`";

                                embed.WithDescription(embedDescription);
                                embed.WithImageUrl(chosenPosts[page].FileUrl);

                                embed.ChangeFooter(Context, $"Post {page + 1}/{pageMax} | Requested by ${Context.Message.Author.GetFullUsername()}");

                                await message.ModifyAsync(y => y.Embed = embed.Build());
                                await message.RemoveReactionAsync(emoji, Context.Message.Author);

                                timer.Restart();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        BotLogger.LogException(ex);
                        await message.RemoveAllReactionsAsync();
                        return ExecutionResult.FromError(ex.Message);
                    }
                }
                timer.Stop();
                await message.RemoveAllReactionsAsync();
            }

            return ExecutionResult.Succesful();
        }

        [ComponentInteraction("nsfw:*,*")]
        public async Task HandleInteractions(string operation, ulong id)
        {

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
