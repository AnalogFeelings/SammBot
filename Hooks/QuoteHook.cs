using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Hooks
{
	[RegisterHook]
	public class QuoteHook : MessageHook
	{
		public override async Task ExecuteHook()
		{
			try
			{
				if (Message.Content.Length < 20 || Message.Content.Length > 64) return;
				if (Message.Attachments.Count > 0 && Message.Content.Length == 0) return;
				if (Message.MentionedUsers.Count > 0) return;
				if (Settings.Instance.UrlRegex.IsMatch(Message.Content)) return;
				if (Settings.Instance.LoadedConfig.BannedPrefixes.Any(x => Message.Content.StartsWith(x))) return;

				using (PhrasesDB PhrasesDatabase = new PhrasesDB())
				{
					List<Phrase> QuoteList = await PhrasesDatabase.Phrase.ToListAsync();
					foreach (Phrase Quote in QuoteList)
					{
						if (Message.Content == Quote.Content) return;
					}

					await PhrasesDatabase.AddAsync(new Phrase
					{
						Content = Message.Content,
						AuthorId = Message.Author.Id,
						ServerId = Context.Guild.Id,
						CreatedAt = Message.Timestamp.ToUnixTimeSeconds()
					});
					await PhrasesDatabase.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				BotLogger.LogException(ex);
			}
		}
	}
}
