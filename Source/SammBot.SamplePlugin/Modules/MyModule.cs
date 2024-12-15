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

using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using SammBot.Library.Attributes;
using SammBot.Library.Models;
using SammBot.Library.Preconditions;
using SammBot.SamplePlugin.Services;

namespace SammBot.SamplePlugin.Modules;

[PrettyName("My Module")]
[Group("sample", "My very own Samm-Bot module!")]
[ModuleEmoji("\U0001f9ec")]
public class MyModule : InteractionModuleBase<ShardedInteractionContext>
{
    private readonly MyService _myService;

    public MyModule(IServiceProvider services)
    {
        _myService = services.GetRequiredService<MyService>();
    }

    [SlashCommand("hello", "Get a hello message!")]
    [DetailedDescription("Gets a hello message!")]
    [RateLimit(2, 1)]
    public async Task<RuntimeResult> HelloAsync()
    {
        string hello = _myService.SayHello();

        await RespondAsync(hello);
        
        return ExecutionResult.Succesful();
    }
}