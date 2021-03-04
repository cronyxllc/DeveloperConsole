using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	public class KeyValuePairParser<TKey, TValue> : ParameterParser<KeyValuePair<TKey, TValue>>
	{
		public override bool TryParse(ArgumentInput input, out KeyValuePair<TKey, TValue> result)
		{
			// This parser will parse a single key/value pair.
			// Valid syntaxes:
			//	<KEYTYPE>[WHITESPACE]<COLON>[WHITESPACE]<VALUETYPE>

			result = default;

			var keyParser = Parser.GetParser<TKey>();
			var valueParser = Parser.GetParser<TValue>();

			if (!keyParser.TryParse(input, out TKey key)) return false; // Failed to parse key
			input.TrimWhitespace();

			if (input.Length == 0 || input[0] != ':') return false; // No colon found
			input.Claim(); // Claim colon

			input.TrimWhitespace();
			if (!valueParser.TryParse(input, out TValue value)) return false; // Failed to parse value

			result = new KeyValuePair<TKey, TValue>(key, value);

			return true;
		}

		public override string GetFormat() => $"{Parser.GetParser<TKey>().GetFormat() ?? "key"}: {Parser.GetParser<TValue>().GetFormat() ?? "value"}";
		public override string GetTypeName() => $"KeyValuePair<{Parser.GetTypeName<TKey>()},{Parser.GetTypeName<TValue>()}>";

		public KeyValuePairParser ()
		{
			Parser.AddSpecialChar(':'); // Add colon special char
		}
	}
}
