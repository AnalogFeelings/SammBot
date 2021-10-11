using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class EmotionalSupport
    {
        [Key] public int SupportId { get; set; }
        public string SupportMessage { get; set; }
    }
}
