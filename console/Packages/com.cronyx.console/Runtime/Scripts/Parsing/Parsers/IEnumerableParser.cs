using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	public class IEnumerableParser<T> : ParameterParser<IEnumerable<T>>
	{
		protected virtual char Seperator => ',';
		protected virtual (char Beginning, char End)[] GroupingChars => new[]
			{
				('(', ')'),
				('[', ']')
			};

		public sealed override bool TryParse(ArgumentInput input, out IEnumerable<T> result)
		{
			// This parser will recursively parse and return an IEnumerable of elements whose individual types are T
			// by using the parser for T. It will add these elements to another collection whose type is BackingType.
			//
			// Each element may optionally be separated by 1 (ONE) comma (','). Duplicate commas and leading or trailing
			// commas are not allowed.
			//
			// The IEnumerable MUST be surrounded by one of the grouping symbols defined by this class or a more derived one.
			//
			// Intermixing grouping symbols is not allowed

			result = null;
			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Unexpected EOL or whitespace

			char? endGrouping = null;
			foreach (var grouping in GroupingChars)
			{
				if (input[0] == grouping.Beginning)
				{
					endGrouping = grouping.End;
					break;
				}
			}
			if (endGrouping == null) return false; // No beginning grouping symbol found, malformed input

			input.Claim(); // Claim initial grouping char

			// Create backing type
			List<T> elements = new List<T>();
			ParameterParser<T> elementParser = GetParser<T>();

			// Claim as many elements as possible
			while (true)
			{
				input.TrimWhitespace();
				if (input.Length == 0) return false; // Unexpected EOL

				if (input[0] == endGrouping)
				{
					input.Claim(); // Claim end grouping char
					break; // Finished list of elements
				} else if (input[0] == Seperator) return false; // Unexpected seperator

				if (!elementParser.TryParse(input, out T element)) return false; // Failed to parse element
				elements.Add(element);

				input.TrimWhitespace(); // Trim any whitespace occuring after element

				if (input.Length == 0) return false; // Unexpected EOL
				if (input[0] == Seperator)
				{
					input.Claim(); // Claim seperator
					input.TrimWhitespace(); // Claim any whitespace occuring after the seperator
					if (input.Length == 0 || input[0] == Seperator || input[0] == endGrouping) return false; // Unexpected EOL, duplicate seperator, or trailing comma before grouping char
				}
			}

			result = elements;
			return true;
		}

		public override string GetFormat() => $"[{GetParser<T>().GetFormat() ?? "foo bar"} ...]";

		public override string GetTypeName() => $"{nameof(IEnumerable)}<{GetTypeName<T>()}>";

		public IEnumerableParser ()
		{
			foreach (var c in GroupingChars.SelectMany(x => new[] { x.Beginning, x.End }))
				Parser.AddSpecialChar(c);
			Parser.AddSpecialChar(Seperator);
		}
	}
}
