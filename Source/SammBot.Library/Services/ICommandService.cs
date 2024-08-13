﻿#region License Information (GPLv3)
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
// along with this program.  If not, see &lt;https://www.gnu.org/licenses/&gt;.
#endregion

namespace SammBot.Library.Services;

/// <summary>
/// Defines an interface for a command handling service.
/// </summary>
public interface ICommandService
{
    /// <summary>
    /// Initializes the command handler.
    /// </summary>
    public Task InitializeHandlerAsync();
}