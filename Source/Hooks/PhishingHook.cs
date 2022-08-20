//using Discord;
//using Discord.WebSocket;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Net.Http;
//using System.Text.RegularExpressions;
//using System.Threading;
//using System.Threading.Tasks;

//namespace SammBotNET.Hooks
//{
//	[RegisterHook]
//	public class PhishingHook : MessageHook
//	{
//		public readonly HttpClient HttpClient;

//		public List<string> PhishingUrls = new List<string>();
//		public Regex UrlRegex = new Regex(@"(?:(?:https?|ftp|mailto):\/\/)?(?:www\.)?(([^\/\s]+\.[a-z\.]+)(\/[^\s]*)?)(?:\/)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

//		private object PhishingListLock = new object();
//		private Timer ListRedownloadTimer;

//		public const string PHISHING_LIST_URL = @"https://raw.githubusercontent.com/nikolaischunk/discord-phishing-links/main/txt/domain-list.txt";

//		public PhishingHook()
//		{
//			HttpClient = new HttpClient();
//			HttpClient.Timeout = TimeSpan.FromSeconds(5);

//			TimeSpan TimerDelay = TimeSpan.FromSeconds(Settings.Instance.LoadedConfig.PhishingUrlRedownloadInterval);
//			ListRedownloadTimer = new Timer(RedownloadList, null, TimerDelay, TimerDelay);

//			Task DownloadTask = Task.Run(() => RedownloadList(null));
//			DownloadTask.Wait();
//		}

//		public async void RedownloadList(object State)
//		{
//			try
//			{
//				string DownloadedList = await HttpClient.GetStringAsync(PHISHING_LIST_URL);

//				using (StringReader Reader = new StringReader(DownloadedList))
//				{
//					lock(PhishingListLock)
//					{
//						string Line;
//						while ((Line = Reader.ReadLine()) != null)
//						{
//							PhishingUrls.Add(Line);
//						}
//					}
//				}

//				BotLogger.Log($"Downloaded phishing URL list from GitHub successfully.", LogSeverity.Debug);
//			}
//			catch(Exception ex)
//			{
//				BotLogger.Log($"Failed to fetch phishing URL list from GitHub. Exception message: {ex.Message}", LogSeverity.Error);
//			}
//		}

//		public override async Task ExecuteHook()
//		{
//			if (Context.Channel is not SocketGuildChannel) return;

//			Match Match = UrlRegex.Match(Context.Message.Content);
//			if(Match.Success)
//			{
//				Group DomainGroup = Match.Groups[2];

//				//Looks ugly, but C# wont let me await inside the lock body.
//				bool IsPhishingUrl;
//				lock(PhishingListLock)
//				{
//					IsPhishingUrl = PhishingUrls.Contains(DomainGroup.Value);
//				}

//				if (IsPhishingUrl)
//				{
//					await Context.Message.DeleteAsync();

//					EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context)
//						.WithTitle("⚠️ Phishing link detected in message.").WithColor(new Color(255, 205, 77));

//					ReplyEmbed.Description = "The anti-phishing message hook has detected a very likely phishing link in a message, and has deleted it.\n\n";
//					ReplyEmbed.Description += $"**Sender**: <@{Context.Message.Author.Id}>";

//					ReplyEmbed.WithFooter(x => { x.Text = "Automated action"; x.IconUrl = Context.Client.CurrentUser.GetGuildGlobalOrDefaultAvatar(128); });

//					await ReplyAsync(null, embed: ReplyEmbed.Build());
//				}
//			}
//		}
//	}
//}
