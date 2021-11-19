namespace SammBotNET.Extensions
{
    public static class BoolExtensions
    {
        public static string ToYesNo(this bool boolean)
        {
            return boolean ? "Yes" : "No";
        }
    }
}
