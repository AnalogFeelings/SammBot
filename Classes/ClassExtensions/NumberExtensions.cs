using System;

namespace SammBotNET.Extensions.ClassExtensions
{
	public static class NumberExtensions
	{
		public static float RoundTo(this float Number, int DecimalCount)
		{
			return (float)Math.Round(Number, DecimalCount);
		}

		public static float ToFahrenheit(this float Number)
		{
			return ((Number * 9 / 5) + 32).RoundTo(1);
		}

		public static float ToPsi(this float Number)
		{
			return (Number / 68.948f).RoundTo(2);
		}

		public static float KmhToMph(this float Number)
		{
			return (Number / 1.609f).RoundTo(1);
		}

		public static float MpsToKmh(this float Number)
		{
			return (Number * 3.6f).RoundTo(1);
		}
	}
}
