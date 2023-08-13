#region License Information (GPLv3)
// Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
// Copyright (C) 2021-2023 Analog Feelings
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using SammBot.Library.Extensions;
using SkiaSharp;

namespace SammBot.Tests.Extensions;

[TestClass]
public class ColorExtensionsTests
{
    [TestMethod]
    public void ToHexStringTest()
    {
        SKColor color = new SKColor(42, 174, 189);
        string actual = color.ToHexString();
        string expected = "#2AAEBD";
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }

    [TestMethod]
    public void ToRgbStringTest()
    {
        SKColor color = new SKColor(42, 174, 189);
        string actual = color.ToRgbString();
        string expected = "rgb(42, 174, 189)";
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }

    [TestMethod]
    public void ToCmykStringTest()
    {
        // Test black handling.
        SKColor firstColor = new SKColor(0, 0, 0);
        string firstActual = firstColor.ToCmykString();
        string firstExpected = "cmyk(0%, 0%, 0%, 100)";
        
        Assert.IsTrue(firstActual == firstExpected, $"Expected {firstExpected}, got {firstActual}.");
        
        // Test other colors.
        SKColor secondColor = new SKColor(42, 174, 189);
        string secondActual = secondColor.ToCmykString();
        string secondExpected = "cmyk(78%, 8%, 0%, 26)";
        
        Assert.IsTrue(secondActual == secondExpected, $"Expected {secondExpected}, got {secondActual}.");
    }
    
    [TestMethod]
    public void ToHsvStringTest()
    {
        SKColor color = new SKColor(42, 174, 189);
        string actual = color.ToHsvString();
        string expected = "hsv(186, 78%, 74%)";
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }
    
    [TestMethod]
    public void ToHslStringTest()
    {
        SKColor color = new SKColor(42, 174, 189);
        string actual = color.ToHslString();
        string expected = "hsl(186, 64%, 45%)";
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }
}