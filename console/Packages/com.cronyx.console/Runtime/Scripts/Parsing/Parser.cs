using Cronyx.Console.Parsing.Parsers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	public class Parser
	{
		static Parser()
		{
			AddParser(new StringParser());
			AddParser(new CharParser());

			// Integral types
			AddParser(new IntegralParser<sbyte>("sbyte", x => sbyte.Parse(x)));
			AddParser(new IntegralParser<byte>("byte", x => byte.Parse(x)));
			AddParser(new IntegralParser<short>("short", x => short.Parse(x)));
			AddParser(new IntegralParser<ushort>("ushort", x => ushort.Parse(x)));
			AddParser(new IntegralParser<int>("int", x => int.Parse(x)));
			AddParser(new IntegralParser<uint>("uint", x => uint.Parse(x)));
			AddParser(new IntegralParser<long>("long", x => long.Parse(x)));
			AddParser(new IntegralParser<ulong>("ulong", x => ulong.Parse(x)));
			
			// Floating point types
			AddParser(new FloatingParser<float>("float", x => float.Parse(x)));
			AddParser(new FloatingParser<double>("double", x => double.Parse(x)));
			AddParser(new FloatingParser<decimal>("decimal", x => decimal.Parse(x)));

			AddParser(new BoolParser());

			BindGenericParser(typeof(IEnumerable<>), typeof(IEnumerableParser<>));
			BindGenericParser(typeof(List<>), typeof(ListParser<>));
			BindGenericParser(typeof(IList<>), typeof(IListParser<>));
			BindGenericParser(typeof(IReadOnlyList<>), typeof(IReadOnlyListParser<>));
			BindGenericParser(typeof(ICollection<>), typeof(ICollectionParaser<>));
			BindGenericParser(typeof(IReadOnlyCollection<>), typeof(IReadOnlyCollectionParser<>));
			BindGenericParser(typeof(Queue<>), typeof(QueueParser<>));
			BindGenericParser(typeof(Stack<>), typeof(StackParser<>));
			BindGenericParser(typeof(HashSet<>), typeof(HashSetParser<>));
			BindGenericParser(typeof(ISet<>), typeof(ISetParser<>));

			// Dictionaries and key-value pairs
			BindGenericParser(typeof(KeyValuePair<,>), typeof(KeyValuePairParser<,>));
			BindGenericParser(typeof(Dictionary<,>), typeof(DictionaryParser<,>));
			BindGenericParser(typeof(IDictionary<,>), typeof(IDictionaryParser<,>));
			BindGenericParser(typeof(IReadOnlyDictionary<,>), typeof(IReadOnlyDictionaryParser<,>));

			// Unity Type parsers
			AddParser(new Vector2Parser());
			AddParser(new Vector2IntParser());
			AddParser(new Vector3Parser());
			AddParser(new Vector3IntParser());
			AddParser(new Vector4Parser());
			AddParser(new QuaternionParser());
			AddParser(new ColorParser());
			AddParser(new Color32Parser());
		}

		private static ISet<char> mSpecialChars = new HashSet<char>();
		public static IEnumerable<char> SpecialChars => mSpecialChars;

		public static void AddSpecialChar(char c) => mSpecialChars.Add(c);
		public static bool IsSpecial(char c) => mSpecialChars.Contains(c);

		private static Dictionary<Type, IParameterParser> mParsers = new Dictionary<Type, IParameterParser>();
		private static Dictionary<Type, Type> mGenericParsers = new Dictionary<Type, Type>();

		public static void AddParser<T>(ParameterParser<T> parser) => AddParser(typeof(T), parser);
		private static void AddParser(Type parseType, IParameterParser parser)
		{
			if (parseType == null) throw new ArgumentNullException(nameof(parseType));
			if (parser == null) throw new ArgumentNullException(nameof(parser));
			mParsers[parseType] = parser;
		}

		public static void BindGenericParser(Type genericTypeDefinition, Type parserType)
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

		private static object GetDefault(Type t)
		{
			// Get default value of type
			if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
				return Activator.CreateInstance(t);
			else return null;
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
		internal static IParameterParser GetParser(Type parseType)
		{
			if (mParsers.ContainsKey(parseType)) return mParsers[parseType];

			void Throw() { throw new ParserNotFoundException(nameof(parseType)); }

			// No concrete parser has been instantiated for this type,
			// let's see if we can generate one using generic type definitions and reflection

			Type constructedParserType;

			if (parseType.IsArray && parseType.GetArrayRank() == 1)
			{
				// Special case for parse types that are 1D arrays
				constructedParserType = typeof(ArrayParser<>).MakeGenericType(parseType.GetElementType());
			}
			else if (TupleParser.IsTupleType(parseType) || TupleParser.IsValueTupleType(parseType))
			{
				// Special case for tuple classes
				constructedParserType = typeof(TupleParser<>).MakeGenericType(parseType);
			} else
			{
				if (parseType.ContainsGenericParameters) Throw(); // Type is an unbounded generic type, we cannot do anything with this as generic arguments are unknown
				if (!parseType.IsGenericType) Throw(); // Check that type is indeed a generic type, such as List<int>, for which a parser can be generated

				var genericTypeDefinition = parseType.GetGenericTypeDefinition(); // Get the open generic type definition, such as List<> or IEnumerable<>
				var typeArguments = parseType.GenericTypeArguments; // Get the array of type arguments. For List<int>, this would return [ System.Int32 ]

				if (!mGenericParsers.ContainsKey(genericTypeDefinition)) Throw(); // No corresponding parser set for this generic type definition
				var genericParserType = mGenericParsers[genericTypeDefinition];

				// Construct the new parser type by plugging in the generic type arguments
				constructedParserType = genericParserType.MakeGenericType(typeArguments);
			}

			// Create an instance of this parser
			var parser = Activator.CreateInstance(constructedParserType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder)null, new object[0], null) as IParameterParser;

			// Add this parser to the dictionary of other parsers we do not have to repeat this process for the same generic type in the future
			AddParser(parseType, parser);

			return parser;
		}

		public static string GetTypeName<T>() => GetTypeName(typeof(T));

		private static string GetTypeName(Type type) {
			if (type == null)
				throw new ArgumentNullException(nameof(type));
			return GetParser(type).GetTypeName();
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
			public string LongName { get; private set; }
			public string Description { get; private set; }
			public int Min { get; private set; } = -1;
			public int Max { get; private set; } = -1;
			public string MetaVariable { get; private set; }
			public bool Required { get; private set; }
			public char ShortName { get; private set; }
			public Type FieldType { get; private set; }

			private Type mOptionalParserType;
			private object mOptionalDefaultValue;

			internal IParameterParser mParser;
			internal IParameterParser Parser
			{
				get
				{
					if (mParser != null) return mParser;
					mParser = GetOrCreateParser();
					return mParser;
				}
			}

			internal object DefaultValue
			{
				get
				{
					// Attempt to generate a default value.
					if (mOptionalDefaultValue == null)
					{
						// No default value specified, use default(T)
						return GetDefault(FieldType);
					} else
					{
						if (FieldType.IsAssignableFrom(mOptionalDefaultValue.GetType())) return mOptionalDefaultValue;
						else
						{
							// Supplied default value does not match field type
							// Is this a string representation?
							if (typeof(string) == mOptionalDefaultValue.GetType())
							{
								// Attempt to parse a default value
								var rawInput = mOptionalDefaultValue as string;
								var argInput = new ArgumentInput(rawInput);
								argInput.TrimWhitespace();
								if (!Parser.TryParse(argInput, out var defaultValue)) return GetDefault(FieldType); // Failed to parse string representation of default value, use default(T)
								return defaultValue;
							} else
							{
								// Not a string representation, and neither is the field type a string representation
								// Return default(T)
								return GetDefault(FieldType);
							}
						}
					}
				}
			}

			public string ParserFormat => Parser.GetFormat();

			private IParameterParser GetOrCreateParser ()
			{
				// If a custom parser was specified, create it
				if (mOptionalParserType != null)
					return Activator.CreateInstance(mOptionalParserType, true) as IParameterParser;
				return GetParser(FieldType);
			}

			public static Parameter FromParameterInfo (ParameterInfo info)
			{
				var parameter = new Parameter();
				parameter.FieldType = info.ParameterType;

				// Fill in default values
				parameter.LongName = info.Name;
				parameter.ParamType = ParameterType.Positional;
				parameter.Required = true;
				parameter.MetaVariable = info.Name;

				// Check to see if an attribute was attached, and attempt to add relevant information
				var parameterAttribute = info.GetCustomAttribute<ParameterAttribute>();
				if (parameterAttribute == null) return parameter; // No attribute attached, nothing more we can infer about this parameter

				if (parameterAttribute.Meta != null)
				{
					parameter.MetaVariable = parameterAttribute.Meta;
				}

				// Handle min and max elements.
				// Min and max can only be assigned if the field type inherits from IEnumerable
				if (typeof(IEnumerable).IsAssignableFrom(parameter.FieldType))
				{
					parameter.Min = parameterAttribute.Min;
					parameter.Max = parameterAttribute.Max;
				}

				parameter.Description = parameterAttribute.Description;
				parameter.mOptionalParserType = parameterAttribute.Parser;
				if (parameterAttribute.Meta != null) parameter.MetaVariable = parameterAttribute.Meta;

				if (parameterAttribute is SwitchAttribute switchAttribute)
				{
					// Handle logic for switch attributes
					parameter.LongName = switchAttribute.LongName;
					parameter.ShortName = switchAttribute.ShortName;

					// Check if this is a switch or a flag (a special kind of switch)
					// Flags can only be applied to boolean arguments
					if (info.ParameterType == typeof(bool) && switchAttribute.Flag)
					{
						parameter.ParamType = ParameterType.Flag;
						parameter.Required = false;
					}
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

				// Get default value (in the case of non-flag parameters)
				if (parameter.ParamType != ParameterType.Flag)
					parameter.mOptionalDefaultValue = parameterAttribute.Default;

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
			// Also check that no two parameters have the same name,
			// that no two parameters have the same metavariable,
			// and that no two non-positional parameters have the same short name

			ISet<string> names = new HashSet<string>();
			ISet<string> metavars = new HashSet<string>();
			ISet<char> shortNames = new HashSet<char>();

			foreach (var arg in arguments)
			{
				var param = Parameter.FromParameterInfo(arg);

				if (param.LongName != null)
				{
					if (names.Contains(param.LongName))
						throw new InvalidOperationException($"Multiple switch parameters found with the same long name: {param.LongName}. Each switch parameter must have a distinct long name.");
					names.Add(param.LongName);
				}

				if (metavars.Contains(param.MetaVariable))
					throw new InvalidOperationException($"Multiple parameters found with the same metavariable: {param.MetaVariable}. Each parameter must have a distinct metavariable.");
				metavars.Add(param.MetaVariable);

				if (param.ParamType != ParameterType.Positional)
				{
					if (shortNames.Contains(param.ShortName))
						throw new InvalidOperationException($"Multiple switches or flag parameters found with the same short name: {param.ShortName}. Each switch or flag parameter must have a distinct short name.");
					shortNames.Add(param.ShortName);
				}

				parser.mParameterIndexes[param] = parser.mPositionals.Count + parser.mNonPositionals.Count;

				parser.Add(param);
			}

			return parser;
		}

		private List<Parameter> mPositionals = new List<Parameter>();
		private List<Parameter> mNonPositionals = new List<Parameter>();
		private Dictionary<Parameter, int> mParameterIndexes = new Dictionary<Parameter, int>();

		private Parser() { }

		private void Add (Parameter parameter)
		{
			if (parameter.ParamType == ParameterType.Positional)
			{
				if (!parameter.Required || mPositionals.Count == 0) mPositionals.Add(parameter);
				else
				{
					// This is a required positional parameter, and should be placed before all optionals
					int insertionIndex = mPositionals.Count;
					for (int i = mPositionals.Count - 1; i >= 0; i--)
						if (!mPositionals[i].Required) insertionIndex = i;
					mPositionals.Insert(insertionIndex, parameter);
				}
			}
			else mNonPositionals.Add(parameter);
		}

		/// <summary>
		/// Attempts to parse a long option appearing at the beginning of <paramref name="input"/>
		/// </summary>
		/// <param name="input">The object representing the command line input.</param>
		/// <param name="parameter">The resulting parameter, null if none found.</param>
		/// <param name="value">The resulting value associated with the long option, null if none found.</param>
		/// <returns>A boolean indicating whether or not a long option was found.</returns>
		private bool TryParseLongOption (ArgumentInput input, out Parameter parameter, out object value)
		{
			parameter = null;
			value = null;
			foreach (var param in mNonPositionals)
			{
				if (input.Match(param.LongName))
				{
					// Found a parameter with this long option name
					input.Claim(param.LongName.Length); // Claim the parameter name

					// If this is a flag parameter, there should be whitespace or EOL appearing immediately afterwards this parameter.
					if (param.ParamType == ParameterType.Flag)
					{
						if (input.Length > 0 && !char.IsWhiteSpace(input[0])) return false;
						value = true;
						parameter = param;
						return true;
					} else
					{
						// Not a flag parameter, should be of the form --OPTION ARG or --OPTION=ARG
						if (input.Length == 0) return false; // Edge case

						if (input[0] == '=') input.Claim(); // Claim optional equals
						else if (char.IsWhiteSpace(input[0])) input.TrimWhitespace(); // Trim whitespace between long option and value
						else return false; // There MUST be an '=' character or whitespace between a long option and its value

						// Attempt to parse parameter value
						if (!param.Parser.TryParse(input, out value)) return false; // Failed to parse

						parameter = param;
						return true;
					}
				}
			}

			return false; // No long option found
		}

		/// <summary>
		/// Attempts to a parse short option (i.e. <c>-a arg</c> or <c>-aarg</c>) or a series of combined flag options (i.e. <c>-abc</c> is equivalent to <c>-a -b -c</c>) at the beginning of this input.
		/// </summary>
		/// <param name="input">The object representing the command line input</param>
		/// <param name="parameters">An array of parameters that were identified as a result of this method</param>
		/// <param name="values">An array of values whose indices correspond to the identified parameters stored in <paramref name="parameters"/></param>
		/// <returns>A boolean indicating whether or not parsing was completed successfully.</returns>
		private bool TryParseShortOptions (ArgumentInput input, out Parameter[] parameters, out object[] values)
		{
			parameters = new Parameter[0];
			values = new object[0];

			// Returns the parameter corresponding the given short name, 
			// or null if none was found.
			Parameter GetOption (char shortName)
			{
				foreach (var param in mNonPositionals)
					if (param.ShortName == shortName)
						return param;
				return null;
			}

			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Edge case for whitespace or EOL

			Parameter current = GetOption(input[0]);
			if (current == null) return false; // No parameter found for this short name

			if (current.ParamType == ParameterType.Switch)
			{
				// This is a short option with a parameter after it
				// Valid syntaxes that follow:
				//	[OPTION][ARGUMENT]
				//	[OPTION][WHITESPACE][ARGUMENT]

				input.Claim();
				input.TrimWhitespace(); // Trim whitespace between option and value if necessary
				if (input.Length == 0) return false; // Edge case for EOL

				// Parse argument
				if (!current.Parser.TryParse(input, out object value)) return false; // Failed to parse value

				parameters = new[] { current };
				values = new[] { value };
			} else if (current.ParamType == ParameterType.Flag)
			{
				// This is a flag, or multiple flags
				// Valid syntaxes that follow:
				// [OPTION]			i.e. -a
				// [OPTION...]		i.e. -abc, where -b and -c are both flag parameters

				List<Parameter> parametersList = new List<Parameter>();
				List<object> valuesList = new List<object>();

				while (current != null)
				{
					if (current.ParamType != ParameterType.Flag) return false; // All merged options must be flag options
					parametersList.Add(current);
					valuesList.Add(true);

					input.Claim(); // Consume character representing this option
					if (input.Length == 0 || char.IsWhiteSpace(input[0])) current = null; // Handle EOL or whitespace
					else
					{
						// Try to find another parameter
						current = GetOption(input[0]);
						if (current == null) return false; // No option corresponds to this short name; invalid
					}
				}

				parameters = parametersList.ToArray();
				values = valuesList.ToArray();
			}

			return true;
		}

		public bool TryParse (string input, out object[] arguments)
		{
			arguments = null;
			var args = new object[mPositionals.Count + mNonPositionals.Count];

			ArgumentInput argInput = new ArgumentInput(input);

			// Function to check that all required parameters have been satisfied
			bool AllSatisfied(ISet<Parameter> parameters)
			{
				foreach (Parameter param in mPositionals)
					if (param.Required && !parameters.Contains(param)) return false;
				foreach (Parameter param in mNonPositionals)
					if (param.Required && !parameters.Contains(param)) return false;
				return true;
			}

			// Keep track of all parameters that have been used so far
			// to prevent duplicate parameters in input and to ensure that
			// all required parameters have been parsed
			var parametersUsed = new HashSet<Parameter>();

			bool TryAddParameter (Parameter parameter, object value)
			{
				if (parametersUsed.Contains(parameter)) return false; // Duplicate parameter

				// If this object is an IEnumerable, check that it has the right counts
				if (value != null && value is IEnumerable e)
				{
					var count = 0;
					foreach (var item in e) count++;
					if ((parameter.Min >= 0 && count < parameter.Min)
						|| (parameter.Max >= 0 && count > parameter.Max))
					{
						return false; // invalid amount of items
					}
				}

				// Find the index in the arguments array of this parameter, and set its value
				args[mParameterIndexes[parameter]] = value;
				parametersUsed.Add(parameter);

				return true;
			}

			bool VerifySwitch (bool canAcceptOptions)
			{
				// Switchs (such as -f or --file) can often be confused with negative numeric positional
				// arguments -6.05 or -.8
				//
				// This function serves to disambiguate this case.
				// If this function returns true, the parser should treat the incoming input as a switch,
				// if it returns false, it should treat it as a positional argument.

				if (argInput[0] == '-' && canAcceptOptions)
				{
					if (argInput.Length >= 2)
					{
						if (char.IsDigit(argInput[1]) || argInput[1] == '.')
						{
							foreach (var param in mNonPositionals)
							{
								// Check if there are any switches of the form -0, -1, -2, ... -9, or -.
								if (param.ShortName == argInput[1]) return true;
							}
							return false;
						}
						else if (char.IsWhiteSpace(argInput[1])) return false;
						else return true;
					}
					else if (argInput.Length == 1) return false; // Single dash, not a switch
					else return true;
				}
				else return false;
			}

			// A sentinel value to keep track of whether or not options can be entered.
			// The parser can be forced to no longer accept options when '--' is found
			bool canAcceptOptions = true;

			// The index of the current positional argument to be parsed and filled in
			int positionalIndex = 0; 

			// Attempt to parse parameters until all input has been consumed
			while (argInput.Length > 0)
			{
				argInput.TrimWhitespace();
				if (argInput.Length == 0)
				{
					if (!AllSatisfied(parametersUsed)) return false; // Reached EOL, but haven't found all parameters
					else break; // All required arguments satisfied, now break
				}

				// Parse switches and flags
				if (VerifySwitch(canAcceptOptions))
				{
					// Found a flag, switch, or end-of-options term ('--')

					if (argInput.Length < 2) return false; // Invalid, possible cases are -SHORTNAME, --LONGNAME, or end-of-options ('--')
					argInput.Claim(); // Claim first '-'

					if (argInput[0] == '-')
					{
						argInput.Claim(); // Claim second '-'

						if (argInput.Length == 0 || char.IsWhiteSpace(argInput[0])) // Matches "--[WHITESPACE...]" or "--[EOL]", i.e. the end-of-options symbol
						{
							canAcceptOptions = false;
						}
						else
						{
							// Long option form, i.e. --foo or --foo-bar
							// Attempt to match one of the long options
							if (!TryParseLongOption(argInput, out var parameter, out object value)) return false; // Failed to parse long option

							if (!TryAddParameter(parameter, value)) return false; // Duplicate parameter
						}
					}
					else
					{
						// Only one dash present, this is a short options symbol, and must be handled appropriately
						if (!TryParseShortOptions(argInput, out Parameter[] parameters, out object[] values)) return false; // Failed to parse short options

						for (int i = 0; i < parameters.Length; i++)
							if (!TryAddParameter(parameters[i], values[i])) return false; // Duplicate parameter
					}
				}
				else
				{
					// Not a switch or a flag, attempt to parse a positional
					if (positionalIndex >= mPositionals.Count) return false; // Unexpected argument

					var positionalParameter = mPositionals[positionalIndex];
					if (!positionalParameter.Parser.TryParse(argInput, out var value)) return false; // Failed to parse positional parameter
					if (!TryAddParameter(positionalParameter, value)) return false; // Duplicate parameter?
					positionalIndex++;
				}
			}

			if (!AllSatisfied(parametersUsed)) return false; // Not all required arguments satisfied

			// Parameters with an unassigned default value:
			//	Must ensure that these are assigned their default value
			foreach (var param in mPositionals)
				if (!parametersUsed.Contains(param)) args[mParameterIndexes[param]] = param.DefaultValue;
			foreach (var param in mNonPositionals)
				if (!parametersUsed.Contains(param)) args[mParameterIndexes[param]] = param.DefaultValue;

			arguments = args;

			return true;
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
			foreach (var param in nonPositionalsSorted)
			{
				if (param.ParamType == ParameterType.Flag)
					// Flags
					sb.Append(param.Required ? $"-{param.ShortName} " : $"[-{param.ShortName}] ");
				else
					// Switches
					sb.Append(param.Required ? $"-{param.ShortName} {param.MetaVariable} " : $"[-{param.ShortName} {param.MetaVariable}] ");
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
			const int padding12 = 8; // padding between console columns 1 and 2
			const int padding23 = 8; // padding between console columns 2 and 4 (in the parameter format section)

			int MaxLength(IEnumerable<string> strings) => strings.Max(s => s?.Length ?? 0);

			string ParameterNames (Parameter param)
			{
				if (param.ParamType == ParameterType.Positional) return param.MetaVariable;
				else if (string.IsNullOrEmpty(param.LongName)) return $"-{param.ShortName}";
				else return $"-{param.ShortName}, --{param.LongName}";
			}

			StringBuilder sb = new StringBuilder()
				.Append(CalculateUsage(commandName));

			var allParams = Enumerable.Concat(mPositionals, mNonPositionals);

			// This method will show four groups of text:
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
				formatGroup.Add(new List<string> { argNamesBuilder.ToString(), GetTypeName(pair.Key), pair.Value[0].Parser.GetFormat() });
			};

			// Order the format group so that the arguments with complex format come first
			formatGroup.Sort((a, b) => string.IsNullOrEmpty(a[2]).CompareTo(string.IsNullOrEmpty(b[2])));

			// Get parameter description for this parameter, including any range restrictions
			List<List<string>> GetDescription(Parameter parameter)
			{
				List<string> rightColumn = new List<string>();
				if (parameter.Description != null) rightColumn.Add(parameter.Description);

				string description = null;
				if (parameter.Min < 0 && parameter.Max < 0) description = null; // there are no enumeration restrictions on this parameter
				else if (parameter.Min == 0 && parameter.Max == 0) description = "May not contain any elements";
				else if (parameter.Min == parameter.Max)
				{
					description = $"May contain {parameter.Min} and only {parameter.Min} element";
					if (parameter.Min > 1) description += "s"; // Make plural
				}
				else if (parameter.Min >= 0 && parameter.Max >= 0)
					description = $"May have between {parameter.Min} and {parameter.Max} element";
				else if (parameter.Min >= 0)
				{
					description = $"Must have at least {parameter.Min} element";
					if (parameter.Min > 1 || parameter.Min == 0) description += "s"; // Make plural
				}
				else if (parameter.Max >= 0)
				{
					description = $"Can have at most {parameter.Max} element";
					if (parameter.Max > 1 || parameter.Max == 0) description += "s";  // Make plural
				}

				if (description != null) rightColumn.Add(description);

				var rows = new List<List<string>>();
				if (rightColumn.Count > 0)
				{
					rows.Add(new List<string>() { ParameterNames(parameter), rightColumn[0] });
					for (int i = 1; i < rightColumn.Count; i++) rows.Add(new List<string>() { string.Empty, rightColumn[i] });
				}
				return rows;
			}

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
				mandatoryGroup.AddRange(GetDescription(param));
			}

			// Optional parameters:
			//	The same as the mandatory parameters, but using only optional params only
			foreach (var param in allParams)
			{
				if (param.Required) continue; // No mandatory params here
				optionalGroup.AddRange(GetDescription(param));
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
				var format = $"{{0, {leftColAlignment}}}{{1, {middleColAlignment}}}{{2}}";
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
