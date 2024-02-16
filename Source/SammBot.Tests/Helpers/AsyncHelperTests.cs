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

using SammBot.Library.Helpers;

namespace SammBot.Tests.Helpers;

[TestClass]
public class AsyncHelperTests
{
    private string _TestVariable = "Lorem";
    
    [TestMethod]
    public void RunSyncReturnTest()
    {
        uint actual = AsyncHelper.RunSync(() => ReturnAsync());
        uint expected = 0xDEADBEEF;
        
        Assert.IsTrue(actual == expected, $"Expected {expected}, got {actual}.");
    }
    
    [TestMethod]
    public void RunSyncNoReturnTest()
    {
        AsyncHelper.RunSync(() => NoReturnAsync());
        string expected = "Lorem Ipsum";
        
        Assert.IsTrue(_TestVariable == expected, $"Expected {expected}, got {_TestVariable}.");
    }

    private async Task<uint> ReturnAsync()
    {
        await Task.Delay(25);

        return 0xDEADBEEF;
    }

    private async Task NoReturnAsync()
    {
        await Task.Delay(25);

        _TestVariable = "Lorem Ipsum";
    }
}