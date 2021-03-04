using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	/// <summary>
	/// Apply to a command to indicate that it is built-in and cannot be unregistered.
	/// Reserved for internal use only.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method,
		AllowMultiple = false,
		Inherited = false)]
	internal class EssentialAttribute : Attribute { }
}
