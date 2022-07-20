using SkiaSharp;
using System.Drawing;

namespace SammBotNET.Classes.ClassExtensions
{
	public static class ColorExtensions
	{
		public static string ToHexString(this SKColor Color)
		{
			return "#" + Color.Red.ToString("X2") + Color.Green.ToString("X2") + Color.Blue.ToString("X2");
		}

		public static string ToRgbString(this SKColor Color)
		{
			return "RGB(" + Color.Red.ToString() + "," + Color.Green.ToString() + "," + Color.Blue.ToString() + ")";
		}
	}
}
