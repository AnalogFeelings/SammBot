using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class Pronoun
    {
        [Key]
        public ulong UserId { get; set; }
        public EnglishPronoun Pronouns { get; set; }
    }
}
