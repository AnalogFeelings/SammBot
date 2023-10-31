#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using SkiaSharp;

namespace SammBot.Library.Extensions;

public static class ColorExtensions
{
    public static string ToHexString(this SKColor Color)
    {
        return "#" + Color.Red.ToString("X2") + Color.Green.ToString("X2") + Color.Blue.ToString("X2");
    }

    public static string ToRgbString(this SKColor Color)
    {
        return "rgb(" + Color.Red + ", " + Color.Green + ", " + Color.Blue + ")";
    }

    public static string ToCmykString(this SKColor Color)
    {
        //Convert 0 to 255 to 0 to 1
        float red = Color.Red / 255f;
        float green = Color.Green / 255f;
        float blue = Color.Blue / 255f;
            
        if(red == 0 && green == 0 && blue == 0) 
            return "cmyk(0%, 0%, 0%, 100)";

        double kf = 1 - Math.Max(red, Math.Max(green, blue));
        double cf = (1 - red - kf) / (1 - kf);
        double mf = (1 - green - kf) / (1 - kf);
        double yf = (1 - blue - kf) / (1 - kf);

        //Use 0 to 100 instead of 0 to 1
        int ki = (int)Math.Round(kf * 100, 0);
        int ci = (int)Math.Round(cf * 100, 0);
        int mi = (int)Math.Round(mf * 100, 0);
        int yi = (int)Math.Round(yf * 100, 0);

        return $"cmyk({ci}%, {mi}%, {yi}%, {ki})";
    }

    public static string ToHsvString(this SKColor Color)
    {
        Color.ToHsv(out float h, out float s, out float v);

        double roundedHue = Math.Round(h, 0);
        double roundedSaturation = Math.Round(s, 0);
        double roundedValue = Math.Round(v, 0);

        return $"hsv({roundedHue}, {roundedSaturation}%, {roundedValue}%)";
    }

    public static string ToHslString(this SKColor Color)
    {
        Color.ToHsl(out float h, out float s, out float l);

        double roundedHue = Math.Round(h, 0);
        double roundedSaturation = Math.Round(s, 0);
        double roundedLuminosity = Math.Round(l, 0);

        return $"hsl({roundedHue}, {roundedSaturation}%, {roundedLuminosity}%)";
    }
}