using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
		AllowMultiple = false,
		Inherited = false)]
	public class CommandAttribute : Attribute
	{
		public string Name { get; private set; }

		/// <summary>
		/// A very short description of what this command does. Can be null.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Default constructor for <see cref="CommandAttribute"/>
		/// </summary>
		/// <param name="name">The name of this command. Note that console command names are case-insensitive, i.e. "help" and "HELP" refer to the same command.</param>
		public CommandAttribute (string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Console command names cannot be null or whitespace.");
			Name = name.Trim().ToLower();
		}
	}
}
