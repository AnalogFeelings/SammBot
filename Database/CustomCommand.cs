using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class CustomCommand
    {
        [Key]
        public string Name { get; set; }
        public string Reply { get; set; }
        public ulong AuthorId { get; set; }
        public long CreatedAt { get; set; }
    }
}
