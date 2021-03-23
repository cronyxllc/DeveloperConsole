using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cronyx.Console.Commands.Shell;
using Cronyx.Console.Parsing;

namespace Cronyx.Console.Commands
{
	/// <summary>
	/// An <see cref="IConsoleCommand"/> that supports subcommands, for instance, <c>git push</c> or <c>git pull</c>. 
	/// </summary>
	public abstract class VerbCommand : IConsoleCommand
	{
		private class VerbMethodCommand : MethodCommand
		{
			protected VerbCommand Parent { get; }

			public override string Help => MethodParser.CalculateHelp(FullName);
			protected string FullName => $"{Parent.Name} {Name}";

			public override void Invoke(string data)
			{
				if (!MethodParser.TryParse(data, out var arguments))
				{
					// Failed to parse input. Show usage
					DeveloperConsole.Log(Help);
					return;
				}

				Method.Invoke(Target, arguments);
			}

			public VerbMethodCommand(string name, MethodInfo method, VerbCommand parent, object target)
				: base(name, method, target)
			{
				Parent = parent;
				MethodParser.QuoteCommandNames = false;
			}
		}

		/// <summary>
		/// A wrapper class around <see cref="IConsoleCommand"/> that contains information about a verb.
		/// </summary>
		public class Verb
		{
			/// <summary>
			/// The unique name of this verb.
			/// </summary>
			public string Name { get; }

			/// <summary>
			/// The <see cref="IConsoleCommand"/> implementation of this verb.
			/// </summary>
			public IConsoleCommand Command { get; }

			/// <summary>
			/// A description of this verb, shown when the parent command is executed.
			/// </summary>
			public string Description { get; }

			internal Verb(string name, IConsoleCommand command, string description)
			{
				Name = name;
				Command = command;
				Description = description;
			}
		}

		public virtual string Help => throw new NotImplementedException();

		/// <summary>
		/// Gets the name of this command for help and usage text.
		/// </summary>
		protected virtual string Name => GetType().GetCustomAttribute<CommandAttribute>().Name;

		private Dictionary<string, Verb> mVerbs = new Dictionary<string, Verb>();

		/// <summary>
		/// An enumeration of the verbs currently registered with this <see cref="VerbCommand"/>
		/// </summary>
		public IEnumerable<Verb> Verbs => mVerbs.Values;

		public void Invoke(string data)
		{
			var args = DeveloperConsole.SplitArgs(data);
			if (args.Length == 0) Invoke();
			else
			{
				// Find a verb whose name is the first argument
				var verb = args[0].Trim().ToLower();

				foreach (var pair in mVerbs)
					if (pair.Key.Equals(verb))
					{
						pair.Value.Command.Invoke(data.Substring(DeveloperConsole.SplitPositions[0]));
						return;
					}

				// No verb with that name
				DeveloperConsole.LogWarning($"'{Name}': no such verb with name '{args[0]}' could be found.");
				if (mVerbs.Count > 0)
				{
					StringBuilder sb = new StringBuilder();
					sb.AppendLine("Possible subcommands are: ");
					var verbsAlphabetized = (from pair in mVerbs
											 let v = pair.Value
											 orderby v.Name
											 select v);

					foreach (var v in verbsAlphabetized)
						sb.AppendLine($"    * {Name} {v.Name}");

					DeveloperConsole.LogWarning(sb.ToString());
				}
			}
		}

		/// <summary>
		/// A command that is called when this verb command is run without specifying a verb. By default, shows help text.
		/// </summary>
		protected virtual void Invoke ()
		{

		}

		/// <summary>
		/// Adds a new verb to this <see cref="VerbCommand"/>
		/// </summary>
		/// <param name="name">A unique name for this verb.</param>
		/// <param name="command">An <see cref="IConsoleCommand"/> instance that contains the implementation for this verb.</param>
		/// <param name="description">An optional, short description for this verb that will be shown alongside all other verbs when the parent command is called. Can contain multiple lines.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty, or if <paramref name="command"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="name"/> is already taken by another verb.</exception>
		protected void RegisterVerb (string name, IConsoleCommand command, string description = null)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Verb name cannot be null or whitespace");
			if (command == null) throw new ArgumentException("Command cannot be null");

			string formattedName = name.Trim().ToLower();
			if (mVerbs.ContainsKey(formattedName)) throw new InvalidOperationException($"Another verb with name '{name}' already exists.");

			mVerbs[formattedName] = new Verb(name, command, description);
		}

		/// <summary>
		/// Adds a new verb to this <see cref="VerbCommand"/> that parses parameters automatically.
		/// </summary>
		/// <param name="name">A unique name for this verb.</param>
		/// <param name="command">A delegate for which parameters will be automatically parsed from the command line, and can be annotated using attributes like <see cref="PositionalAttribute"/> or <see cref="SwitchAttribute"/></param>
		/// <param name="description">An optional, short description for this verb that will be shown alongside all other verbs when the parent command is called. Can contain multiple lines.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty, or if <paramref name="command"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="name"/> is already taken by another verb.</exception>
		protected void RegisterVerb(string name, Delegate command, string description = null)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Verb name cannot be null or whitespace");
			if (command == null) throw new ArgumentException("Command cannot be null");
			name = name.Trim().ToLower();
			RegisterVerb(name, new VerbMethodCommand(name, command.Method, this, command.Target), description);
		}

		/// <summary>
		/// Adds a new verb to this <see cref="VerbCommand"/> that parses the command line input manually.
		/// </summary>
		/// <param name="name">A unique name for this verb.</param>
		/// <param name="command">An action that takes a string containing the raw command line input passed to this verb and parses it manually.</param>
		/// <param name="description">An optional, short description for this verb that will be shown alongside all other verbs when the parent command is called. Can contain multiple lines.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty, or if <paramref name="command"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="name"/> is already taken by another verb.</exception>
		protected void RegisterVerbManual(string name, Action<string> command, string description = null)
		{
			RegisterVerb(name, new DynamicCommand(command), description);
		}

		/// <summary>
		/// Unregisters a verb with the given name.
		/// </summary>
		/// <param name="name">A name </param>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="name"/> is not a registered verb.</exception>
		/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or whitespace.</exception>
		protected void UnregisterVerb (string name)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Verb name cannot be null or whitespace.");

			name = name.Trim().ToLower();

			if (!mVerbs.ContainsKey(name)) throw new InvalidOperationException("No such verb to unregister");
			else mVerbs.Remove(name);
		}
	}

	[Command("test")]
	public class VerbTest : VerbCommand
	{
		public VerbTest()
		{
			RegisterVerb("foo", new Action(Foo), "hi there");
			RegisterVerb("bar", new Action<string, int>(Bar));
		}

		public void Foo ()
		{
			DeveloperConsole.Log("FOO!!!!");
		}

		public void Bar (string x, [Switch('f')] int y)
		{
			DeveloperConsole.Log($"This is bar: {x} {y}");
		}
	}
}
