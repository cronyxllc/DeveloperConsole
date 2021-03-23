using Cronyx.Console.Commands.Shell;
using Cronyx.Console.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Commands
{
	/// <summary>
	/// Encapsulates an <see cref="IConsoleCommand"/> that is created from a method delegate.
	/// </summary>
	internal class MethodCommand : IConsoleCommand
	{
		protected string Name { get; }
		protected MethodInfo Method { get; }
		protected object Target { get; }
		protected Parser MethodParser { get; }

		public virtual string Help => MethodParser.CalculateHelp(Name);

		public virtual void Invoke(string data)
		{
			if (!MethodParser.TryParse(data, out var arguments))
			{
				// Failed to parse input. Show usage
				DeveloperConsole.LogWarning($"{MethodParser.CalculateUsage(Name)}\n" +
					$"Try '{typeof(HelpCommand).GetCustomAttribute<CommandAttribute>().Name} {Name}' for more information.");
				return;
			}

			Method.Invoke(Target, arguments);
		}

		public MethodCommand(string name, MethodInfo method, object target = null)
		{
			Name = name;
			Method = method;
			Target = target;
			MethodParser = Parser.FromMethodInfo(method);
		}
	}
}
