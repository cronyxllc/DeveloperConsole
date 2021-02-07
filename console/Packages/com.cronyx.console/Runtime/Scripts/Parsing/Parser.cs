using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	public class Parser
	{
		private static Dictionary<Type, IParameterParser> mParsers = new Dictionary<Type, IParameterParser>();
		private static Dictionary<Type, Type> mGenericParsers = new Dictionary<Type, Type>();

		public static void AddParser<T>(ParameterParser<T> parser) => AddParser(typeof(T), parser);
		private static void AddParser(Type parseType, IParameterParser parser)
		{
			if (parseType == null) throw new ArgumentNullException(nameof(parseType));
			if (parser == null) throw new ArgumentNullException(nameof(parser));
			mParsers[parseType] = parser;
		}

		public static void AddGenericParser(Type genericTypeDefinition, Type parserType)
		{
			if (genericTypeDefinition == null) throw new ArgumentNullException(nameof(genericTypeDefinition));
			if (parserType == null) throw new ArgumentNullException(nameof(parserType));

			// Use reflection to check the input types match specification
			if (!genericTypeDefinition.IsGenericTypeDefinition) throw new ArgumentException(nameof(genericTypeDefinition));
			if (!parserType.IsGenericTypeDefinition) throw new ArgumentException(nameof(parserType));
			if (parserType.GetGenericArguments().Length != genericTypeDefinition.GetGenericArguments().Length)
				throw new ArgumentException($"{nameof(genericTypeDefinition)} and {nameof(parserType)} must have the same number of generic type arguments.");

			if (parserType.GetConstructor(
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
				null, Type.EmptyTypes, null) == null)
				throw new ArgumentException($"{parserType.Name} must have a parameterless constructor to be used as a generic parser.");

			mGenericParsers[genericTypeDefinition] = parserType;

		}

		public static ParameterParser<T> GetParser <T> ()
		{
			var parser = GetParser(typeof(T)) as ParameterParser<T>;
			if (parser == null) throw new ParserNotFoundException(nameof(T));
			return parser;
		}

		// Type can be a concrete type, such as int or float, for which an explicit parser has been declared,
		// or it can be a closed generic type, such as List<int> or HashSet<float>, for which no explicit parser has been declared
		// but instead can be generated from an existing generic parser definition
		private static IParameterParser GetParser (Type parseType)
		{
			if (mParsers.ContainsKey(parseType)) return mParsers[parseType];

			void Throw () { throw new ParserNotFoundException(nameof(parseType)); }

			// No concrete parser has been instantiated for this type,
			// let's see if we can generate one using generic type definitions and reflection

			if (parseType.ContainsGenericParameters) Throw(); // Type is an unbounded generic type, we cannot do anything with this as generic arguments are unknown
			if (!parseType.IsGenericType) Throw(); // Check that type is indeed a generic type, such as List<int>, for which a parser can be generated

			var genericTypeDefinition = parseType.GetGenericTypeDefinition(); // Get the open generic type definition, such as List<> or IEnumerable<>
			var typeArguments = parseType.GenericTypeArguments; // Get the array of type arguments. For List<int>, this would return [ System.Int32 ]

			if (!mGenericParsers.ContainsKey(genericTypeDefinition)) Throw(); // No corresponding parser set for this generic type definition
			var genericParserType = mGenericParsers[genericTypeDefinition];

			// Construct the new parser type by plugging in the generic type arguments
			var constructedParserType = genericParserType.MakeGenericType(typeArguments);

			// Create an instance of this parser
			var parser = Activator.CreateInstance(constructedParserType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, new object[0], null) as IParameterParser;

			// Add this parser to the dictionary of other parsers we do not have to repeat this process for the same generic type in the future
			AddParser(constructedParserType, parser);
			return parser;
		}

		public static string GetFormattedName (Type type)
		{
			return null;
		}

		private enum ParameterType
		{
			Positional,
			Switch,
			Flag
		}

		private class Parameter
		{
			public ParameterType ParamType { get; private set; }
			public string Name { get; private set; }
			public string Description { get; private set; }
			public int Min { get; private set; } = -1;
			public int Max { get; private set; } = -1;
			public Type OptionalParserType { get; private set; }
			public string MetaVariable { get; private set; }
			public object DefaultValue { get; private set; }
			public bool Required { get; private set; }
			public char ShortName { get; private set; }
			public Type FieldType { get; private set; }

			internal IParameterParser Parser => GetParser(FieldType);
			public string ParserFormat => Parser.GetFormat();

			public static Parameter FromParameterInfo (ParameterInfo info)
			{
				var parameter = new Parameter();
				parameter.FieldType = info.ParameterType;

				// Fill in default values
				parameter.ParamType = ParameterType.Positional;
				parameter.MetaVariable = info.Name;

				// Check to see if an attribute was attached, and attempt to add relevant information
				var parameterAttribute = info.GetCustomAttribute<ParameterAttribute>();
				if (parameterAttribute == null) return parameter; // No attribute attached, nothing more we can infer about this parameter

				if (parameterAttribute.Name != null)
				{
					parameter.Name = parameterAttribute.Name;
					parameter.MetaVariable = parameterAttribute.Name;
				}

				parameter.Min = parameterAttribute.Min;
				parameter.Max = parameterAttribute.Max;
				parameter.Description = parameterAttribute.Description;
				parameter.OptionalParserType = parameterAttribute.Parser;
				if (parameterAttribute.Meta != null) parameter.MetaVariable = parameterAttribute.Meta;

				if (parameterAttribute is SwitchAttribute switchAttribute)
				{
					// Handle logic for switch attributes
					parameter.ShortName = switchAttribute.ShortName;

					// Check if this is a switch or a flag (a special kind of switch)
					// Flags can only be applied to boolean arguments
					if (info.ParameterType == typeof(bool) && switchAttribute.Flag)
						parameter.ParamType = ParameterType.Flag;
					else
					{
						// This is a switch, not a flag
						parameter.ParamType = ParameterType.Switch;
						parameter.Required = switchAttribute.Required;
					}
				} else if (parameterAttribute is PositionalAttribute positionalAttribute)
				{
					// Handle logic for positional attributes
					parameter.ParamType = ParameterType.Positional;
					parameter.Required = !positionalAttribute.Optional;
				}

				// Get default value (in the case of non-flag parameters) if it exists
				if (parameter.ParamType != ParameterType.Flag && info.HasDefaultValue)
					parameter.DefaultValue = info.DefaultValue;

				return parameter;
			}
		}

		internal static Parser FromMethodInfo(MethodInfo info)
		{
			Parser parser = new Parser();

			// Verify that the method is valid:
			//	(*) Method cannot be an open generic method
			//	(*)	None of the types of the arguments may be open generic types
			//	(*) Required positional parameters must come before optional positional parameters,
			//		and optional positional parameters must come before switch/flag parameters
			//	(*) None of the arguments may be labelled ref or out

			if (info.ContainsGenericParameters)
				throw new InvalidOperationException($"Method {info.Name} in {info.DeclaringType.Name} cannot be generic.");

			var arguments = info.GetParameters();

			// Check that all arguments are not ref or out
			foreach (var arg in arguments)
				if (arg.IsOut || arg.ParameterType.IsByRef)
					throw new InvalidOperationException($"Parameter {arg.Name} in method {info.Name} in type {info.DeclaringType.Name} cannot be marked ref or out.");

			// Build parameter list, checking proper order
			foreach (var arg in arguments)
			{
				var param = Parameter.FromParameterInfo(arg);

				if (param.ParamType == ParameterType.Positional && parser.mNonPositionals.Count > 0)
				{
					// Cannot add positional argument when there are already optionals
					throw new InvalidOperationException($"Positional parameter {arg.Name} in method {info.Name} in type {info.DeclaringType.Name} may not come after any switch/flag arguments.");
				}

				if (param.ParamType == ParameterType.Positional && param.Required && parser.mPositionals.Any(p => !p.Required))
				{
					// Cannot add non-optional positional arguments when there are already optional positional arguments
					throw new InvalidOperationException($"Required positional parameter {arg.Name} in method {info.Name} in type {info.DeclaringType.Name} may not come after any optional positional arguments.");
				}

				parser.Add(param);
			}

			return parser;
		}

		private List<Parameter> mPositionals = new List<Parameter>();
		private List<Parameter> mNonPositionals = new List<Parameter>();

		private Parser() { }

		private void Add (Parameter parameter)
		{
			if (parameter.ParamType == ParameterType.Positional)
				mPositionals.Add(parameter);
			else mNonPositionals.Add(parameter);
		}

		public bool TryParse (string input, out object[] arguments)
		{
			arguments = new object[mPositionals.Count + mNonPositionals.Count];

			ArgumentInput argInput = new ArgumentInput(input);

			return false;
		}

		/// <summary>
		/// Calculates and returns a usage string of the form: <c>usage: command arg1 arg2 [arg3 [arg4]] [-a] [-b foo]</c>
		/// </summary>
		/// <param name="commandName">The name of the command to be used in the usage string.</param>
		/// <returns>A formatted usage string representing the arguments this <see cref="Parser"/> will parse</returns>
		public string CalculateUsage (string commandName)
		{
			StringBuilder sb = new StringBuilder()
				.Append("usage: ")
				.Append(commandName)
				.Append(' ');

			// Start with mandatory positional arguments
			int optionalsStartIndex = -1;
			for (int i = 0; i < mPositionals.Count; i++)
			{
				Parameter current = mPositionals[i];

				if (current.Required) sb.Append($"{current.MetaVariable} ");
				else
				{
					optionalsStartIndex = i;
					break;
				}
			}

			// Then generate usage for optional positional arguments recursively
			// (if there are any)
			if (optionalsStartIndex >= 0)
			{
				string optionals = string.Empty;
				int currentIndex = mPositionals.Count - 1;
				while (currentIndex >= optionalsStartIndex)
				{
					optionals = string.IsNullOrEmpty(optionals) ? $"[{mPositionals[currentIndex].MetaVariable}]" : $"[{mPositionals[currentIndex].MetaVariable} {optionals}]";
					currentIndex--;
				}

				sb.Append(optionals).Append(' ');
			}

			// Now do all non-positionals
			// Organize by required parameters first
			var nonPositionalsSorted = mNonPositionals.OrderByDescending(p => p.Required);
			foreach (var param in mNonPositionals)
			{
				if (param.ParamType == ParameterType.Flag)
					// Flags
					sb.Append(param.Required ? $"-{param.ShortName} " : $"[-{param.ShortName}] ");
				else
					// Switches
					sb.Append(param.Required ? $"-{param.ShortName} {param.MetaVariable} " : $"[{param.ShortName} {param.MetaVariable}] ");
			}

			return sb.ToString();
		}

		/// <summary>
		/// Calculates a long help string showing usage information, parameter formatting, and descriptions of optional arguments.
		/// </summary>
		/// <param name="commandName">The name of the command to be used in the help message.</param>
		public string CalculateHelp (string commandName)
		{
			const int indent = 4; // number of spaces in a single indent
			const int padding12 = 4; // padding between console columns 1 and 2
			const int padding23 = 8; // padding between console columns 2 and 4 (in the parameter format section)

			int MaxLength(IEnumerable<string> strings) => strings.Max(s => s?.Length ?? 0);

			string ParameterNames (Parameter param)
			{
				if (param.ParamType == ParameterType.Positional) return param.MetaVariable;
				else if (string.IsNullOrEmpty(param.Name)) return $"-{param.ShortName}";
				else return $"-{param.ShortName}, --{param.Name}";
			}

			StringBuilder sb = new StringBuilder()
				.Append(CalculateUsage(commandName));

			var allParams = Enumerable.Concat(mPositionals, mNonPositionals);

			// This method will show three groups of text:
			//	(*)	Format, i.e. the types of all parameters and any format help for nested types
			//	(*)	Mandatory Parameters, i.e. a list of and description of any mandatory parameters
			//	(*)	Optional Parameters, i.e. a list of and description of any optional parameters
			//
			// Each group will be stored in a list of lists, representing rows and columns
			// Each inner list represents a single row

			List<List<string>> formatGroup = new List<List<string>>();
			List<List<string>> mandatoryGroup = new List<List<string>>();
			List<List<string>> optionalGroup = new List<List<string>>();

			// Format group:
			//	Each row will represent a set of parameters tied to a single type, for instance:
			//	    arg			List<List<int>>			((foo ...) ...)
			//		col			Vector3					(x y z)
			//		foo			Color					(r g b a)
			//		bar, boz	string
			//		baz			float
			//
			//	The first column will contain the metavariable name, the second column will contain
			//	the typename, and the third column any complex formatting help.
			//	Flag parameters will be excluded from this section.

			IDictionary<Type, List<Parameter>> typeMap = new Dictionary<Type, List<Parameter>>();
			
			// Map parameters to types
			foreach (var param in allParams)
			{
				if (param.ParamType == ParameterType.Flag) continue; // Don't add flag parameters
				if (!typeMap.ContainsKey(param.FieldType)) typeMap[param.FieldType] = new List<Parameter>();
				typeMap[param.FieldType].Add(param);
			}

			// Stringify the resulting dictionary
			StringBuilder argNamesBuilder = new StringBuilder();
			foreach (var pair in typeMap)
			{
				argNamesBuilder.Clear();
				for (int i = 0; i < pair.Value.Count; i++)
					if (i == pair.Value.Count - 1) argNamesBuilder.Append(pair.Value[i].MetaVariable);
					else argNamesBuilder.Append($"{pair.Value[i].MetaVariable}, ");
				formatGroup.Add(new List<string> { argNamesBuilder.ToString(), GetFormattedName(pair.Key), pair.Value[0].Parser.GetFormat() });
			};

			// Order the format group so that the arguments with complex format come first
			formatGroup.Sort((a, b) => string.IsNullOrEmpty(a[2]).CompareTo(string.IsNullOrEmpty(b[2])));

			// Mandatory parameters:
			//	Each row will designate a mandatory parameter and its description.
			//	Required positional arguments will come first, followed by required switches
			//	If a parameter has no description, it will not be included in the readout
			//
			//	Example:
			//		foo				A vector representing a direction
			//		-b, --bar		A color representing the color of the drawn ray
			//
			//	The first column will represent the parameter name (*names* in the case of mandatory switches)
			//	and the second column will represent the description of that parameter

			foreach (var param in allParams)
			{
				if (!param.Required) continue; // No optional parameters here
				if (string.IsNullOrWhiteSpace(param.Description)) continue; // No description means no readout for this parameter
				mandatoryGroup.Add(new List<string>() { ParameterNames(param), param.Description });
			}

			// Optional parameters:
			//	The same as the mandatory parameters, but using only optional params only
			foreach (var param in allParams)
			{
				if (param.Required) continue; // No mandatory params here
				if (string.IsNullOrWhiteSpace(param.Description)) continue; // No description
				optionalGroup.Add(new List<string>() { ParameterNames(param), param.Description });
			}

			// Calculate column alignment based on the maximum length string appearing in each column
			var allRows = formatGroup.Concat(mandatoryGroup).Concat(optionalGroup);

			if (allRows.Count() == 0) // No rows to display at all, just return now
				return sb.ToString();

			var leftColAlignment = -(MaxLength(allRows.Select(x => x[0])) + padding12);
			
			// Display format group:
			if (formatGroup.Count > 0) {
				sb.AppendLine().AppendLine("Format:");

				// First calculate the alignment of column 2
				var middleColAlignment = -(MaxLength(formatGroup.Select(x => x[1])) + padding23);

				// Display each row
				var format = $"{{0, {leftColAlignment}}}{{1, {middleColAlignment}}}";
				foreach (var row in formatGroup)
				{
					sb.Append(' ', indent); // Indent text
					sb.AppendLine(string.Format(format, row[0], row[1], row[2]));
				}
			}

			var parameterFormat = $"{{0, {leftColAlignment}}}{{1}}";

			// Display mandatory parameters group:
			if (mandatoryGroup.Count > 0)
			{
				sb.AppendLine().AppendLine("Mandatory Parameters:");

				// Display each row
				foreach (var row in mandatoryGroup)
				{
					sb.Append(' ', indent); // Indent text
					sb.AppendLine(string.Format(parameterFormat, row[0], row[1]));
				}
			}

			// Display optional parameters group:
			if (optionalGroup.Count > 0)
			{
				sb.AppendLine().AppendLine("Optional Parameters:");
				
				// Display each row
				foreach (var row in optionalGroup)
				{
					sb.Append(' ', indent); // Indent text
					sb.AppendLine(string.Format(parameterFormat, row[0], row[1]));
				}
			}

			return sb.ToString();
		}
	}
}
