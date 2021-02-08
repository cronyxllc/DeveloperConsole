using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands.Shell
{
	[PersistentCommand(kCommandName, Description = "Displays help information about available commands")]
	[EssentialCommand]
	internal class HelpCommand : IConsoleCommand
	{
		private const string kCommandName = "help";
		private static readonly string kUsage = $"usage: {kCommandName} [command]";

		public string Help { get; } = $"{kUsage}\nIf no command is provided, lists all available commands.";

		private void LogWarning(object message) => DeveloperConsole.LogWarning($"{kCommandName}: {message}");

		public void Invoke(string data)
		{
			var args = DeveloperConsole.SplitArgs(data);
			if (args.Length > 1) LogWarning(kUsage);
			if (args.Length == 0)
			{
				// No arguments provided, list all commands
				var commands = DeveloperConsole.Console.mCommands;

				// Compile string of all commands and descriptions
				StringBuilder sb = new StringBuilder();
				foreach (var command in commands) sb.AppendLine($"{command.Key,-20} {command.Value.Description ?? string.Empty,-100}");
				DeveloperConsole.Log(sb.ToString());
			}
			else if (args.Length == 1)
			{
				var cmdName = args[0].Trim().ToLower();
				var commands = DeveloperConsole.Console.mCommands;

				foreach (var pair in commands)
				{
					if (pair.Key.Equals(cmdName))
					{
						var description = pair.Value.Description;
						var help = pair.Value.Command.Help;

						if (string.IsNullOrWhiteSpace(description))
						{
							if (string.IsNullOrWhiteSpace(help))
							{
								// No description or help text was provided with this command. Notify the user.
								DeveloperConsole.Log($"No help information provided for <b>{pair.Key}</b>");
							} else
							{
								// Help was provided, but no description
								// Just print help text
								DeveloperConsole.Log(help);
							}
						} else
						{
							if (string.IsNullOrWhiteSpace(help))
							{
								// A description was provided, but no help.
								// Just print description
								DeveloperConsole.Log($"<b>{pair.Key}:</b> {description}");
							} else
							{
								// Both a description and help text were provided.
								// Print both with a paragraph break between
								DeveloperConsole.Log($"<b>{pair.Key}:</b> {description}\n\n{help}");
							}
						}

						return;
					}
				}

				// No command found
				LogWarning($"'{cmdName}' is not a command");
			}
		}
	}
}
