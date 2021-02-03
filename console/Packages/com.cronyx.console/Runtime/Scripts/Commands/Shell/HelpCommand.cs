using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands.Shell
{
	[PersistentCommand(kCommandName, Description = "Displays help information about a command")]
	[EssentialCommand]
	internal class HelpCommand : IConsoleCommand
	{
		private const string kCommandName = "help";

		public string Help => $"usage: {kCommandName} <command>";

		private void LogWarning(object message) => DeveloperConsole.LogWarning($"{kCommandName}: {message}");

		public void Invoke(string data)
		{
			var args = DeveloperConsole.SplitArgs(data);
			if (args.Length != 1) LogWarning(Help);
			else
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
