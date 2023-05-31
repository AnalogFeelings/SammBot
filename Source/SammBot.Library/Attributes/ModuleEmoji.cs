#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 Analog Feelings
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

namespace SammBot.Library.Attributes;

/// <summary>
/// An attribute used to denote a module's emoji to be used on
/// the bot's help command.
/// </summary>
/// <remarks>
/// Please use UTF escape sequences. Don't paste the emoji
/// into the code directly.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ModuleEmoji : Attribute
{
    /// <summary>
    /// The module emoji.
    /// </summary>
    public readonly string Emoji;

    /// <summary>
    /// Creates a new instance of the <see cref="ModuleEmoji"/> class.
    /// </summary>
    /// <param name="Emoji">The emoji to set.</param>
    public ModuleEmoji(string Emoji) => 
        this.Emoji = Emoji;
}