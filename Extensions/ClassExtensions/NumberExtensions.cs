namespace SammBotNET.Extensions.ClassExtensions
{
	public static class NumberExtensions
	{
		public static float ToFahrenheit(this float Number)
		{
			return (Number * 9 / 5) + 32;
		}

		public static float ToPsi(this float Number)
		{
			return Number / 68.948f;
		}

		public static float KmhToMph(this float Number)
		{
			return Number / 1.609f;
		}

		public static float MpsToKmh(this float Number)
		{
			return Number * 3.6f;
		}
	}
}
