using Newtonsoft.Json;
using System.Collections.Generic;

namespace SammBotNET.RestDefinitions
{
	public class UrbanSearchParams
	{
		public string term { get; set; }
	}

	public class UrbanDefinitionList
	{
		[JsonProperty("list")]
		public List<UrbanDefinition> List { get; set; }
	}

	public class UrbanDefinition
	{
		[JsonProperty("definition")]
		public string Definition { get; set; }
		[JsonProperty("permalink")]
		public string Permalink { get; set; }
		[JsonProperty("thumbs_up")]
		public int ThumbsUp { get; set; }
		[JsonProperty("thumbs_down")]
		public int ThumbsDown { get; set; }
		[JsonProperty("sound_urls")]
		public List<string> SoundUrls { get; set; }
		[JsonProperty("author")]
		public string Author { get; set; }
		[JsonProperty("word")]
		public string Word { get; set; }
		[JsonProperty("defid")]
		public long DefinitionId { get; set; }
		[JsonProperty("current_vote")]
		public string CurrentVote { get; set; }
		[JsonProperty("written_on")]
		public string WrittenOn { get; set; }
		[JsonProperty("example")]
		public string Example { get; set; }
	}
}
