using System;
using System.Linq;

namespace SammBotNET.Extensions
{
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

            string Result = char.ToUpper(Target.First()) + Target.Substring(1).ToLower();

            return Result;
        }

        //Thanks Joshua Honig from StackOverflow :)
        public static int DamerauLevenshteinDistance(this string Source, string Target, int Threshold)
        {
            void Swap<T>(ref T Arg1, ref T Arg2)
            {
                T Temp = Arg1;
                Arg1 = Arg2;
                Arg2 = Temp;
            }

            int SourceLength = Source.Length;
            int TargetLength = Target.Length;

            // Return trivial case - difference in string lengths exceeds threshhold
            if (Math.Abs(SourceLength - TargetLength) > Threshold) { return int.MaxValue; }

            // Ensure arrays [i] / length1 use shorter length 
            if (SourceLength > TargetLength)
            {
                Swap(ref Target, ref Source);
                Swap(ref SourceLength, ref TargetLength);
            }

            int MaxI = SourceLength;
            int MaxJ = TargetLength;

            int[] dCurrent = new int[MaxI + 1];
            int[] dMinus1 = new int[MaxI + 1];
            int[] dMinus2 = new int[MaxI + 1];
            int[] dSwap;

            for (int i = 0; i <= MaxI; i++) { dCurrent[i] = i; }

            int jm1 = 0, im1 = 0, im2 = -1;

            for (int j = 1; j <= MaxJ; j++)
            {
                // Rotate
                dSwap = dMinus2;
                dMinus2 = dMinus1;
                dMinus1 = dCurrent;
                dCurrent = dSwap;

                // Initialize
                int MinDistance = int.MaxValue;
                dCurrent[0] = j;
                im1 = 0;
                im2 = -1;

                for (int i = 1; i <= MaxI; i++)
                {
                    int Cost = Source[im1] == Target[jm1] ? 0 : 1;

                    int del = dCurrent[im1] + 1;
                    int ins = dMinus1[i] + 1;
                    int sub = dMinus1[im1] + Cost;

                    //Fastest execution for min value of 3 integers
                    int Min = (del > ins) ? (ins > sub ? sub : ins) : (del > sub ? sub : del);

                    if (i > 1 && j > 1 && Source[im2] == Target[jm1] && Source[im1] == Target[j - 2])
                        Min = Math.Min(Min, dMinus2[im2] + Cost);

                    dCurrent[i] = Min;
                    if (Min < MinDistance) { MinDistance = Min; }
                    im1++;
                    im2++;
                }
                jm1++;
                if (MinDistance > Threshold) { return int.MaxValue; }
            }

            int Result = dCurrent[MaxI];
            return (Result > Threshold) ? int.MaxValue : Result;
        }
    }
}
