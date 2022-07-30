﻿using Discord;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SammBotNET.Modules
{
	[Name("Help")]
	public class HelpModule : ModuleBase<SocketCommandContext>
	{
		public CommandService CommandService { get; set; }

		[Command("help")]
		[Summary("Provides all commands and modules available.")]
		[FullDescription("Provides a list of all the commands and modules available.")]
		[HideInHelp]
		public async Task<RuntimeResult> HelpAsync()
		{
			string BotPrefix = Settings.Instance.LoadedConfig.BotPrefix;

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Help", $"These are all of the modules available to you." +
																		$"\n Use `{BotPrefix}help <Group Name>` to see its commands.");

			foreach (ModuleInfo Module in CommandService.Modules)
			{
				bool FoundCommand = false;

				foreach (CommandInfo Command in Module.Commands)
				{
					PreconditionResult Result = await Command.CheckPreconditionsAsync(Context);

					if (Command.Attributes.Any(x => x is HideInHelp)) continue;

					if (Result.IsSuccess) FoundCommand = true;
				}

				if (FoundCommand)
				{
					ModuleEmoji ModuleEmoji = Module.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
					string StringifiedEmoji = ModuleEmoji != null ? ModuleEmoji.Emoji + " " : string.Empty;

					ReplyEmbed.AddField($"{StringifiedEmoji}{Module.Name}\n(Group: `{Module.Group}`)",
						string.IsNullOrEmpty(Module.Summary) ? "No description." : Module.Summary, true);
				}
			}

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);
			await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);

			return ExecutionResult.Succesful();
		}

		[Command("help")]
		[Summary("Provides all commands and modules available.")]
		[FullDescription("Provides a list of all the commands and modules available.")]
		[HideInHelp]
		public async Task<RuntimeResult> HelpAsync([Remainder] string ModuleName)
		{
			string BotPrefix = Settings.Instance.LoadedConfig.BotPrefix;
			string[] SplittedName = ModuleName.Split(' ');

			MessageReference Reference = new MessageReference(Context.Message.Id, Context.Channel.Id, null, false);
			AllowedMentions AllowedMentions = new AllowedMentions(AllowedMentionTypes.Users);

			if (SplittedName.Length == 1)
			{
				ModuleInfo ModuleInfo = CommandService.Modules.SingleOrDefault(x => x.Name == ModuleName || x.Group == ModuleName);

				if (ModuleInfo == default(ModuleInfo))
					return ExecutionResult.FromError($"The module \"{ModuleName}\" doesn't exist.");

				ModuleEmoji ModuleEmoji = ModuleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
				string StringifiedEmoji = ModuleEmoji != default(ModuleEmoji) ? ModuleEmoji.Emoji + " " : string.Empty;

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context,
						"Help", $"**{StringifiedEmoji}{ModuleInfo.Name}**\n{ModuleInfo.Summary}\n**Syntax**: `{BotPrefix}{ModuleInfo.Group} <Command Name>`");

				bool FoundCommand = false;
				foreach (CommandInfo Command in ModuleInfo.Commands)
				{
					if (Command.Attributes.Any(x => x is HideInHelp)) continue;

					PreconditionResult Result = await Command.CheckPreconditionsAsync(Context);

					if (Result.IsSuccess)
					{
						ReplyEmbed.AddField(Command.Name, $"{(string.IsNullOrWhiteSpace(Command.Summary) ? "No summary." : Command.Summary)}", true);
						FoundCommand = true;
					}
				}

				if(!FoundCommand)
					return ExecutionResult.FromError($"The module \"{ModuleInfo.Name}\" has no commands, or you don't have enough permissions to see them.");

				await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}
			else
			{
				string ActualName = SplittedName.Last();

				SearchResult SearchResult = CommandService.Search(Context, ModuleName);

				if (!SearchResult.IsSuccess)
					return ExecutionResult.FromError($"There is no command named \"{ModuleName}\". Check your spelling.");

				CommandMatch Match = SearchResult.Commands.SingleOrDefault(x => x.Command.Name == ActualName);

				if (Match.Command == null)
					return ExecutionResult.FromError($"There is no command named \"{ModuleName}\". Check your spelling.");

				CommandInfo Command = Match.Command;

				List<string> ProcessedAliases = Command.Aliases.ToList();
				ProcessedAliases.RemoveAt(0); //Remove the command name itself from the aliases list.

				//Remove group name from aliases list.
				for (int i = 0; i < ProcessedAliases.Count; i++)
				{
					ProcessedAliases[i] = ProcessedAliases[i].Split(' ').Last();
				}

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Help");

				FullDescription CommandDescription = Command.Attributes.FirstOrDefault(x => x is FullDescription) as FullDescription;
				string FormattedDescription = CommandDescription != default(FullDescription) && !string.IsNullOrEmpty(CommandDescription.Description)
					? CommandDescription.Description : "No description.";

				ReplyEmbed.AddField("Command Name", Command.Name, true);
				ReplyEmbed.AddField("Command Group", Command.Module.Group, true);
				ReplyEmbed.AddField("Command Aliases", ProcessedAliases.Count == 0 ? "No aliases." : string.Join(", ", ProcessedAliases.ToArray()), false);
				ReplyEmbed.AddField("Command Description", FormattedDescription);

				string CommandParameters = "`*` = Optional **||** `^` = No quote marks needed if it contains spaces.\n";
				foreach (ParameterInfo ParameterInfo in Command.Parameters)
				{
					string TypeName = ParameterInfo.Type.Name;
					string AdditionalSymbols = string.Empty;
					string DefaultValue = string.Empty;
					
					if (ParameterInfo.IsOptional) AdditionalSymbols += "*";
					if (ParameterInfo.IsRemainder) AdditionalSymbols += "^";
					if (ParameterInfo.DefaultValue != null) DefaultValue = $" (Default: **{ParameterInfo.DefaultValue}**)";

					CommandParameters += $"[**{TypeName}**{AdditionalSymbols}] `{ParameterInfo.Name}`{DefaultValue}";

					CommandParameters += "\n";
				}

				ReplyEmbed.AddField("Command Parameters", Command.Parameters.Count == 0 ? "No parameters." : CommandParameters);

				await ReplyAsync(null, false, ReplyEmbed.Build(), allowedMentions: AllowedMentions, messageReference: Reference);
			}

			return ExecutionResult.Succesful();
		}
	}
}
