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

using Discord.Interactions;

namespace SammBot.Library.Models;

/// <summary>
/// Represents the result of an interaction execution.
/// </summary>
/// <seealso cref="RuntimeResult"/>
public class ExecutionResult : RuntimeResult
{
    /// <summary>
    /// Creates a new instance of the <see cref="ExecutionResult"/> class.
    /// </summary>
    /// <param name="error">The interaction error, if any.</param>
    /// <param name="reason">The reason or explanation of the error or success.</param>
    /// <remarks>If <paramref name="error"/> is null, this will be interpreted as successful.</remarks>
    public ExecutionResult(InteractionCommandError? error, string reason) : base(error, reason) { }
        
    /// <summary>
    /// Convenience method to create an unsuccessful <see cref="ExecutionResult"/>.
    /// </summary>
    /// <param name="reason">The error reason or explanation.</param>
    /// <returns>A new <see cref="ExecutionResult"/> object with <paramref name="reason"/>.</returns>
    public static ExecutionResult FromError(string reason) =>
        new ExecutionResult(InteractionCommandError.Unsuccessful, reason);
        
    /// <summary>
    /// Convenience method to create a successful <see cref="ExecutionResult"/>.
    /// </summary>
    /// <returns>A new successful <see cref="ExecutionResult"/> object.</returns>
    public static ExecutionResult Succesful() =>
        new ExecutionResult(null, "Execution succesful.");
}