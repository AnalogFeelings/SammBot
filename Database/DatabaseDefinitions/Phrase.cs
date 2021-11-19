using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class Phrase
    {
        [Key]
        public string Content { get; set; }
        public ulong AuthorId { get; set; }
        public ulong ServerId { get; set; }
        public long CreatedAt { get; set; }
    }
}
