using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	internal class CommandData
	{
		public bool Essential { get; }
		public string Description { get; }
		public IConsoleCommand Command { get; }

		public CommandData (bool essential, string description, IConsoleCommand command)
		{
			Description = description;
			Essential = essential;
			Command = command;
		}
	}
}
