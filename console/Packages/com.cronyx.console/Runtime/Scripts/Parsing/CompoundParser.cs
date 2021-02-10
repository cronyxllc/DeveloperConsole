using System;
using System.Collections.Generic;
using System.Linq;
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

		private IList<Type> mTypeSequence;

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

			var elements = new object[mTypeSequence.Count];
			int elementIndex = 0;

			while (true)
			{
				input.TrimWhitespace();
				if (input.Length == 0) return false; // Unexpected EOL

				if (input[0] == endGrouping)
				{
					if (elementIndex != elements.Length) return false; // Found end grouping symbol, but not all elements have been parsed!
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

			result = GetResult(elements);
			return true;
		}

		public CompoundParser()
		{
			if (GroupingChars == null || GroupingChars.Length == 0)
				throw new ArgumentException($"You must provide at least one pair of grouping characters to use!");

			mTypeSequence = GetTypes().ToList();
			if (mTypeSequence == null || mTypeSequence.Count == 0)
				throw new ArgumentException($"You must provide at least one type!");

			foreach (var c in GroupingChars.SelectMany(x => new[] { x.Beginning, x.End }))
				Parser.AddSpecialChar(c);
			Parser.AddSpecialChar(Seperator);
		}

		public override string GetFormat()
		{
			mFormatBuilder.Clear();
			mFormatBuilder.Append(GroupingChars[0].Beginning);

			for (int i = 0; i < mTypeSequence.Count; i++)
			{
				var parser = Parser.GetParser(mTypeSequence[i]);
				mFormatBuilder.Append(parser.GetFormat() ?? parser.GetTypeName());
				if (i != mTypeSequence.Count - 1) mFormatBuilder.Append(' ');
			}

			mFormatBuilder.Append(GroupingChars[0].End);
			return mFormatBuilder.ToString();
		}
	}

	/// <summary>
	/// A <see cref="ParameterParser{T}"/> that produces a <typeparamref name="TResult"/> object from one parameter.
	/// </summary>
	/// <remarks>
	/// <para>By default, this parser will parse an object with the following format:</para>
	/// <code>
	/// [T0]
	/// </code>
	/// <para>and will return a <typeparamref name="TResult"/> by passing the constituent parameters (<typeparamref name="T0"/>) to <c>GetResult</c>.</para>
	/// <para>By default, the grouping symbols used are parenthesis '()' and brackets '[],' but these can be changed by overriding <c>GroupingChars</c> in a derived class.
	/// Elements can be optionally seperated from one another using a comma ','. This default seperator character can be changed by overriding <c>Seperator</c>.</para>
	/// </remarks>
	/// <typeparam name="T0">The first parameter type in this compound type.</typeparam>
	/// <typeparam name="TResult">The type that this <see cref="ParameterParser{T}"/> produces.</typeparam>
	public abstract class CompoundParser<T0, TResult> : CompoundParser<TResult>
	{
		protected sealed override TResult GetResult(object[] elements) => GetResult((T0) elements[0]);
		
		protected sealed override IEnumerable<Type> GetTypes() => new[] { typeof(T0) };

		/// <summary>
		/// Constructs a <typeparamref name="TResult"/> object from its constituents.
		/// </summary>
		/// <param name="element1">The first parameter, whose type is <typeparamref name="T0"/></param>
		/// <returns>A <typeparamref name="TResult"/> object.</returns>
		protected abstract TResult GetResult(T0 element1);
	}
}
