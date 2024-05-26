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

namespace SammBot.Tests.Extensions;

[TestClass]
public class StringExtensionsTests
{
    [TestMethod]
    public void TruncateTest()
    {
        string actual = "Lorem ipsum dolor sit amet".Truncate(11);
        string expected = "Lorem ipsum...";

        Assert.IsTrue(actual == expected, $"Expected \"{expected}\", got \"{actual}\".");
    }

    [TestMethod]
    public void TemplateReplaceTest()
    {
        string templateString = "The favorite food of user %user% is %favoriteFood%.";
        Dictionary<string, object?> templateDict = new Dictionary<string, object?>()
        {
                ["user"] = "Analog Feelings",
                ["favoriteFood"] = "Ramen"
        };

        string actual = templateString.TemplateReplace(templateDict);
        string expected = "The favorite food of user Analog Feelings is Ramen.";
        
        Assert.IsTrue(actual == expected, $"Expected \"{expected}\", got \"{actual}\".");
    }

    [TestMethod]
    public void CountryCodeToFlagTest()
    {
        string actualFirst = "ES".CountryCodeToFlag();
        string expectedFirst = "\U0001f1ea\U0001f1f8";

        Assert.IsTrue(actualFirst == expectedFirst, $"Expected \"{expectedFirst}\", got \"{actualFirst}\".");

        string actualSecond = "US".CountryCodeToFlag();
        string expectedSecond = "\U0001f1fa\U0001f1f8";

        Assert.IsTrue(actualSecond == expectedSecond, $"Expected \"{expectedSecond}\", got \"{actualSecond}\".");
    }

    [TestMethod]
    public void CapitalizeFirstTest()
    {
        string actual = "lorem ipsum".CapitalizeFirst();
        string expected = "Lorem ipsum";

        Assert.IsTrue(actual == expected, $"Expected \"{expected}\", got \"{actual}\".");
    }

    [TestMethod]
    public void DamerauDistanceTest()
    {
        int actualFirst = "lorem psum".DamerauDistance("lorem ipsum", 3);
        int expectedFirst = 1;

        Assert.IsTrue(actualFirst == expectedFirst, $"Expected {expectedFirst}, got {actualFirst}.");

        int actualSecond = "loren ipun".DamerauDistance("lorem ipsum", 2);
        int expectedSecond = int.MaxValue;

        Assert.IsTrue(actualSecond == expectedSecond, $"Expected {expectedSecond}, got {actualSecond}.");
    }
}