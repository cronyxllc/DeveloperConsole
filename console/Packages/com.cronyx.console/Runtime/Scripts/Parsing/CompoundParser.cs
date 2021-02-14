using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	public abstract class CompoundParser<T> : ParameterParser<T>
	{
		private static readonly StringBuilder mFormatBuilder = new StringBuilder();

		/// <summary>
		/// A set of grouping characters that can be used to surround the parameters encoded by this <see cref="CompoundParser{T}"/>.
		/// Override in a derived class to change which grouping characters surround this type.
		/// </summary>
		/// <remarks>
		/// The first element of this array will be used to generate the format returned by <see cref="GetFormat"/>.
		/// </remarks>
		protected virtual (char Beginning, char End)[] GroupingChars { get; } = new[]
			{
				('[', ']'),
				('(', ')')
			};

		/// <summary>
		/// A character that can optionally be used to seperate the elements of this <see cref="CompoundParser{T}"/>.
		/// Override in a derived class to specify which character is used.
		/// </summary>
		protected virtual char Seperator { get; } = ',';

		private Type[] mTypeSequence;
		private object[] mDefaultValues;
		private bool[] mHasDefaultValue;

		/// <summary>
		/// Return the cached type sequence of this <see cref="CompoundParser{T}"/> without having to regenerate them by calling <see cref="GetTypes"/>
		/// </summary>
		public IReadOnlyList<Type> Types => mTypeSequence;

		/// <summary>
		/// Gets the sequence of types that constitute the compound parameter this object parses.
		/// </summary>
		/// <remarks>
		/// For instance, for an object that contains a float, a string, and an array of characters, such as the following:
		/// <code>
		/// [5.4 "hello world" [a b c d e]]
		/// </code>
		/// <see cref="GetTypes"/> might return:
		/// <code>
		/// new Type[] { typeof(float), typeof(string), typeof(char[]) }
		/// </code>
		/// </remarks>
		/// <returns>An <see cref="IEnumerable{T}"/> of <see cref="Type"/> objects.</returns>
		protected abstract IEnumerable<Type> GetTypes();

		/// <summary>
		/// Converts parsed objects into a compound type, <typeparamref name="T"/>.
		/// </summary>
		/// <param name="elements">An array of parsed objects. Has the same length as the length of the enumeration returned by <see cref="GetTypes"/></param>
		/// <returns>An object constructed from the parameters in <paramref name="elements"/></returns>
		/// <remarks>
		/// For a given index, the type of the object in <paramref name="elements"/> will correspond to the <see cref="Type"/> object at the same index in the enumeration returned by <see cref="GetTypes"/>.
		/// For instance, if the 2nd element of the enumeration returned by <see cref="GetTypes"/> equals typeof(bool), then the second element in <paramref name="elements"/> will either be a bool.
		/// </remarks>
		protected abstract T GetResult(object[] elements);

		public override bool TryParse(ArgumentInput input, out T result)
		{
			result = default;
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

			var elements = new object[mTypeSequence.Length];
			int elementIndex = 0;

			while (true)
			{
				input.TrimWhitespace();
				if (input.Length == 0) return false; // Unexpected EOL

				if (input[0] == endGrouping)
				{
					if (elementIndex < elements.Length)
					{
						// Not all parameters were found. Were all required parameters found?
						if (mHasDefaultValue[elementIndex])
						{
							// Set all default values
							for (int i = elementIndex; i < elements.Length; i++) elements[i] = mDefaultValues[i];
						} else return false; // Found end grouping symbol, but not all elements have been parsed!

					}
						
					input.Claim(); // Claim end grouping char
					break; // Finished list of elements
				}
				else if (input[0] == Seperator) return false; // Unexpected seperator

				if (elementIndex >= elements.Length) return false; // Too many elements
				if (!Parser.GetParser(mTypeSequence[elementIndex]).TryParse(input, out object element)) return false; // Failed to parse element
				elements[elementIndex] = element;
				elementIndex++;

				input.TrimWhitespace(); // Trim any whitespace occuring after element

				if (input.Length == 0) return false; // Unexpected EOL
				if (input[0] == Seperator)
				{
					input.Claim(); // Claim seperator
					input.TrimWhitespace(); // Claim any whitespace occuring after the seperator
					if (input.Length == 0 || input[0] == Seperator || input[0] == endGrouping) return false; // Unexpected EOL, duplicate seperator, or trailing seperator before grouping char
				}
			}

			try
			{
				result = GetResult(elements);
			} catch (Exception) { return false; }

			return true;
		}

		private void TryGetDefaultValues ()
		{
			mDefaultValues = new object[mTypeSequence.Length];
			mHasDefaultValue = new bool[mTypeSequence.Length];

			// Look for GetResult method in a derived type
			var method = GetType().GetMethod(nameof(GetResult), BindingFlags.Instance | BindingFlags.NonPublic, null,  mTypeSequence, null);
			if (method == null) return; // No method found

			var parameters = method.GetParameters();

			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i].HasDefaultValue)
				{
					mHasDefaultValue[i] = true;
					mDefaultValues[i] = parameters[i].DefaultValue;
				}
			}
		}

		public CompoundParser()
		{
			if (GroupingChars == null || GroupingChars.Length == 0)
				throw new ArgumentException($"You must provide at least one pair of grouping characters to use!");

			mTypeSequence = GetTypes().ToArray();
			if (mTypeSequence == null || mTypeSequence.Length == 0)
				throw new ArgumentException($"You must provide at least one type!");

			TryGetDefaultValues();

			foreach (var c in GroupingChars.SelectMany(x => new[] { x.Beginning, x.End }))
				Parser.AddSpecialChar(c);
			Parser.AddSpecialChar(Seperator);
		}

		public override string GetFormat()
		{
			mFormatBuilder.Clear();
			mFormatBuilder.Append(GroupingChars[0].Beginning);

			for (int i = 0; i < mTypeSequence.Length; i++)
			{
				var parser = Parser.GetParser(mTypeSequence[i]);
				mFormatBuilder.Append(parser.GetFormat() ?? parser.GetTypeName());
				if (i != mTypeSequence.Length - 1) mFormatBuilder.Append(' ');
			}

			mFormatBuilder.Append(GroupingChars[0].End);
			return mFormatBuilder.ToString();
		}
	}
}
