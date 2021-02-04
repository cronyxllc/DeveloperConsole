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
						var helpString = $"<b>{pair.Key}:</b> {pair.Value.Description}\n\n{pair.Value.Command.Help}";
						DeveloperConsole.Log(helpString);
						return;
					}
				}

				// No command found
				LogWarning($"'{cmdName}' is not a command");
			}
		}
	}
}
