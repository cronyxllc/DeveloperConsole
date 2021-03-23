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


	}
}
