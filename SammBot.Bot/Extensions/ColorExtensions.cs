#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
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
using System;

namespace SammBot.Bot.Extensions;

public static class ColorExtensions
{
    public static string ToHexString(this SKColor Color)
    {
        return "#" + Color.Red.ToString("X2") + Color.Green.ToString("X2") + Color.Blue.ToString("X2");
    }

    public static string ToRgbString(this SKColor Color)
    {
        return "rgb(" + Color.Red.ToString() + ", " + Color.Green.ToString() + ", " + Color.Blue.ToString() + ")";
    }

    public static string ToCmykString(this SKColor Color)
    {
        //Convert 0 to 255 to 0 to 1
        float red = Color.Red / 255f;
        float green = Color.Green / 255f;
        float blue = Color.Blue / 255f;
            
        if(red == 0 && green == 0 && blue == 0) return $"cmyk(0%, 0%, 0%, 100)";

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
        //Convert 0 to 255 to 0 to 1
        float red = Color.Red / 255f;
        float green = Color.Green / 255f;
        float blue = Color.Blue / 255f;

        double cMax = Math.Max(red, Math.Max(green, blue));
        double cMin = Math.Min(red, Math.Min(green, blue));
        double cDifference = cMax - cMin;

        double h = -1;
        double s = -1;
        double v = 0;

        if (cMax == cMin) h = 0;
        else if (cMax == red) h = (60 * ((green - blue) / cDifference) + 360) % 360;
        else if (cMax == green) h = (60 * ((blue - red) / cDifference) + 120) % 360;
        else if (cMax == blue) h = (60 * ((red - green) / cDifference) + 240) % 360;

        if (cMax == 0) s = 0;
        else s = (cDifference / cMax) * 100;

        v = cMax * 100;

        h = Math.Round(h, 0);
        s = Math.Round(s, 0);
        v = Math.Round(v, 0);

        return $"hsv({h}, {s}%, {v}%)";
    }

    public static string ToHslString(this SKColor Color)
    {
        //Convert 0 to 255 to 0 to 1
        float red = Color.Red / 255f;
        float green = Color.Green / 255f;
        float blue = Color.Blue / 255f;

        double cMax = Math.Max(red, Math.Max(green, blue));
        double cMin = Math.Min(red, Math.Min(green, blue));
        double cDifference = cMax - cMin;

        double h = 0;
        double s = 0;
        double l = (cMax + cMin) / 2f;

        if (cDifference != 0)
        {
            if (l <= 0.5f) s = cDifference / (cMax + cMin);
            else s = cDifference / (2f - cDifference);

            if (cMax == red) h = (green - blue) / cDifference + (green < blue ? 6 : 0);
            else if (cMax == green) h = 2f + (blue - red) / cDifference;
            else if (cMax == blue) h = 4f + (red - green) / cDifference;

            h /= 6;
        }

        h *= 360; h = Math.Round(h, 0);
        s *= 100; s = Math.Round(s, 0);
        l *= 100; l = Math.Round(l, 0);

        return $"hsl({h}, {s}%, {l}%)";
    }
}