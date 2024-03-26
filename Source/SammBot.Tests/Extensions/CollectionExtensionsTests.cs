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
public class CollectionExtensionsTests
{
    [TestMethod]
    public void IsNullTest()
    {
        IEnumerable<object>? nullCollection = null;
        bool expected = true;
        bool actual = nullCollection.IsNullOrEmpty();
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }
    
    [TestMethod]
    public void IsEmptyTest()
    {
        IEnumerable<object> emptyCollection = Array.Empty<object>();
        bool expected = true;
        bool actual = emptyCollection.IsNullOrEmpty();
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }
    
    [TestMethod]
    public void IsNotNullOrEmpty()
    {
        IEnumerable<object> emptyCollection = [ "hello", 42 ];
        bool expected = false;
        bool actual = emptyCollection.IsNullOrEmpty();
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }
}