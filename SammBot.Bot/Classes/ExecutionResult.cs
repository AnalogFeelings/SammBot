#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2022 AestheticalZ
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
#endregion

using Discord.Interactions;

namespace SammBot.Bot;

public class ExecutionResult : RuntimeResult
{
    public ExecutionResult(InteractionCommandError? Error, string Reason) : base(Error, Reason) { }
        
    public static ExecutionResult FromError(string Reason) =>
        new ExecutionResult(InteractionCommandError.Unsuccessful, Reason);
        
    public static ExecutionResult Succesful() =>
        new ExecutionResult(null, "Execution succesful.");
}