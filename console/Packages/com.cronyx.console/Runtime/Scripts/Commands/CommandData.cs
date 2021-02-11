using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	/// <summary>
	/// A wrapper around <see cref="IConsoleCommand"/> that bundles relevant metadata about a command with its implementation.
	/// </summary>
	public class CommandData
	{
		/// <summary>
		/// Gets the unique name of this command.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets a boolean indicating whether or not this command is essential.
		/// </summary>
		/// <remarks>
		/// Essential commands are registered before all other commands and cannot be unregisted using <see cref="DeveloperConsole.Unregister(string)"/>
		/// </remarks>
		public bool Essential { get; }

		/// <summary>
		/// Gets a string containing a short description of this command. Can be null.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Gets the <see cref="IConsoleCommand"/> object containing the implementation of this command.
		/// </summary>
		public IConsoleCommand Command { get; }

		internal CommandData (string name, bool essential, string description, IConsoleCommand command)
		{
			Name = name;
			Description = description;
			Essential = essential;
			Command = command;
		}
	}
}
