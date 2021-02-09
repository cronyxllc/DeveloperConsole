using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	public class StringParser : ParameterParser<string>
	{
		private static readonly StringBuilder mBuilder = new StringBuilder();

		public override bool TryParse(ArgumentInput input, out string result)
		{
			// This parser will look for a string of characters that are either surrounded by single or double quotes
			//
			// If the string is surrounded by quotes, it will include everything between the first quote character found
			// and the last unescaped quote character. Quote characters may be escaped using \"
			//
			// If the string is not surrounded by quotes, it will start with the first character and stop at a special
			// character or whitespace.

			mBuilder.Clear();

			result = null;
			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Unexpected EOL or whitespace

			char? quoteChar = null;
			if (input[0] == '\'') quoteChar = '\'';
			else if (input[0] == '"') quoteChar = '"';

			if (quoteChar != null) input.Claim(); // Claim the quote character, if it was found

			// Begin scanning for the string
			while (input.Length > 0)
			{
				if (quoteChar != null)
				{
					if (input.Match("\\" + quoteChar))
					{
						// Escaped quotechar
						mBuilder.Append(quoteChar);
						input.Claim(2);
						continue;
					} else if (input[0] == quoteChar)
					{
						input.Claim(); // Claim end-quote
						break;
					}
				} else
				{
					if (char.IsWhiteSpace(input[0])) break; // Found whitespace not inside quotes
					else if (Parser.IsSpecial(input[0])) break; // Special character not inside quotes, must stop
				}

				mBuilder.Append(input[0]);
				input.Claim();
			}

			result = mBuilder.ToString();
			return true;
		}

		public override string GetTypeName() => "string";
	}
}
