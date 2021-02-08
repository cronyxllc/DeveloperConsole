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
	/// Encapsulates an <see cref="IConsoleCommand"/> that is created from a method marked with a <see cref="PersistentCommandAttribute"/>.
	/// </summary>
	internal class MethodCommand : IConsoleCommand
	{
		private PersistentCommandAttribute mAttribute;
		private MethodInfo mMethod;
		private Parser mParser;

		public string Help => mParser.CalculateHelp(mAttribute.Name);

		public void Invoke(string data)
		{
			if (!mParser.TryParse(data, out var arguments))
			{
				// Failed to parse input. Show usage
				DeveloperConsole.LogWarning($"{mAttribute.Name}: {mParser.CalculateUsage(mAttribute.Name)}\n" +
					$"Try '{typeof(HelpCommand).GetCustomAttribute<PersistentCommandAttribute>().Name} {mAttribute.Name}' for more information.");
				return;
			}

			mMethod.Invoke(null, arguments);
		}

		public MethodCommand(PersistentCommandAttribute attribute, MethodInfo method)
		{
			mAttribute = attribute;
			mMethod = method;
			mParser = Parser.FromMethodInfo(method);
		}
	}
}
