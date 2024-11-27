#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021 Analog Feelings
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

namespace SammBot.Library.Extensions;

/// <summary>
/// Contains extension methods for strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Truncates a string to a certain amount of characters and appends "..." to it.
    /// </summary>
    /// <param name="targetString">The string to truncate.</param>
    /// <param name="maxCharacters">The maximum amount of characters allowed.</param>
    /// <returns>The truncated string.</returns>
    /// <remarks>The function doesn't take into account the extra characters added by the dots.</remarks>
    public static string Truncate(this string targetString, int maxCharacters)
    {
        return targetString.Length <= maxCharacters ? targetString : targetString.Substring(0, maxCharacters) + "...";
    }

    /// <summary>
    /// Replaces a string's templates with the values specified in <paramref name="templateDictionary"/>.
    /// </summary>
    /// <param name="targetString">The target string to process.</param>
    /// <param name="templateDictionary">A dictionary of template strings with the objects to replace them with.</param>
    /// <returns>The processed string.</returns>
    /// <remarks>The template dictionary's keys must not be surrounded with "%".</remarks>
    public static string TemplateReplace(this string targetString, Dictionary<string, object?> templateDictionary)
    {
        foreach (KeyValuePair<string, object?> pair in templateDictionary)
        {
            targetString = targetString.Replace($"%{pair.Key}%", pair.Value?.ToString());
        }

        return targetString;
    }
    
    /// <summary>
    /// Calculates the Damerau-Levenshtein distance between a source and a target
    /// string, based on a threshold value.
    /// </summary>
    /// <param name="source">The source string.</param>
    /// <param name="target">The string to compare the source to.</param>
    /// <param name="threshold">The threshold value.</param>
    /// <returns>
    /// The distance between the two strings, or <see cref="int.MaxValue"/> if the
    /// distance exceeds the threshold.
    /// </returns>
    public static int DamerauDistance(this string source, string target, int threshold)
    {
        //Thanks Joshua Honig from StackOverflow :)
        void Swap<T>(ref T arg1, ref T arg2)
        {
            (arg1, arg2) = (arg2, arg1);
        }

        int sourceLength = source.Length;
        int targetLength = target.Length;

        // Return trivial case - difference in string lengths exceeds threshhold
        if (Math.Abs(sourceLength - targetLength) > threshold) { return int.MaxValue; }

        // Ensure arrays [i] / length1 use shorter length 
        if (sourceLength > targetLength)
        {
            Swap(ref target, ref source);
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
                int cost = source[im1] == target[jm1] ? 0 : 1;

                int del = dCurrent[im1] + 1;
                int ins = dMinus1[i] + 1;
                int sub = dMinus1[im1] + cost;

                //Fastest execution for min value of 3 integers
                int min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                if (i > 1 && j > 1 && source[im2] == target[jm1] && source[im1] == target[j - 2])
                    min = Math.Min(min, dMinus2[im2] + cost);

                dCurrent[i] = min;
                if (min < minDistance) { minDistance = min; }
                im1++;
                im2++;
            }
            jm1++;
            if (minDistance > threshold) { return int.MaxValue; }
        }

        int result = dCurrent[maxI];
        return (result > threshold) ? int.MaxValue : result;
    }
}