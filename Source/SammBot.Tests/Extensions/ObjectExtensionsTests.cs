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

using JetBrains.Annotations;
using SammBot.Library.Attributes;
using SammBot.Library.Extensions;

namespace SammBot.Tests.Extensions;

[TestClass]
public class ObjectExtensionsTests
{
    [TestMethod]
    public void ToQueryStringTest()
    {
        TestClass testObject = new TestClass();
        string actual = testObject.ToQueryString();
        string expected = "testString=hello!&testInt=2";
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }
}

file class TestClass
{
    [UglyName("testString")] 
    public string TestString { get; } = "hello!";
    
    [UglyName("testInt")] 
    public int TestInt { get; } = 2;
}