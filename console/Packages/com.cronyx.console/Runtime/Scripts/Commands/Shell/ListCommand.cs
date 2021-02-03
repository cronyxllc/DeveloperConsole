using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands.Shell
{
	[PersistentCommand(kCommandName, Description = "Lists available commands")]
	[EssentialCommand]
	internal class ListCommand : IConsoleCommand
	{
		private const string kCommandName = "list";

		public string Help => $"usage: {kCommandName}";

		private void LogWarning(object message) => DeveloperConsole.LogWarning($"{kCommandName}: {message}");

		public void Invoke(string data)
		{
			var args = DeveloperConsole.SplitArgs(data);
			if (args.Length != 0) LogWarning(Help);
			else
			{
				// Get all commands
				var commands = DeveloperConsole.Console.mCommands;
				foreach (var command in commands)
				{
					DeveloperConsole.Log($"{command.Key, -20} {command.Value.Description ?? string.Empty, -100}");
				}
			}
		}
	}
}
