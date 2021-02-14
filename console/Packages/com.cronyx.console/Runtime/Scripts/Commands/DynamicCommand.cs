using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	/// <summary>
	/// Represents a command created at runtime dynamically, rather than one that is created from a method or class marked with <see cref="CommandAttribute"/>.
	/// </summary>
	internal class DynamicCommand : IConsoleCommand
	{
		public string Help { get; }
		public void Invoke(string data) => mParseCommand.Invoke(data);

		private Action<string> mParseCommand;

		public DynamicCommand(Action<string> parseCommand, string help = null)
		{
			mParseCommand = parseCommand;
			Help = help;
		}
	}
}
