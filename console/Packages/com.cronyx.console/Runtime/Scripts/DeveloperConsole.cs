using Cronyx.Console.Commands;
using Cronyx.Console.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

namespace Cronyx.Console
{
	[DefaultExecutionOrder(-2000)]
	public class DeveloperConsole : MonoBehaviour
	{
		#region Singleton

		private static DeveloperConsole mConsole;
		internal static DeveloperConsole Console
		{
			get
			{
				if (mConsole != null) return mConsole;

				// Check if the console has been enabled first
				if (!ConsoleSettings.EnableConsole.IsEnabled()) return null;

				if (mConsole == null)
					mConsole = FindObjectOfType<DeveloperConsole>();
				if (mConsole == null)
				{
					var go = new GameObject();
					mConsole = go.AddComponent<DeveloperConsole>();
				}

				mConsole.gameObject.name = Logger.ApplicationName;
				DontDestroyOnLoad(mConsole);

				return mConsole;
			}
		}

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]	
		internal static DeveloperConsole InitializeSingleton() => Console;
		
		private bool EnsureSingleton ()
		{
			if (!ConsoleSettings.EnableConsole.IsEnabled()) return false;
			if (Console != this) return false;
			return true;
		}

		#endregion Singleton

		/// <summary>
		/// Gets whether or not the developer console is enabled and accessible. This depends on the value of <see cref="ConsoleSettings.EnableConsole"/>
		/// </summary>
		public static bool Enabled => Console != null;

		/// <summary>
		/// Gets whether or not the developer console is currently open and visible.
		/// </summary>
		public static bool IsOpen => Console?.mOpen ?? false;

		/// <summary>
		/// An event that is invoked when the console is opened.
		/// </summary>
		public static event Action OnConsoleOpened
		{
			add { if (Console != null) Console.mOnConsoleOpened += value; }
			remove { if (Console != null) Console.mOnConsoleOpened -= value; }
		}
		private event Action mOnConsoleOpened;

		/// <summary>
		/// An event that is invoked when the console is closed.
		/// </summary>
		public static event Action OnConsoleClosed
		{
			add { if (Console != null) Console.mOnConsoleClosed += value; }
			remove { if (Console != null) Console.mOnConsoleClosed -= value; }
		}
		private event Action mOnConsoleClosed;

		/// <summary>
		/// An event that is invoked when the console's working directory changes.
		/// </summary>
		public static event Action<string> OnDirectoryChanged
		{
			add { if (Console != null) Console.mOnDirectoryChanged += value; }
			remove { if (Console != null) Console.mOnDirectoryChanged -= value; }
		}
		private event Action<string> mOnDirectoryChanged;

		/// <summary>
		/// Opens the console. Does nothing if the console is already open.
		/// </summary>
		public static void Open() => Console?.OpenConsole();

		/// <summary>
		/// Closes the console. Does nothing if the console is already closed.
		/// </summary>
		public static void Close() => Console?.CloseConsole();

		/// <summary>
		/// Changes the console's working directory to the given one.
		/// </summary>
		/// <param name="directoryPath">The console's new working directory. Must exist.</param>
		/// <exception cref="DirectoryNotFoundException">Thrown if the given directory does not exist.</exception>
		public static void ChangeDirectory(string directoryPath) => Console?.UpdateDirectory(directoryPath);

		/// <summary>
		/// The console's current working directory.
		/// </summary>
		public static string WorkingDirectory => Console?.mCurrentWorkingDirectory;

		/// <summary>
		/// The console's home directory, represented by <c>~</c>.
		/// </summary>
		public static string HomeDirectory => Console?.mHomeDirectory;

		/// <summary>
		/// Logs a message to the console.
		/// </summary>
		/// <param name="message">A message, which will be converted to a string.</param>
		public static void Log(object message) => Console?.Write(message, ConsoleSettings.ConsoleFontColor);

		/// <summary>
		/// Logs a warning to the console.
		/// </summary>
		/// <param name="message">A warning, which will be converted to a string.</param>
		public static void LogWarning(object message) => Console?.Write(message, ConsoleSettings.ConsoleFontWarningColor);

		/// <summary>
		/// Logs an error to the console.
		/// </summary>
		/// <param name="message">An error, which will be converted to a string.</param>
		public static void LogError(object message) => Console?.Write(message, ConsoleSettings.ConsoleFontErrorColor);

		public static void LogFormat(string format, params object[] args) => Log(string.Format(format, args));
		public static void LogWarningFormat(string format, params object[] args) => LogWarning(string.Format(format, args));
		public static void LogErrorFormat(string format, params object[] args) => LogError(string.Format(format, args));

		/// <summary>
		/// <para>Logs an arbitrary <see cref="ConsoleEntry"/> to the console. Use this to add custom widgets and media in the console.</para>
		/// <para>To write text to the console, use <see cref="Log(object)"/>, <see cref="LogWarning(object)"/>, and <see cref="LogError(object)"/>, instead.</para>
		/// </summary>
		/// <remarks>
		/// <para>This method can be used to add user-defined media to the console by supplying a custom prefab, <paramref name="entryPrefab"/>.</para>
		/// <para><paramref name="entryPrefab"/> must be a non-null prefab whose root GameObject has a component of type <typeparamref name="T"/>. This component may contain arbitrary user logic to customize the appearance and behaviour of the created entry.</para>
		/// </remarks>
		/// <typeparam name="T">The <see cref="ConsoleEntry"/> component, which must be attached to the root object in <paramref name="entryPrefab"/>.</typeparam>
		/// <param name="entryPrefab">A prefab to be instantiated, whose root GameObject has an attached <see cref="ConsoleEntry"/> component of type <typeparamref name="T"/>.</param>
		/// <returns>An active <see cref="ConsoleEntry"/> instance of type <typeparamref name="T"/></returns>
		public static T AppendEntry<T>(GameObject entryPrefab) where T : ConsoleEntry => Console?.mUI.CreateEntry<T>(entryPrefab);

		/// <summary>
		/// Splits the given input string into a series of command-line arguments.
		/// </summary>
		/// <param name="input">An input string to be split. Leading and trailing whitespace will be trimmed.</param>
		/// <param name="groupingChars">A set of pairs of characters that delineate a single argument. If null, it is equivalent to an empty array.</param>
		/// <returns>An array of command-line arguments.</returns>
		/// <remarks>
		///		<para>This command splits the inputted string into an array of command-line arguments, in way that might be typical of a Bash shell.</para>
		///		<para>Use <paramref name="groupingChars"/> to specify optional grouping characters to delineate a single argument.</para>
		///		<para>This function will always use the outermost grouping characters to denote a single argument. For instance, <c>SplitArgs("'Foo [bar]'", new[] {('\'', '\''), ('[', ']'}</c> will return an length-one array of strings containing "Foo [bar]".</para>
		///		<para>The outermost grouping characters will not appear in any of the resulting arguments (they are "consumed").</para>
		///		<para>Arguments are seperated by whitespace. This means that even if an input seems to be seperated by grouping characters, it can still be considered one single argument.
		///		For instance, <c>SplitArgs("Foo'Bar'", new[] {('\'', '\'')})</c> will return a length-one array containing "FooBar".
		///		Even though this input contains a pair of grouping characters, it returns only one argument because the portion inside the grouping characters is not seperated from the portion outside by whitespace.</para>
		///		<para>Grouping characters may be escaped by prefixing them with a backslash.</para>
		/// </remarks>
		/// <example>
		///		<code>SplitArgs("Foo Bar Baz", null); -> string[] {"Foo", "Bar", "Baz"}</code>
		///		<code>SplitArgs("'Foo Bar Baz'", new[] {('\'', '\'')}); -> string[] {"Foo Bar Baz"}</code>
		///		<code>SplitArgs("'Foo Bar' Baz", new[] {('\'', '\'')}); -> string[] {"Foo Bar", "Baz"}</code>
		///		<code>SplitArgs("'Foo Bar Baz", new[] {('\'', '\'')}); -> string[] {"Foo Bar Baz"}</code>
		///		<code>SplitArgs("'Foo [Bar Baz]'", new[] {('\'', '\''), ('[', ']')}); -> string[] {"Foo [Bar Baz]"}</code>
		///		<code>SplitArgs("Foo'Bar'", new[] {('\'', '\'')}); -> string[] {"FooBar"}</code>
		/// </example>
		public static string[] SplitArgs(string input, (char Beginning, char Ending)[] groupingChars) => StringUtils.SplitArgs(input, groupingChars);

		/// <summary>
		/// Splits the given input string into a series of command-line arguments.
		/// </summary>
		/// <param name="input">An input string to be split. Leading and trailing whitespace will be trimmed.</param>
		/// <remarks>See <see cref="SplitArgs(string)"/> for additional remarks.</remarks>
		/// <returns>A series of command line arguments, using single and double quotations as grouping symbols.</returns>
		public static string[] SplitArgs(string input) => SplitArgs(input, new [] { ('\"', '\"'), ('\'', '\'') });

		/// <summary>
		/// Registers a non-persistent console command.
		/// </summary>
		/// <param name="name">
		/// <para>The name of the command.</para>
		/// <para>Command names are case-insensitive, e.g., "help" and "HELP" are equivalent command names.</para>
		/// <para>Leading and trailing whitespace will be trimmed, e.g., " help " and "help" are equivalent.</para>
		/// </param>
		/// <param name="description">A short, optional description of the command that appears in a list of all commands.</param>
		/// <param name="parseCommand">
		/// <para>A callback to parse this command supplying all string data that appeared after the command when it was entered to the console.</para>
		/// <para>To split this string into an array of string arguments (like one would expect in a bash or Windows console), use <see cref="SplitArgs(string)"/>.</para>
		/// </param>
		/// <param name="help">An optional help string that may display usage information, subcommands, etc. when requested by the user.</param>
		/// <exception cref="ArgumentException">Thrown if <paramref name="name"/> is null or empty, or if <paramref name="parseCommand"/> is null.</exception>
		/// <exception cref="InvalidOperationException">Thrown if <paramref name="name"/> is taken by another command.</exception>
		public static void RegisterCommand (string name, Action<string> parseCommand, string description=null, string help=null)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("Command name cannot be null or whitespace.");

			if (parseCommand == null)
				throw new ArgumentException("Command parsing callback cannot be null.");

			Console?.Register(name, new CommandData(false, description, new NonPersistentCommand(parseCommand, help)));
		}

		/// <summary>
		/// Unregisters a command.
		/// </summary>
		/// <param name="name">The name of the command to unregister (case-insensitive, leading and trailing whitespace ignored).</param>
		/// <exception cref="InvalidOperationException">Thrown when <paramref name="name"/> is not a registered command, or when attempting to unregister a built-in command, such as "cd," "pwd," or "list".</exception>
		public static void UnregisterCommand(string name) => mConsole?.Unregister(name);

		private bool mOpen;
		private float mCachedTimeScale;
		private string mCurrentWorkingDirectory;
		private string mHomeDirectory;
		private GameObject mComponentCommandsRoot;

		internal Dictionary<string, CommandData> mCommands { get; private set; } = new Dictionary<string, CommandData>();

		private ConsoleView mUI;

		private void Awake()
		{
			if (!EnsureSingleton()) Destroy(gameObject);
			mUI = ConsoleView.CreateUI(transform);

			// Register all persistent commands
			mComponentCommandsRoot = new GameObject("ComponentCommands");
			mComponentCommandsRoot.transform.SetParent(transform, false);
			RegisterPersistentCommands();

			// Set home directory and navigate to it
			mHomeDirectory = Application.persistentDataPath;
			UpdateDirectory(mHomeDirectory);
		}

		private void Update()
		{
			if (Input.GetKeyDown(ConsoleSettings.ConsoleOpenKey))
			{
				if (mOpen) CloseConsole();
				else OpenConsole();
			}
		}

		private void Register(string name, CommandData command)
		{
			name = name.Trim().ToLower();
			if (mCommands.ContainsKey(name))
				throw new InvalidOperationException($"A command with name \"{name}\" already exists.");

			mCommands[name] = command;
		}

		private void Unregister (string name)
		{
			name = name.Trim().ToLower();
			if (!mCommands.ContainsKey(name))
				throw new InvalidOperationException($"No command found with name \"{name}\" to unregister.");

			var comData = mCommands[name];
			if (comData.Essential)
				throw new InvalidOperationException($"Could not unregister command \"{name}\": it is essential.");

			mCommands.Remove(name);
		}

		// Registers and instantiates all persistent commands, i.e., concrete classes that inherit from IConsoleCommand
		// and are marked with the appropriate attribute
		private void RegisterPersistentCommands ()
		{
			// Get a list of all valid persistent commands
			var commandTypes = GetPersistentCommandTypes();

			// Instantiate/register each command
			foreach (var commandType in commandTypes)
			{
				var commandAttribute = commandType.GetCustomAttribute<PersistentCommandAttribute>();

				// Check that this command name has not been taken
				if (mCommands.ContainsKey(commandAttribute.Name))
				{
					Logger.Warn($"Type '{commandType.Name}' with attached {nameof(PersistentCommandAttribute)} has a command name, '{commandAttribute.Name},' that has already been taken and will not be registered as a command. " +
						$"Did you mean to use a different name?");
					continue;
				}

				// Instantiate the command
				IConsoleCommand command;

				// If the command is a Unity component, attach it to this object
				if (typeof(Component).IsAssignableFrom(commandType))
					command = mComponentCommandsRoot.AddComponent(commandType) as IConsoleCommand; 
				// Otherwise use activator
				else command = Activator.CreateInstance(commandType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, new object[0], null) as IConsoleCommand;

				// Register the command
				Register(commandAttribute.Name, new CommandData(commandType.GetCustomAttribute<EssentialCommandAttribute>() != null, commandAttribute.Description, command));
			}
		}

		// Return a list of persistent command types, with essential commands sorted first
		private static List<Type> GetPersistentCommandTypes ()
		{
			// First find all classes that are marked with the relevant attribute
			// Order by descending
			var flaggedTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
								from type in assembly.GetTypes()
								let attr = type.GetCustomAttribute<PersistentCommandAttribute>()
								where attr != null
								orderby type.GetCustomAttribute<EssentialCommandAttribute>() != null descending
								select type);

			HashSet<string> commandNames = new HashSet<string>();

			// Go through the enumeration and find types that are validly constructed, and log warnings for those that aren't
			List<Type> validTypes = new List<Type>();
			foreach (var type in flaggedTypes)
			{
				// Check that the type inherits from the command class
				if (!type.GetInterfaces().Contains(typeof(IConsoleCommand)))
				{
					Logger.Warn($"Type '{type.Name}' with attached {nameof(PersistentCommandAttribute)} does not inherit from {typeof(IConsoleCommand).FullName} and will not be registered as a command. Did you mean to inherit from {typeof(IConsoleCommand).FullName}?");
					continue;
				}

				// Check that the type is concrete
				if (type.IsAbstract)
				{
					Logger.Warn($"Type '{type.Name}' is marked with a {nameof(PersistentCommandAttribute)}, but is not instantiable and will not be registered as a command. Did you mean for this type to be a non-abstract class?");
					continue;
				}

				// Check that the type is not an open generic type
				if (type.ContainsGenericParameters)
				{
					Logger.Warn($"Type '{type.Name}' is marked with a {nameof(PersistentCommandAttribute)}, but it contains unassigned generic type arguments and will not be registered as a command.");
					continue;
				}

				// Check that the type has a parameterless constructor
				if (type.GetConstructor(
					BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
					null, Type.EmptyTypes, null) == null)
				{
					Logger.Warn($"Type '{type.Name}' is marked with a {nameof(PersistentCommandAttribute)}, but it lacks a parameterless constructor and will not be registered as a command.");
					continue;
				}

				validTypes.Add(type);
			}

			return validTypes;
		}

#if UNITY_EDITOR
		[DidReloadScripts]
		private static void VerifyTypesOnRecompile() => GetPersistentCommandTypes();
#endif

		// Called by ConsoleView when the user has submitted a line to the input field
		internal void OnInputReceived (string input)
		{
			// Handle command recognition logic here
			// First seperate input into command portion and argument portion, and trim strings
			var splitArgs = SplitArgs(input);

			var command = splitArgs[0].Trim().ToLower();
			var args = input.Substring(StringUtils.Splits[0]).Trim(); // Get the position of the end of the first argument in the original input string

			// Attempt to match entered command with a command instance
			foreach (var cmdPair in mCommands)
			{
				if (command.Equals(cmdPair.Key))
				{
					cmdPair.Value.Command.Invoke(args);
					return;
				}
			}

			// No command was found, log this
			LogWarning($"{command}: command not found");
		}

		private void UpdateDirectory (string newDirectory)
		{
			if (!Directory.Exists(newDirectory))
				throw new DirectoryNotFoundException($"No such directory exists: {newDirectory}");

			mCurrentWorkingDirectory = newDirectory;
			mOnDirectoryChanged?.Invoke(mCurrentWorkingDirectory);
		}

		private void OpenConsole ()
		{
			if (mOpen) return;
			mOpen = true;

			if (ConsoleSettings.PauseOnOpen.IsEnabled())
			{
				mCachedTimeScale = Time.timeScale;
				Time.timeScale = 0;
			}

			mOnConsoleOpened?.Invoke();
		}

		private void CloseConsole ()
		{
			if (!mOpen) return;
			mOpen = false;

			if (ConsoleSettings.PauseOnOpen.IsEnabled())
				Time.timeScale = mCachedTimeScale;

			mOnConsoleClosed?.Invoke();
		}

		private void Write(object message, Color color)
		{
			var text = mUI.CreateTextEntry();
			text.Text = message.ToString();
			text.TextColor = color;
		}
	}
}
