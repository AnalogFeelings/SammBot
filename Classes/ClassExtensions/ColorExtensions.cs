using System.Drawing;

namespace SammBotNET.Classes.ClassExtensions
{
	public static class ColorExtensions
	{
		public static string ToHexString(this Color Color)
		{
			return "#" + Color.R.ToString("X2") + Color.G.ToString("X2") + Color.B.ToString("X2");
		}

		public static string ToRgbString(this Color Color)
		{
			return "RGB(" + Color.R.ToString() + "," + Color.G.ToString() + "," + Color.B.ToString() + ")";
		}
	}
}
