using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public class UserWarning
    {
        [Key]
        public string Id { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
        public string Reason { get; set; }
        public long Date { get; set; }
    }
}
