using SkiaSharp;
using System;

namespace SammBotNET.Extensions
{
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
            float R = Color.Red / 255f;
            float G = Color.Green / 255f;
            float B = Color.Blue / 255f;

            double Kf = 1 - Math.Max(R, Math.Max(G, B));
            double Cf = (1 - R - Kf) / (1 - Kf);
            double Mf = (1 - G - Kf) / (1 - Kf);
            double Yf = (1 - B - Kf) / (1 - Kf);

            //Use 0 to 100 instead of 0 to 1
            int Ki = (int)Math.Round(Kf * 100, 0);
            int Ci = (int)Math.Round(Cf * 100, 0);
            int Mi = (int)Math.Round(Mf * 100, 0);
            int Yi = (int)Math.Round(Yf * 100, 0);

            return $"cmyk({Ci}%, {Mi}%, {Yi}%, {Ki})";
        }

        public static string ToHsvString(this SKColor Color)
        {
            //Convert 0 to 255 to 0 to 1
            float R = Color.Red / 255f;
            float G = Color.Green / 255f;
            float B = Color.Blue / 255f;

            double CMax = Math.Max(R, Math.Max(G, B));
            double CMin = Math.Min(R, Math.Min(G, B));
            double CDifference = CMax - CMin;

            double H = -1;
            double S = -1;
            double V = 0;

            if (CMax == CMin) H = 0;
            else if (CMax == R) H = (60 * ((G - B) / CDifference) + 360) % 360;
            else if (CMax == G) H = (60 * ((B - R) / CDifference) + 120) % 360;
            else if (CMax == B) H = (60 * ((R - G) / CDifference) + 240) % 360;

            if (CMax == 0) S = 0;
            else S = (CDifference / CMax) * 100;

            V = CMax * 100;

            H = Math.Round(H, 0);
            S = Math.Round(S, 0);
            V = Math.Round(V, 0);

            return $"hsv({H}, {S}%, {V}%)";
        }

        public static string ToHslString(this SKColor Color)
        {
            //Convert 0 to 255 to 0 to 1
            float R = Color.Red / 255f;
            float G = Color.Green / 255f;
            float B = Color.Blue / 255f;

            double CMax = Math.Max(R, Math.Max(G, B));
            double CMin = Math.Min(R, Math.Min(G, B));
            double CDifference = CMax - CMin;

            double H = 0;
            double S = 0;
            double L = (CMax + CMin) / 2f;

            if (CDifference != 0)
            {
                if (L <= 0.5f) S = CDifference / (CMax + CMin);
                else S = CDifference / (2f - CDifference);

                if (CMax == R) H = (G - B) / CDifference + (G < B ? 6 : 0);
                else if (CMax == G) H = 2f + (B - R) / CDifference;
                else if (CMax == B) H = 4f + (R - G) / CDifference;

                H /= 6;
            }

            H *= 360; H = Math.Round(H, 0);
            S *= 100; S = Math.Round(S, 0);
            L *= 100; L = Math.Round(L, 0);

            return $"hsl({H}, {S}%, {L}%)";
        }
    }
}
