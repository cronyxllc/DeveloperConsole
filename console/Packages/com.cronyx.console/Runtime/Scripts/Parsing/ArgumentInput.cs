using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	public class ArgumentInput
	{
		public int Length => mInput.Length;
		public char this[int index] => mInput[index];

		private StringBuilder mInput;

		internal ArgumentInput(string input)
		{
			if (input == null) input = string.Empty;
			mInput = new StringBuilder(input);
		}

		public void Claim(int count) => mInput.Remove(0, count);

		public bool Match (string token, StringComparison comparison = StringComparison.InvariantCulture)
		{
			if (Length < token.Length) return false;
			return mInput.ToString(0, token.Length).Equals(token);
		}

		/// <summary>
		/// Trims all whitespace from the beginning of the input feed.
		/// </summary>
		public void TrimWhitespace ()
		{
			while (Length > 0 && char.IsWhiteSpace(mInput[0]))
				Claim(1);
		}

		public override string ToString() => mInput.ToString();
	}
}
