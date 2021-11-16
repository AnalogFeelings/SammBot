using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class Pronoun
    {
        [Key]
        public ulong UserId { get; set; }
        public string Subject { get; set; }
        public string Object { get; set; }
        public string DependentPossessive { get; set; }
        public string IndependentPossessive { get; set; }
        public string Reflexive { get; set; }
    }
}
