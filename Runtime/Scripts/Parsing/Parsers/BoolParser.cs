using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	public class BoolParser : ParameterParser<bool>
	{
		private static readonly StringBuilder mBuilder = new StringBuilder();

		public override bool TryParse(ArgumentInput input, out bool result)
		{
			// Parses a boolean. This can accept a variety of inputs:
			//
			//	Full word:
			//		True => true (case-insensitive)
			//		False => false (case-insensitive)
			//
			//	Abbreviated word:
			//		t => true (case-insensitive)
			//		f => false (case-insensitive)
			//
			//	Numeric:
			//		1 => true
			//		0 => false

			mBuilder.Clear();
			result = false;

			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Unexpected EOL or whitespace

			while (input.Length > 0 && !char.IsWhiteSpace(input[0]) && !Parser.IsSpecial(input[0]))
			{
				mBuilder.Append(input[0]);
				input.Claim();
			}

			switch (mBuilder.ToString().ToLowerInvariant())
			{
				case "true":
					result = true;
					break;
				case "false":
					result = false;
					break;
				case "t":
					result = true;
					break;
				case "f":
					result = false;
					break;
				case "1":
					result = true;
					break;
				case "0":
					result = true;
					break;
				default:
					return false; // Doesn't match specified inputs
			}

			return true;
		}

		public override string GetTypeName() => "bool";
		public override string GetFormat() => "t/f";
	}
}
