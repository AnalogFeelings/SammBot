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

namespace SammBot.Library.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string TargetString, int MaxCharacters)
    {
        return TargetString.Length <= MaxCharacters ? TargetString : TargetString.Substring(0, MaxCharacters) + "...";
    }

    public static string CountryCodeToFlag(this string CountryCode)
    {
        return string.Concat(CountryCode.ToUpper().Select(x => char.ConvertFromUtf32(x + 0x1F1A5)));
    }

    public static string CapitalizeFirst(this string Target)
    {
        if (string.IsNullOrEmpty(Target)) throw new ArgumentException("Target string is null or empty.");

        string resultString = char.ToUpper(Target.First()) + Target.Substring(1).ToLower();

        return resultString;
    }

    //Thanks Joshua Honig from StackOverflow :)
    public static int DamerauDistance(this string Source, string Target, int Threshold)
    {
        void Swap<T>(ref T Arg1, ref T Arg2)
        {
            (Arg1, Arg2) = (Arg2, Arg1);
        }

        int sourceLength = Source.Length;
        int targetLength = Target.Length;

        // Return trivial case - difference in string lengths exceeds threshhold
        if (Math.Abs(sourceLength - targetLength) > Threshold) { return int.MaxValue; }

        // Ensure arrays [i] / length1 use shorter length 
        if (sourceLength > targetLength)
        {
            Swap(ref Target, ref Source);
            Swap(ref sourceLength, ref targetLength);
        }

        int maxI = sourceLength;
        int maxJ = targetLength;

        int[] dCurrent = new int[maxI + 1];
        int[] dMinus1 = new int[maxI + 1];
        int[] dMinus2 = new int[maxI + 1];
        int[] dSwap;

        for (int i = 0; i <= maxI; i++) { dCurrent[i] = i; }

        int jm1 = 0, im1 = 0, im2 = -1;

        for (int j = 1; j <= maxJ; j++)
        {
            // Rotate
            dSwap = dMinus2;
            dMinus2 = dMinus1;
            dMinus1 = dCurrent;
            dCurrent = dSwap;

            // Initialize
            int minDistance = int.MaxValue;
            dCurrent[0] = j;
            im1 = 0;
            im2 = -1;

            for (int i = 1; i <= maxI; i++)
            {
                int cost = Source[im1] == Target[jm1] ? 0 : 1;

                int del = dCurrent[im1] + 1;
                int ins = dMinus1[i] + 1;
                int sub = dMinus1[im1] + cost;

                //Fastest execution for min value of 3 integers
                int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                if (i > 1 && j > 1 && Source[im2] == Target[jm1] && Source[im1] == Target[j - 2])
                    min = Math.Min(min, dMinus2[im2] + cost);

                dCurrent[i] = min;
                if (min < minDistance) { minDistance = min; }
                im1++;
                im2++;
            }
            jm1++;
            if (minDistance > Threshold) { return int.MaxValue; }
        }

        int result = dCurrent[maxI];
        return (result > Threshold) ? int.MaxValue : result;
    }
}