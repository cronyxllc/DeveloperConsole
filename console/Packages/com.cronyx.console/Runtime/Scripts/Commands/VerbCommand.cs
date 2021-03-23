using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	public abstract class VerbCommand : IConsoleCommand
	{
		public virtual string Help => throw new NotImplementedException();

		public void Invoke(string data)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Adds a new verb to this <see cref="VerbCommand"/>
		/// </summary>
		/// <param name="name">A unique name for this verb.</param>
		/// <param name="command">An <see cref="IConsoleCommand"/> instance that contains the implementation for this verb.</param>
		/// <param name="description">An optional, short description for this verb that will be shown alongside all other verbs when the parent command is called. Can contain multiple lines.</param>
		protected void AddVerb (string name, IConsoleCommand command, string description = null)
		{

		}
	}
}
