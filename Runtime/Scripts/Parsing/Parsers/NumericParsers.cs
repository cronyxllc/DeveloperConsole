using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	public class IntegralParser<T> : ParameterParser<T> where T : struct
	{
		private static readonly StringBuilder mBuilder = new StringBuilder();

		private string mTypeName;
		private Func<string, T> mParser;

		internal IntegralParser (string primitiveName, Func<string, T> parse) {
			mTypeName = primitiveName;
			mParser = parse;
		}

		public override bool TryParse(ArgumentInput input, out T result)
		{
			result = default;

			mBuilder.Clear();
			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Unexpected EOL or whitespace

			// Check for negative or positive sign and claim it
			if (input[0] == '-' || input[0] == '+')
			{
				mBuilder.Append(input[0]);
				input.Claim(1);
			}

			while (input.Length > 0)
			{
				if (char.IsWhiteSpace(input[0])) break;
				else if (Parser.IsSpecial(input[0])) break; // Special character, must stop
				else if (!char.IsDigit(input[0])) return false; // Character not a digit

				mBuilder.Append(input[0]);
				input.Claim();
			}

			try
			{
				result = mParser(mBuilder.ToString());
			}
			catch (FormatException) { return false; }
			catch (OverflowException) { return false; }

			return true;
		}

		public override string GetTypeName() => mTypeName;
	}

	public class FloatingParser<T> : ParameterParser<T> where T : struct
	{
		private static readonly StringBuilder mBuilder = new StringBuilder();

		private string mTypeName;
		private Func<string, T> mParser;

		internal FloatingParser(string primitiveName, Func<string, T> parse)
		{
			mTypeName = primitiveName;
			mParser = parse;
		}

		public override bool TryParse(ArgumentInput input, out T result)
		{
			result = default;
			mBuilder.Clear();
			if (input.Length == 0 || char.IsWhiteSpace(input[0])) return false; // Unexpected EOL or whitespace

			// Check for negative or positive sign and claim it
			if (input[0] == '-' || input[0] == '+')
			{
				mBuilder.Append(input[0]);
				input.Claim(1);
			}

			bool decimalFound = false;
			while (input.Length > 0)
			{
				if (input[0] == '.')
				{
					if (decimalFound) return false;
					decimalFound = true;
				}
				else if (char.IsWhiteSpace(input[0])) break;
				else if (Parser.IsSpecial(input[0])) break; // Special character, must stop
				else if (!char.IsDigit(input[0])) return false; // Character not a digit

				mBuilder.Append(input[0]);
				input.Claim();
			}

			try
			{
				result = mParser(mBuilder.ToString());
			}
			catch (FormatException) { return false; }
			catch (OverflowException) { return false; }

			return true;
		}

		public override string GetTypeName() => mTypeName;

	}
}
