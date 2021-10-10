using System.ComponentModel.DataAnnotations;

namespace SammBotNET.Database
{
    public class BlacklistedUser
    {
        [Key] public ulong userID { get; set; }
        public string banType { get; set; }
    }
}
