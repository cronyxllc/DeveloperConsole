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
		private string mName;
		private MethodInfo mMethod;
		private Parser mParser;

		public string Help => mParser.CalculateHelp(mName);

		public void Invoke(string data)
		{
			if (!mParser.TryParse(data, out var arguments))
			{
				// Failed to parse input. Show usage
				DeveloperConsole.LogWarning($"{mParser.CalculateUsage(mName)}\n" +
					$"Try '{typeof(HelpCommand).GetCustomAttribute<CommandAttribute>().Name} {mName}' for more information.");
				return;
			}

			mMethod.Invoke(null, arguments);
		}

		public MethodCommand(string name, MethodInfo method)
		{
			mName = name;
			mMethod = method;
			mParser = Parser.FromMethodInfo(method);
		}
	}
}
