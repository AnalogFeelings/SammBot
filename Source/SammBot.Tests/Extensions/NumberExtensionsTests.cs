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

using SammBot.Library.Extensions;
using System.Globalization;

namespace SammBot.Tests.Extensions;

[TestClass]
public class NumberExtensionsTests
{
    [TestMethod]
    public void RoundToTest()
    {
        float actual = 3.9234234234f.RoundTo(2);
        float expected = 3.92f;

        string actualString = actual.ToString(CultureInfo.InvariantCulture);
        string expectedString = expected.ToString(CultureInfo.InvariantCulture);
        
        Assert.IsTrue(actualString == expectedString, $"Expected {expectedString}, got {actualString}.");
    }

    [TestMethod]
    public void ToFahrenheitTest()
    {
        float firstActual = 90f.ToFahrenheit();
        float firstExpected = 194f;
        
        string firstActualString = firstActual.ToString(CultureInfo.InvariantCulture);
        string firstExpectedString = firstExpected.ToString(CultureInfo.InvariantCulture);
        
        Assert.IsTrue(firstActualString == firstExpectedString, $"Expected {firstExpectedString}, got {firstActualString}.");

        float secondActual = 0f.ToFahrenheit();
        float secondExpected = 32f;
        
        string secondActualString = secondActual.ToString(CultureInfo.InvariantCulture);
        string secondExpectedString = secondExpected.ToString(CultureInfo.InvariantCulture);
        
        Assert.IsTrue(secondActualString == secondExpectedString, $"Expected {secondExpectedString}, got {secondActualString}.");
    }
}