#region License Information (GPLv3)
/*
 * Samm-Bot - A lightweight Discord.NET bot for moderation and other purposes.
 * Copyright (C) 2021-2023 AestheticalZ
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using SammBot.Bot.Common;
using SammBot.Bot.Common.Attributes;
using SammBot.Bot.Common.Preconditions;
using SammBot.Bot.Core;
using SammBot.Bot.Database;
using SammBot.Bot.Database.Models;
using SammBot.Bot.Extensions;

namespace SammBot.Bot.Modules;

[PrettyName("Server Settings")]
[Group("scfg", "Server settings such as logging, welcome messages, etc.")]
[ModuleEmoji("\u2699\uFE0F")]
public class GuildConfigModule : InteractionModuleBase<ShardedInteractionContext>
{
    [SlashCommand("list", "Lists all the available settings and their current values.")]
    [DetailedDescription("Lists all the available settings and their current values.")]
    [RateLimit(3, 3)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task<RuntimeResult> ListSettingsAsync()
    {
        await DeferAsync(true);
            
        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Server Settings");

        replyEmbed.Title = "\u2699\uFE0F Server Settings";
        replyEmbed.Description = "These are all the server settings available.\n" +
                                 "Their real names and current values will also be listed.";
        replyEmbed.Color = new Color(102, 117, 127);
            
        List<PropertyInfo> propertyList = typeof(GuildConfig).GetProperties().Where(x => x.Name != "Id" && x.Name != "GuildId").ToList();
            
        using (BotDatabase botDatabase = new BotDatabase())
        {
            GuildConfig serverSettings = await botDatabase.GuildConfigs.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id) 
                                         ?? new GuildConfig()
                                         {
                                             GuildId = Context.Guild.Id
                                         };

            foreach (PropertyInfo property in propertyList)
            {
                string valueString;
                object? propertyValue = property.GetValue(serverSettings, null);

                // Property is a collection.
                // String is an IEnumerable too, so check for that.
                if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
                {
                    valueString = string.Join(", ", propertyValue);
                }
                else valueString = propertyValue!.ToString()!; // ???
                
                // It's impossible to have more than one PrettyName attribute, so use Single().
                PrettyName propertyFullName = property.GetCustomAttributes(false).OfType<PrettyName>().Single();
                
                // Small blue diamond emoji.
                string propertyName = "\U0001f539 ";

                propertyName += !string.IsNullOrEmpty(propertyFullName.Name) ? propertyFullName.Name : property.Name;
                propertyName += $"\n(Name: `{property.Name}`)";
                
                // It's also impossible to have more than one DetailedDescription attribute, so use Single().
                DetailedDescription propertyFullDescription = property.GetCustomAttributes(false).OfType<DetailedDescription>().Single();
                
                string propertyDescription = !string.IsNullOrEmpty(propertyFullDescription.Description) ? propertyFullDescription.Description : "No description.";

                propertyDescription += $"\n**• Current Value**: `{valueString}`";

                replyEmbed.AddField(propertyName, propertyDescription);
            }
        }
            
        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);

        return ExecutionResult.Succesful();
    }

    [SlashCommand("set", "Sets the value of a setting.")]
    [DetailedDescription("Sets the value of a server setting.\nYou must use the real name (\"EnableLogging\", not \"Enable Logging\").")]
    [RateLimit(2, 3)]
    [RequireContext(ContextType.Guild)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task<RuntimeResult> SetSettingAsync([Summary(description: "The name of the setting you want to set.")] string SettingName,
        [Summary(description: "The value you want to set it to.")] string SettingValue)
    {
        if (SettingName == "GuildId") return ExecutionResult.FromError("You cannot modify this setting.");

        PropertyInfo? targetProperty = typeof(GuildConfig).GetProperty(SettingName);
        object? newValue;
            
        if (targetProperty == null) return ExecutionResult.FromError("This setting does not exist! Check your spelling.");
            
        await DeferAsync(true);
            
        using (BotDatabase botDatabase = new BotDatabase())
        {
            GuildConfig? serverSettings = await botDatabase.GuildConfigs.FirstOrDefaultAsync(x => x.GuildId == Context.Guild.Id);
                
            if (serverSettings == default(GuildConfig))
            {
                serverSettings = new GuildConfig()
                {
                    GuildId = Context.Guild.Id
                };

                await botDatabase.AddAsync(serverSettings);
            }
                
            // Convert.ChangeType cannot handle enums well.
            // Add special case.
            if(targetProperty.PropertyType.IsEnum)
                targetProperty.SetValue(serverSettings, Enum.Parse(targetProperty.PropertyType, SettingValue, true));
            else
                targetProperty.SetValue(serverSettings, Convert.ChangeType(SettingValue, targetProperty.PropertyType));
                
            newValue = targetProperty.GetValue(serverSettings);

            await botDatabase.SaveChangesAsync();
        }
            
        EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);

        replyEmbed.Title = "\u2705 Success";
        replyEmbed.Description = $"Successfully set setting **{SettingName}** to value `{newValue}`.";
        replyEmbed.WithColor(119, 178, 85);
            
        await FollowupAsync(embed: replyEmbed.Build(), allowedMentions: BotGlobals.Instance.AllowOnlyUsers);
            
        return ExecutionResult.Succesful();
    }
}