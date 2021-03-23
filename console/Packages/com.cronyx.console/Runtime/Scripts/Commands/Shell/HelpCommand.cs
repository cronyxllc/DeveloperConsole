using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace Cronyx.Console.Commands.Shell
{
	[Essential, Command(kCommandName, Description = "Displays help information about available commands")]
	internal class HelpCommand : IConsoleCommand
	{
		private const string kCommandName = "help";
		private static readonly string kUsage = $"usage: {kCommandName} [command]";

		private static readonly string kEssentialColor = ColorUtility.ToHtmlStringRGB(new Color(76f / 255, 146f / 255, 245f / 255));

		private const int kIndent = 4;

		public string Help { get; } = $"{kUsage}\nIf no command is provided, lists all available commands.";

		private void LogWarning(object message) => DeveloperConsole.LogWarning($"{kCommandName}: {message}");

		public void Invoke(string data)
		{
			var args = DeveloperConsole.SplitArgs(data);
			if (args.Length > 1) LogWarning(kUsage);
			if (args.Length == 0)
			{
				// No arguments provided, list all commands

				// Sort commands alphabetically
				var commands = from command in DeveloperConsole.Commands
							   orderby command.Name
							   select command;

				const int kRichTextMargin = 23; // Add characters for the additional <color=#xxxxxx></color> rich text tags
				var commandColumnWidth = commands.Select(command => command.Name).Max(name => name.Length) + kIndent; 

				// Compile string of all commands and descriptions
				StringBuilder sb = new StringBuilder();

				string essentialRowFormat = $"{{0,{-commandColumnWidth - kRichTextMargin}}}{{1}}";
				string rowFormat = $"{{0,{-commandColumnWidth}}}{{1}}";

				foreach (var command in commands)
				{
					string commandName = command.Name;

					// Show commands in a special color if they are essential
					if (command.Essential) commandName = $"<color=#{kEssentialColor}>{commandName}</color>";

					if (string.IsNullOrWhiteSpace(command.Description)) sb.AppendLine(command.Name); // No description, so don't print it
					else
					{
						// There is a description. Split it into lines and then print it out with the proper column indentation
						var lines = Regex.Split(command.Description, "\r\n|\r|\n");
						sb.AppendLine(string.Format(command.Essential ? essentialRowFormat : rowFormat, commandName, lines[0]));
						for (int i = 1; i < lines.Length; i++)
							sb.AppendLine(string.Format(rowFormat, string.Empty, lines[i]));
					}
				}
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
