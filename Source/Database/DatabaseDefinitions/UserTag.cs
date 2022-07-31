using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
	public class UserTag
	{
		[Key]
		public string Id { get; set; }
		public string Name { get; set; }
		public string Reply { get; set; }
		public ulong AuthorId { get; set; }
		public ulong GuildId { get; set; }
		public long CreatedAt { get; set; }
	}
}
