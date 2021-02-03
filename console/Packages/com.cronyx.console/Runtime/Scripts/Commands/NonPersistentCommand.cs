using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	public class NonPersistentCommand : IConsoleCommand
	{
		public string Help { get; }
		public void Invoke(string data) => mParseCommand.Invoke(data);

		private Action<string> mParseCommand;

		public NonPersistentCommand(Action<string> parseCommand, string help = null)
		{
			mParseCommand = parseCommand;
			Help = help;
		}
	}
}
