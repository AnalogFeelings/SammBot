using Discord;
using Discord.Commands;
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
		[Summary("Searches for posts in rule34.xxx")]
		[RequireNsfw]
		public async Task<RuntimeResult> SearchR34Async([Remainder] string Tags)
		{
			Rule34SearchParams SearchParameters = new Rule34SearchParams()
			{
				limit = 1000,
				tags = Tags,
				json = 1
			};

			List<Rule34Post> NsfwPosts = null;
			using (Context.Channel.EnterTypingState()) NsfwPosts = await GetRule34PostsAsync(SearchParameters);

			if (NsfwPosts == null || NsfwPosts.Count == 0)
				return ExecutionResult.FromError("Rule34 returned no posts! Maybe one of your tags doesn't exist!");

			List<Rule34Post> FilteredPosts = NsfwPosts.Where(x => x.Score >= Settings.Instance.LoadedConfig.Rule34Threshold
															&& !x.FileUrl.EndsWith(".mp4")).OrderByDescending(x => x.Score).ToList();

			string EmbedDescription = $"**Tags** : `{FilteredPosts[0].Tags.Truncate(512)}`\n";
			EmbedDescription += $"**Author** : `{FilteredPosts[0].Owner}`\n";
			EmbedDescription += $"**Score** : `{FilteredPosts[0].Score}`";

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, Description: EmbedDescription);
			ReplyEmbed.ChangeTitle("Rule34 Search");
			ReplyEmbed.ChangeFooter(Context, $"Post 1/{FilteredPosts.Count} | Requested by {Context.Message.Author.GetFullUsername()}");
			ReplyEmbed.WithImageUrl(FilteredPosts[0].FileUrl);

			//My wish is that this code is so fucking horrendous that I dont have to touch paginated
			//embeds ever fucking again.
			IUserMessage ReplyMessage = await Context.Channel.SendMessageAsync("", false, ReplyEmbed.Build());

			if (FilteredPosts.Count > 1)
			{
				List<Emoji> ReactionList = new List<Emoji>() { new Emoji("⏮"), new Emoji("◀"), new Emoji("▶"), new Emoji("⏭"), new Emoji("❌") };

				await ReplyMessage.AddReactionsAsync(ReactionList.ToArray());

				int CurrentPage = 0;
				int MaxPage = FilteredPosts.Count;
				Stopwatch Timer = new Stopwatch();

				Timer.Start();
				while (Timer.ElapsedMilliseconds <= 10000)
				{
					try
					{
						foreach (Emoji Reaction in ReactionList)
						{
							IEnumerable<IUser> UserEnumerable = await ReplyMessage.GetReactionUsersAsync(Reaction, 4).FlattenAsync();
							List<IUser> UserList = UserEnumerable.ToList();
							if (UserList.Count < 2) continue;

							if (!UserList.Any(x => x.Id == Context.Message.Author.Id))
							{
								foreach (IUser User in UserList)
								{
									await ReplyMessage.RemoveReactionAsync(Reaction, User);
								}
								continue;
							}
							else
							{
								switch (Reaction.Name)
								{
									case "⏮":
										CurrentPage = 0;
										break;
									case "◀":
										if (CurrentPage != 0) CurrentPage--;
										break;
									case "▶":
										if (CurrentPage < MaxPage) CurrentPage++;
										break;
									case "⏭":
										CurrentPage = MaxPage - 1;
										break;
									case "❌":
										Timer.Stop();
										await ReplyMessage.RemoveAllReactionsAsync();
										return ExecutionResult.Succesful();
									default:
										break;
								}
								EmbedDescription = $"**Tags** : `{FilteredPosts[CurrentPage].Tags.Truncate(512)}`\n";
								EmbedDescription += $"**Author** : `{FilteredPosts[CurrentPage].Owner}`\n";
								EmbedDescription += $"**Score** : `{FilteredPosts[CurrentPage].Score}`";

								ReplyEmbed.WithDescription(EmbedDescription);
								ReplyEmbed.WithImageUrl(FilteredPosts[CurrentPage].FileUrl);

								ReplyEmbed.ChangeFooter(Context, $"Post {CurrentPage + 1}/{MaxPage} | Requested by {Context.Message.Author.GetFullUsername()}");

								await ReplyMessage.ModifyAsync(y => y.Embed = ReplyEmbed.Build());
								await ReplyMessage.RemoveReactionAsync(Reaction, Context.Message.Author);

								Timer.Restart();
							}
						}
					}
					catch (Exception ex)
					{
						BotLogger.LogException(ex);
						await ReplyMessage.RemoveAllReactionsAsync();
						return ExecutionResult.FromError(ex.Message);
					}
				}
				Timer.Stop();
				await ReplyMessage.RemoveAllReactionsAsync();
			}

			return ExecutionResult.Succesful();
		}

		public async Task<List<Rule34Post>> GetRule34PostsAsync(Rule34SearchParams SearchParameters)
		{
			string QueryString = SearchParameters.ToQueryString();
			string JsonReply = string.Empty;

			using (HttpResponseMessage HttpResponse = await NsfwService.NsfwClient.GetAsync($"index.php?page=dapi&s=post&q=index&{QueryString}"))
			{
				JsonReply = await HttpResponse.Content.ReadAsStringAsync();
			}

			List<Rule34Post> RetrievedPosts = JsonConvert.DeserializeObject<List<Rule34Post>>(JsonReply);

			return RetrievedPosts;
		}
	}
}
