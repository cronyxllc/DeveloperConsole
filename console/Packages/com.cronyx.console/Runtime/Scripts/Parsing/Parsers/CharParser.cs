using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	public class CharParser : ParameterParser<char>
	{
		public override bool TryParse(ArgumentInput input, out char result)
		{
			// This parser should match ONE and ONLY ONE character in the input.
			// If multiple characters appear together without whitespace between them,
			// this is a faulty input.
			//
			// Syntax:
			//	[CHAR][WHITESPACE]

			result = '\0';
			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Unexpected EOL or whitespace
			if (input.Length > 1 && !char.IsWhiteSpace(input[1])) return false; // Second character was not whitespace, malformed input 
			result = input[0];
			input.Claim(); // Claim the character
			return true;
 		}

		public override string GetTypeName() => "char";
	}
}
