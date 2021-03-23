using Cronyx.Console.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cronyx.Console.Commands.Shell
{
	internal static class FileCommands {

		private static readonly char[] invalidChars = Path.GetInvalidPathChars();

		// Navigates to a file path that may be relative to the console's current
		// working directory. If it is, stores the equivalent rooted path
		//
		// Returns false if the given path is invalid
		public static bool TryNavigatePath (string path, out string newPath)
		{
			path = path.Trim();
			newPath = string.Empty;

			if (path.Any(i => invalidChars.Contains(i))) return false;

			if (Path.IsPathRooted(path))
				newPath = path;
			else newPath = Path.Combine(DeveloperConsole.WorkingDirectory, path);

			return true;
		}

		// Creates a new path given an input path.
		//
		// Appends <path> to the current working directory to create the new path if it is a relative path,
		// otherwise if <path> is absolute, uses <path> itself.
		// 
		// Returns true if the new path is a valid directory
		public static bool TryNavigateDirectory (string path, out string newPath)
		{
			if (!TryNavigatePath(path, out newPath)) return false;
			return Directory.Exists(newPath); 
		}

		// Creates a new path given an input path.
		//
		// Appends <path> to the current working directory to create the new path if it is a relative path,
		// otherwise if <path> is absolute, uses <path> itself.
		// 
		// Returns true if the new path is a valid file
		public static bool TryNavigateFile(string path, out string newPath)
		{
			if (!TryNavigatePath(path, out newPath)) return false;
			return File.Exists(newPath);
		}

		[Command(kCommandName, Description = "Changes the current directory")]
		[Essential]
		public class ChangeDirectoryCommand : IConsoleCommand
		{
			private const string kCommandName = "cd";
			private static readonly string kUsage = $"usage: {kCommandName} [dir]";

			private void LogWarning(object message) => DeveloperConsole.LogWarning($"{kCommandName}: {message}");

			public string Help { get; } = $"{kUsage}\nIf no directory is provided, navigates to the home directory.";

			public void Invoke(string data)
			{
				var args = DeveloperConsole.SplitArgs(data);
				if (args.Length == 0) DeveloperConsole.ChangeDirectory(DeveloperConsole.HomeDirectory);
				else if (args.Length == 1)
				{
					if (TryNavigateDirectory(args[0], out var newDir))
						DeveloperConsole.ChangeDirectory(newDir);
					else LogWarning("not a directory");
				}
				else LogWarning(kUsage);
			}
		}

		// TODO:	This command could certainly use some additional code
		//			to add additional switches (such as -l or -a) to make it
		//			similar to the Linux 'ls.' For now, it is kept quite simple.
		[Command(kCommandName, Description = "Lists all files and subdirectories in the current directory")]
		[Essential]
		public class ListCommand : IConsoleCommand
		{
			private const string kCommandName = "ls";
			private static readonly string kUsage = $"usage: {kCommandName} [dir]";

			private static readonly Color kDirectoryColor = new Color(96 / 255f, 96 / 255f, 255 / 255f);
			private static readonly string kDirectoryColorHtml = ColorUtility.ToHtmlStringRGB(kDirectoryColor);

			public string Help { get; } = $"{kUsage}\n" +
				$"If directory is not provided, lists all files and directories in the current directory.";

			private void LogWarning(object message) => DeveloperConsole.LogWarning($"{kCommandName}: {message}");

			public void Invoke(string data)
			{
				var args = DeveloperConsole.SplitArgs(data);
				if (args.Length > 1)
				{
					LogWarning(kUsage);
					return;
				}

				string dirPath;
				if (args.Length == 1)
				{
					if (!TryNavigateDirectory(args[0], out dirPath))
					{
						// Inputted directory does not exist
						LogWarning("not a directory");
						return;
					}
				}
				else dirPath = DeveloperConsole.WorkingDirectory;

				// Get all directories and files in the current path and alphabetize
				var entries = Directory.GetFileSystemEntries(dirPath, "*", SearchOption.TopDirectoryOnly)
					.OrderBy(e => e);
				
				// Write directories to console
				StringBuilder sb = new StringBuilder();
				foreach (var entry in entries)
				{
					ConsoleUtilities.TryGetRelative(dirPath, entry, out var formatted);
					if (Directory.Exists(entry)) formatted = $"<color=#{kDirectoryColorHtml}>{formatted}</color>";
					sb.AppendLine(formatted);
				}

				DeveloperConsole.Log(sb.ToString());
			}
		}

		[Command(kCommandName, Description = "Prints the current directory")]
		[Essential]
		public class PrintWorkingDirectoryCommand : IConsoleCommand
		{
			private const string kCommandName = "pwd";

			public string Help { get; } = $"usage: {kCommandName}";

			private void LogWarning(object message) => DeveloperConsole.LogWarning($"{kCommandName}: {message}");

			public void Invoke(string data)
			{
				var args = DeveloperConsole.SplitArgs(data);
				if (args.Length != 0) LogWarning(Help);
				else DeveloperConsole.Log(DeveloperConsole.WorkingDirectory);
			}
		}

		private static void LogWarning(string commandName, object message) => DeveloperConsole.LogWarning($"{commandName}: {message}");

		private const string kRmCmdName = "rm";
		[Command(kRmCmdName, Description = "Deletes the specified file or directory"), Essential]
		public static void RemoveDirectory (
			[Positional(Description = "A path to a file (or a directory if '-r' is specified).")] string path,
			[Switch('r', Description = "Removes directories recursively. Only works for directories.")] bool recursive
			)
		{
			try
			{
				if (recursive)
				{
					// Deleting a directory
					if (TryNavigateDirectory(path, out string dirPath))
					{
						Directory.Delete(dirPath, true);
					}
					else LogWarning(kRmCmdName, $"cannot remove '{path}': no such directory");
				} else
				{
					// Deleting a file
					if (TryNavigateFile(path, out string filePath))
					{
						File.Delete(filePath);
					} else LogWarning(kRmCmdName, $"cannot remove '{path}': no such file");
				}
			}
			catch (IOException)
			{
				LogWarning(kRmCmdName, $"cannot remove '{path}': encountered IO exception");
			}
			catch (UnauthorizedAccessException)
			{
				LogWarning(kRmCmdName, $"cannot remove '{path}': unauthorized access");
			}
			catch (ArgumentException)
			{
				LogWarning(kRmCmdName, $"invalid path name");
			}
		}

		private const string kMakeDirCmdName = "mkdir";
		[Command(kMakeDirCmdName, Description = "Creates the specified directory"), Essential]
		public static void MakeDirectory (
			[Positional(Description = "A path to the directory to be created.")] string path
			)
		{
			if (TryNavigatePath(path, out string dirPath))
			{
				try
				{
					Directory.CreateDirectory(dirPath);
				}
				catch (PathTooLongException)
				{
					LogWarning(kRmCmdName, $"cannot create directory '{path}': path too long");
				}
				catch (DirectoryNotFoundException)
				{
					LogWarning(kRmCmdName, $"cannot create directory '{path}': the specified path is invalid");
				}
				catch (NotSupportedException)
				{
					LogWarning(kRmCmdName, $"cannot create directory '{path}': path contains a colon (:) that is not part of a drive label (e.g. 'C:\\')");
				}
				catch (IOException)
				{
					LogWarning(kMakeDirCmdName, $"cannot create directory '{path}': encountered IO exception");
				}
				catch (UnauthorizedAccessException)
				{
					LogWarning(kRmCmdName, $"cannot create directory '{path}': unauthorized access");
				}
				catch (ArgumentException)
				{
					LogWarning(kRmCmdName, $"invalid path name");
				} 

			}
			else LogWarning(kMakeDirCmdName, "invalid path name");
		}
	}
}
