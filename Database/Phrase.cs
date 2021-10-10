using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class Phrase
    {
        [Key]
        public string content { get; set; }
        public ulong authorID { get; set; }
        public ulong serverID { get; set; }
    }
}
