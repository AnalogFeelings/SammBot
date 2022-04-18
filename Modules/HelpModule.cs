using Discord;
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
		[HideInHelp]
		[Summary("Provides all commands and modules available.")]
		public async Task<RuntimeResult> HelpAsync()
		{
			string BotPrefix = Settings.Instance.LoadedConfig.BotPrefix;

			EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Help", $"These are all of the modules available to you." +
																		$"\n Use `{BotPrefix}help <Module/Group Name>` to see its commands.");

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

			await ReplyAsync("", false, ReplyEmbed.Build());

			return ExecutionResult.Succesful();
		}

		[Command("help")]
		[HideInHelp]
		[Summary("Provides all commands and modules available.")]
		public async Task<RuntimeResult> HelpAsync([Remainder] string ModuleName)
		{
			string BotPrefix = Settings.Instance.LoadedConfig.BotPrefix;
			string[] SplittedName = ModuleName.Split(' ');

			if (SplittedName.Length == 1)
			{
				ModuleInfo ModuleInfo = CommandService.Modules.SingleOrDefault(x => x.Name == ModuleName || x.Group == ModuleName);

				if (ModuleInfo == null || ModuleInfo == default(ModuleInfo))
					return ExecutionResult.FromError($"The module \"{ModuleName}\" doesn't exist.");

				ModuleEmoji ModuleEmoji = ModuleInfo.Attributes.FirstOrDefault(x => x is ModuleEmoji) as ModuleEmoji;
				string StringifiedEmoji = ModuleEmoji != null ? ModuleEmoji.Emoji + " " : string.Empty;

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context,
						"Help", $"**{StringifiedEmoji}{ModuleInfo.Name}**\nSyntax: `{BotPrefix}{ModuleInfo.Group} <Command Name>`");

				bool FoundCommand = false;
				string EmbedDescription = string.Empty;
				foreach (CommandInfo Command in ModuleInfo.Commands)
				{
					if (Command.Attributes.Any(x => x is HideInHelp)) continue;

					PreconditionResult Result = await Command.CheckPreconditionsAsync(Context);

					if (Result.IsSuccess)
					{
						ReplyEmbed.AddField(Command.Name, $"{(string.IsNullOrWhiteSpace(Command.Summary) ? "No description." : Command.Summary)}", true);
						FoundCommand = true;
					}
				}

				if(!FoundCommand)
					return ExecutionResult.FromError($"The module \"{ModuleInfo.Name}\" has no commands, or you don't have enough permissions to see them.");

				await ReplyAsync("", false, ReplyEmbed.Build());
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

				EmbedBuilder ReplyEmbed = new EmbedBuilder().BuildDefaultEmbed(Context, "Help");

				CommandInfo Command = Match.Command;

				List<string> ProcessedAliases = Command.Aliases.ToList();
				ProcessedAliases.RemoveAt(0); //Remove the command name itself from the aliases list.

				//Remove group name from aliases list.
				for (int i = 0; i < ProcessedAliases.Count; i++)
				{
					ProcessedAliases[i] = ProcessedAliases[i].Split(' ').Last();
				}

				ReplyEmbed.AddField("Command Name", Command.Name);
				ReplyEmbed.AddField("Command Aliases", ProcessedAliases.Count == 0 ? "No aliases." : string.Join(", ", ProcessedAliases.ToArray()));
				ReplyEmbed.AddField("Command Group", Command.Module.Group);
				ReplyEmbed.AddField("Command Summary", string.IsNullOrWhiteSpace(Command.Summary) ? "No summary." : Command.Summary);

				string CommandParameters = string.Empty;
				foreach (ParameterInfo ParameterInfo in Command.Parameters)
				{
					CommandParameters += $"[**{ParameterInfo.Type.Name}**] `{ParameterInfo.Name}`";
					if (ParameterInfo.IsOptional) CommandParameters += " (OPTIONAL)";
					CommandParameters += "\n";
				}

				ReplyEmbed.AddField("Command Parameters", string.IsNullOrEmpty(CommandParameters) ? "No parameters." : CommandParameters);

				await ReplyAsync("", false, ReplyEmbed.Build());
			}

			return ExecutionResult.Succesful();
		}
	}
}
