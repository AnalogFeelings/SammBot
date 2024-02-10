#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2024 Analog Feelings
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

public static class NumberExtensions
{
    public static float RoundTo(this float number, int decimalCount)
    {
        return (float)Math.Round(number, decimalCount);
    }

    public static float ToFahrenheit(this float number)
    {
        return ((number * 9 / 5) + 32).RoundTo(1);
    }

    public static float ToPsi(this float number)
    {
        return (number / 68.948f).RoundTo(2);
    }

    public static float KmhToMph(this float number)
    {
        return (number / 1.609f).RoundTo(1);
    }

    public static float MpsToKmh(this float number)
    {
        return (number * 3.6f).RoundTo(1);
    }
}