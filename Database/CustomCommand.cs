using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class CustomCommand
    {
        [Key]
        public string name { get; set; }
        public ulong authorID { get; set; }
        public string reply { get; set; }
    }
}
