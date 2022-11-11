// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using System.Threading.Tasks;
// using Discord;
// using Discord.Commands;
// using SammBot.Bot.Classes;
//
// namespace SammBot.Bot.Modules
// {
//     [Name("Server Settings")]
//     [Group("scfg")]
//     [Summary("Server settings such as logging, welcome messages, etc.")]
//     [ModuleEmoji("\u2699\uFE0F")]
//     public class GuildConfigModule : ModuleBase<SocketCommandContext>
//     {
//         [Command("list")]
//         [Alias("settings")]
//         [Summary("Lists all the available settings and their current values.")]
//         [FullDescription("Lists all the available settings and their current values.")]
//         [RateLimit(3, 3)]
//         [RequireContext(ContextType.Guild)]
//         [RequireBotPermission(GuildPermission.ManageChannels)]
//         [RequireUserPermission(GuildPermission.Administrator)]
//         public async Task<RuntimeResult> ListSettingsAsync()
//         {
//             EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Server Settings");
//
//             replyEmbed.Title = "\u2699\uFE0F Server Settings";
//             replyEmbed.Description = "These are all the server settings available.\n" +
//                                      "Their real names and current values will also be listed.";
//             replyEmbed.Color = new Color(102, 117, 127);
//             
//             List<PropertyInfo> propertyList = typeof(GuildConfig).GetProperties().Where(x => x.Name != "GuildId").ToList();
//             
//             using (BotDatabase botDatabase = new BotDatabase())
//             {
//                 GuildConfig serverSettings = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == Context.Guild.Id) 
//                                              ?? new GuildConfig()
//                                              {
//                                                  GuildId = Context.Guild.Id
//                                              };
//
//                 foreach (PropertyInfo property in propertyList)
//                 {
//                     string valueString = string.Empty;
//                     object propertyValue = property.GetValue(serverSettings, null);
//
//                     // Property is a collection.
//                     // String is an IEnumerable too, so check for that.
//                     if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType) && property.PropertyType != typeof(string))
//                     {
//                         valueString = string.Join(", ", propertyValue);
//                     }
//                     else valueString = propertyValue.ToString();
//                     
//                     FullName propertyFullName = property.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(FullName)) as FullName;
//                     // Small blue diamond emoji.
//                     string propertyName = "\U0001f539 ";
//
//                     propertyName += !string.IsNullOrEmpty(propertyFullName.Name) ? propertyFullName.Name : property.Name;
//                     propertyName += $"\n(Name: `{property.Name}`)";
//                     
//                     FullDescription propertyFullDescription = property.GetCustomAttributes(false)
//                         .FirstOrDefault(x => x.GetType() == typeof(FullDescription)) as FullDescription;
//                     string propertyDescription = !string.IsNullOrEmpty(propertyFullDescription.Description) ? propertyFullDescription.Description : "No description.";
//
//                     propertyDescription += $"\n**• Current Value**: `{valueString}`";
//
//                     replyEmbed.AddField(propertyName, propertyDescription);
//                 }
//             }
//             
//             MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
//             AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
//             await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
//
//             return ExecutionResult.Succesful();
//         }
//
//         [Command("set")]
//         [Summary("Sets the value of a setting.")]
//         [FullDescription("Sets the value of a server setting.\nYou must use the real name (\"EnableLogging\", not \"Enable Logging\").")]
//         [RateLimit(2, 3)]
//         [RequireContext(ContextType.Guild)]
//         [RequireBotPermission(GuildPermission.ManageChannels)]
//         [RequireUserPermission(GuildPermission.Administrator)]
//         public async Task<RuntimeResult> SetSettingAsync([Summary("The name of the setting you want to set.")] string SettingName,
//                                                          [Summary("The value you want to set it to.")] [Remainder] string SettingValue)
//         {
//             if (SettingName == "GuildId") return ExecutionResult.FromError("You cannot modify this setting.");
//             
//             PropertyInfo targetProperty = typeof(GuildConfig).GetProperty(SettingName);
//             object newValue = null;
//             
//             if (targetProperty == null) return ExecutionResult.FromError("This setting does not exist! Check your spelling.");
//             
//             using (BotDatabase botDatabase = new BotDatabase())
//             {
//                 GuildConfig serverSettings = botDatabase.GuildConfigs.FirstOrDefault(x => x.GuildId == Context.Guild.Id);
//                 
//                 if (serverSettings == default(GuildConfig))
//                 {
//                     serverSettings = new GuildConfig()
//                     {
//                         GuildId = Context.Guild.Id
//                     };
//
//                     await botDatabase.AddAsync(serverSettings);
//                 }
//                 
//                 // Convert.ChangeType cannot handle enums well.
//                 // Add special case.
//                 if(targetProperty.PropertyType.IsEnum)
//                     targetProperty.SetValue(serverSettings, Enum.Parse(targetProperty.PropertyType, SettingValue, true));
//                 else
//                     targetProperty.SetValue(serverSettings, Convert.ChangeType(SettingValue, targetProperty.PropertyType));
//                 
//                 newValue = targetProperty.GetValue(serverSettings);
//
//                 await botDatabase.SaveChangesAsync();
//             }
//             
//             EmbedBuilder replyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context);
//
//             replyEmbed.Title = "\u2705 Success";
//             replyEmbed.Description = $"Successfully set setting **{SettingName}** to value `{newValue}`.";
//             replyEmbed.WithColor(119, 178, 85);
//             
//             MessageReference messageReference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
//             AllowedMentions allowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
//             await ReplyAsync(null, false, replyEmbed.Build(), allowedMentions: allowedMentions, messageReference: messageReference);
//             
//             return ExecutionResult.Succesful();
//         }
//     }
// }