using Discord;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SammBotNET.Hooks
{
	[RegisterHook]
	public class PhishingHook : MessageHook
	{
		public readonly HttpClient HttpClient = new HttpClient();

		public List<string> PhishingUrls = new List<string>();
		public Regex UrlRegex = new Regex(@"(?:(?:https?|ftp|mailto):\/\/)?(?:www\.)?(([^\/\s]+\.[a-z\.]+)(\/[^\s]*)?)(?:\/)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		public const string PHISHING_LIST_URL = @"https://raw.githubusercontent.com/nikolaischunk/discord-phishing-links/main/txt/domain-list.txt";

		public override async Task ExecuteHook()
		{
			if (PhishingUrls.Count == 0)
			{
				string DownloadedList = await HttpClient.GetStringAsync(PHISHING_LIST_URL);

				using (StringReader Reader = new StringReader(DownloadedList))
				{
					string Line;
					while ((Line = Reader.ReadLine()) != null)
					{
						PhishingUrls.Add(Line);
					}
				}
			}

			Match Match = UrlRegex.Match(Context.Message.Content);
			if(Match.Success)
			{
				Group DomainGroup = Match.Groups[2];

				if (PhishingUrls.Contains(DomainGroup.Value))
				{
					await Context.Message.DeleteAsync();

					EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
						.WithTitle("⚠️ Phishing link detected in message.").WithColor(new Color(255, 205, 77));

					ReplyEmbed.Description = "The anti-phishing message hook has detected a very likely phishing link in a message, and has deleted it.\n\n";
					ReplyEmbed.Description += $"**Sender**: <@{Context.Message.Author.Id}>";

					ReplyEmbed.WithFooter(x => { x.Text = "Automated action"; x.IconUrl = Context.Client.CurrentUser.GetGuildGlobalOrDefaultAvatar(128); });

					await ReplyAsync(null, embed: ReplyEmbed.Build());
				}
			}
		}
	}
}
