namespace SammBotNET.Extensions
{
	public static class BoolExtensions
	{
		public static string ToYesNo(this bool Boolean)
		{
			return Boolean ? "Yes" : "No";
		}
	}
}
