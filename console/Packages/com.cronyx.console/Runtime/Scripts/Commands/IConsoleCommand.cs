using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	public interface IConsoleCommand
	{
		/// <summary>
		/// <para>A longer description of this command that appears when help information is requested.</para>
		/// <para>Could include usage information, subcommands, etc. Can be null.</para>
		/// </summary>
		string Help { get; }

		/// <summary>
		/// A method which is invoked when this command is called, supplying all string arguments passed to the command by the user.
		/// </summary>
		/// <param name="data">Any text that appeared after this command when it was entered to the console.</param>
		void Invoke(string data);
	}
}
