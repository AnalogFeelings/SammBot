using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public partial class EmotionalSupport
    {
        [Key] public string SupportMessage { get; set; }
    }
}
