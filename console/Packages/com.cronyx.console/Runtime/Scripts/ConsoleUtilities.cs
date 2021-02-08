using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console
{
	internal static class ConsoleUtilities
	{
		// Stores a list of indices of split locations made by SplitArgs
		public static List<int> Splits { get; } = new List<int>();

		public static string[] SplitArgs(string input, (char Beginning, char Ending)[] groupingChars)
		{
			Splits.Clear();

			if (groupingChars == null) groupingChars = new (char, char)[0];

			// First we must escape special characters from the string, and note
			// the positions of any escaped characters
			//
			// At the moment, this only applies to grouping characters, but could certainly
			// apply to other escapes in the future, such as \n or \t
			StringBuilder sb = new StringBuilder(input.Length);
			HashSet<int> escapeChars = new HashSet<int>();
			for (int i = 0; i < input.Length; i++)
			{
				if (i < input.Length - 1 && input[i] == '\\')
				{
					bool escapeFound = false;
					foreach (var pair in groupingChars)
					{
						if (input[i + 1] == pair.Beginning || input[i + 1] == pair.Ending)
						{
							var escaped = input[i + 1] == pair.Beginning ? pair.Beginning : pair.Ending;
							escapeChars.Add(sb.Length);
							sb.Append(escaped);
							i++;
							escapeFound = true;
							break;
						}
					}

					if (!escapeFound) sb.Append(input[i]);
				}
				else sb.Append(input[i]);
			}

			input = sb.ToString();
			sb.Clear();

			int currentGrouping = -1;
			List<string> arguments = new List<string>();

			void AddArgument (int index)
			{
				arguments.Add(sb.ToString());

				// Count no. escape characters up to this point
				int e = 0;
				foreach (var escape in escapeChars)
					if (index >= escape) e++;

				Splits.Add(index + e);

				sb.Clear();
			}

			for (int i = 0; i < input.Length; i++)
			{
				if (currentGrouping < 0)
				{
					// We are not in a grouping at the moment.

					// Search for the beginning of a grouping character
					bool groupingFound = false;
					for (int j = 0; j < groupingChars.Length; j++)
					{
						if (input[i] == groupingChars[j].Beginning && !escapeChars.Contains(i))
						{
							currentGrouping = j;
							groupingFound = true;
							break;
						}
					}
					if (groupingFound) continue; // We are in a grouping, no more needs to be done here

					// We have not found a grouping
					// We need to check for arguments thare are not inside of a grouping

					// Skip whitespace
					if (char.IsWhiteSpace(input[i]))
					{
						if (sb.Length > 0)
							AddArgument(i);
					}
					else sb.Append(input[i]);

				}
				else
				{
					// We are in a grouping at the moment
					// Pay attention the end character

					if (input[i] == groupingChars[currentGrouping].Ending && !escapeChars.Contains(i))
					{
						currentGrouping = -1;
						if (i == input.Length - 1) AddArgument(i);
					}
					else sb.Append(input[i]); // Not a special character, append the current character.
				}
			}

			// Add last argument if there was one
			if (sb.Length > 0)
				AddArgument(input.Length);

			return arguments.ToArray();
		}

		/// <summary>
		/// If <paramref name="to"/> is a subdirectory or subfile of <paramref name="from"/>, then outputs the path representing
		/// <paramref name="to"/> relative to <paramref name="from"/>. If it is not relative, then outputs the full path
		/// of <paramref name="to"/>.
		/// </summary>
		/// <returns>True if <paramref name="to"/> was a subdirectory or subfile of <paramref name="from"/></returns>
		public static bool TryGetRelative (string from, string to, out string formatted)
		{
			bool DirectoriesEqual (DirectoryInfo d1, DirectoryInfo d2)
			{
				return string.Equals(Path.GetFullPath(d1.FullName).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
					Path.GetFullPath(d2.FullName).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
					StringComparison.InvariantCultureIgnoreCase);
			}

			bool IsSubdirectory(string parent, string child)
			{
				DirectoryInfo dir = new DirectoryInfo(parent);
				DirectoryInfo sub = File.Exists(child) ? new FileInfo(child).Directory : new DirectoryInfo(child);

				if (DirectoriesEqual(sub, dir)) return true;

				while (sub.Parent != null)
				{
					if (DirectoriesEqual(sub.Parent, dir))
						return true;
					sub = sub.Parent;
				}

				return false;
			}

			string FormatItemName (string item)
			{
				item = Path.GetFullPath(item);
				if (File.Exists(item)) item = item.TrimEnd(Path.DirectorySeparatorChar);
				else if (!item.EndsWith(Path.DirectorySeparatorChar.ToString())) item += Path.DirectorySeparatorChar;
				return item;
			}

			from = FormatItemName(from);
			to = FormatItemName(to);
			bool isRelative = IsSubdirectory(from, to);

			if (isRelative)
			{
				// Get formatted relative string
				Uri file = new Uri(to);
				Uri folder = new Uri(from);
				formatted = Uri.UnescapeDataString(
					folder.MakeRelativeUri(file)
						.ToString()
						.Replace('/', Path.DirectorySeparatorChar)
					);

			} else formatted = to;

			return isRelative;
		}

		internal static string GetFormattedName(this MethodInfo method)
		{
			return $"{method.DeclaringType.Name}:{method.Name}";
		}
	}
}
