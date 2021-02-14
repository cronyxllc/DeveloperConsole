using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	/// <summary>
	/// A wrapper around <see cref="string"/> that allows <see cref="ParameterParser{T}"/> instances to procedurally process raw input from the console.
	/// </summary>
	public class ArgumentInput
	{
		/// <summary>
		/// The number of unclaimed characters remaining in the input.
		/// </summary>
		public int Length => mInput.Length;

		/// <summary>
		/// Gets the character at <paramref name="index"/>
		/// </summary>
		/// <param name="index">The position of the character</param>
		/// <returns>The character at <paramref name="index"/></returns>
		/// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="index"/> is less than zero or greater than or equal to <see cref="Length"/></exception>
		public char this[int index] => mInput[index];

		private StringBuilder mInput;

		internal ArgumentInput(string input)
		{
			if (input == null) input = string.Empty;
			mInput = new StringBuilder(input);
		}

		/// <summary>
		/// Removes <paramref name="count"/> characters from the beginning of the input. If <paramref name="count"/> is not supplied, removes one character from the beginning of the input.
		/// </summary>
		/// <param name="count">The number of characters to claim from the beginning of the input.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than zero or greater than <see cref="Length"/></exception>
		public void Claim(int count=1) => mInput.Remove(0, count);

		/// <summary>
		/// Attempts to match a string at the beginning of the input.
		/// </summary>
		/// <remarks>
		/// If <see cref="Length"/> is less than the length of <paramref name="token"/>, returns false.
		/// </remarks>
		/// <param name="token">A string to match at the beginning of the input.</param>
		/// <param name="comparison">The <see cref="StringComparison"/> to use to compare <paramref name="token"/> to the input.</param>
		/// <returns>A boolean indicating whether or not <paramref name="token"/> was found at the beginning of the input.</returns>
		public bool Match (string token, StringComparison comparison = StringComparison.InvariantCulture)
		{
			if (Length < token.Length) return false;
			return mInput.ToString(0, token.Length).Equals(token);
		}

		/// <summary>
		/// Trims and claims all whitespace characters from the beginning of the input.
		/// </summary>
		public void TrimWhitespace ()
		{
			while (Length > 0 && char.IsWhiteSpace(mInput[0]))
				Claim();
		}

		/// <summary>
		/// Gets a string representing the current state of the input.
		/// </summary>
		/// <returns>A string representing the current state of the input.</returns>
		public override string ToString() => mInput.ToString();
	}
}
