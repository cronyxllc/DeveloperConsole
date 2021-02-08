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
		public override bool TryParse(ArgumentInput input, out IEnumerable<T> result)
		{
			// This parser will recursively parse and return an IEnumerable of elements whose individual types are T
			// by using the parser for T
			//
			// Each element may optionally be separated by 1 (ONE) comma (','). Duplicate commas and leading or trailing
			// commas are not allowed.
			//
			// The IEnumerable MUST be surrounded by one of the following grouping symbols:
			//		(*) Parenthesis, i.e. '(' and ')'
			//		(*) Brackets, i.e. '[' and ']'
			//
			// Intermixing grouping symbols is not allowed

			result = null;
			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Unexpected EOL or whitespace

			char endGrouping;
			if (input[0] == '(') endGrouping = ')';
			else if (input[0] == '[') endGrouping = ']';
			else return false; // No grouping symbol found, malformed input

			input.Claim(1); // Claim initial grouping char

			List<T> elements = new List<T>(); // Using list as the backing type, as it is simple
			ParameterParser<T> elementParser = GetParser<T>();

			// Claim as many elements as possible
			while (true)
			{
				input.TrimWhitespace();
				if (input.Length == 0) return false; // Unexpected EOL

				if (input[0] == endGrouping)
				{
					input.Claim(1); // Claim end grouping char
					break; // Finished list of elements
				} else if (input[0] == ',') return false; // Unexpected seperator

				if (!elementParser.TryParse(input, out T element)) return false; // Failed to parse element
				elements.Add(element);

				input.TrimWhitespace(); // Trim any whitespace occuring after element

				if (input.Length == 0) return false; // Unexpected EOL
				if (input[0] == ',')
				{
					input.Claim(1); // Claim seperator comma
					input.TrimWhitespace();
					if (input.Length == 0 || input[0] == ',' || input[0] == endGrouping) return false; // Unexpected EOL, duplicate comma, or trailing comma before grouping char
				}
			}

			input.TrimWhitespace(); // Trim any whitespace occuring after this object

			result = elements;
			return true;
		}

		public override string GetFormat() => $"({GetParser<T>().GetFormat() ?? "foo bar"} ...)";

		public override string GetTypeName() => $"{nameof(IEnumerable)}<{GetTypeName<T>()}>";
	}
}
