using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cronyx.Console.Parsing.Parsers;

namespace Cronyx.Console.Parsing
{
	internal interface IParameterParser
	{
		string GetTypeName();
		string GetFormat();
		bool TryParse(ArgumentInput input, out object result);
	}

	/// <summary>
	/// An object that parses and returns an object of type <typeparamref name="T"/> from a string representation.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public abstract class ParameterParser<T> : IParameterParser
	{
		bool IParameterParser.TryParse(ArgumentInput input, out object result)
		{
			var success = TryParse(input, out T res);
			result = res;
			return success;
		}

		/// <summary>
		/// Attempts to parse and store an object of type <typeparamref name="T"/>.
		/// </summary>
		/// <param name="input">An object containing the raw string input passed to this parser.</param>
		/// <param name="result">An out parameter that stores the result of this parser. If parsing fails, set to <c>null</c> or <c>default(<typeparamref name="T"/>)</c></param>
		/// <returns>A boolean value representing whether this parser succeeded in parsing the object. Return true if parsing succeeded, and false if parsing failed.</returns>
		public abstract bool TryParse(ArgumentInput input, out T result);

		/// <summary>
		/// Returns a string representing the complex format of this object.
		/// </summary>
		/// <remarks>
		/// <para>For simple types like <see cref="string"/> and <see cref="int"/>, return null.</para>
		/// <para>
		/// For objects with complex formats, return a template for that format. For instance, <see cref="IEnumerableParser{T}.GetFormat"/> returns <c>"[foo bar ...]"</c> to indicate that an arbitrary number of objects can be parsed.
		/// This string serves to remind users what the format of this object should be when it is entered to the console.</para>
		/// </remarks>
		/// <returns>A string representing the complex format of this object, or <c>null</c> for objects without complex format.</returns>
		public virtual string GetFormat() => null;

		/// <summary>
		/// Returns a string representing the formatted type name of this object. Override in derived classes for parsers with non-standard type names, such as those with generic type arguments.
		/// </summary>
		/// <returns>A string representing the formatted type name of this object, such as <c>"string"</c> or <c>"IEnumerable&lt;int&gt;"</c></returns>
		public virtual string GetTypeName() => typeof(T).Name;
	}

	/// <summary>
	/// A <see cref="ParameterParser{T}"/> for values of type <typeparamref name="TAlias"/> that uses the parser for <typeparamref name="TBase"/>,
	/// provided there is way to convert from values of type <typeparamref name="TBase"/> to <typeparamref name="TAlias"/>.
	/// </summary>
	/// <typeparam name="TBase"></typeparam>
	/// <typeparam name="TAlias"></typeparam>
	public abstract class AliasParser<TBase, TAlias> : ParameterParser<TAlias>
	{
		private ParameterParser<TBase> mParser;
		protected ParameterParser<TBase> BaseParser
		{
			get
			{
				if (mParser != null) return mParser;
				mParser = Parser.GetParser<TBase>();
				return mParser;
			}
		}

		public override bool TryParse(ArgumentInput input, out TAlias result)
		{
			result = default;
			if (!BaseParser.TryParse(input, out TBase baseValue)) return false;
			result = Convert(baseValue);
			return true;
		}

		/// <summary>
		/// Converts an object of type <typeparamref name="TBase"/> to an object of type <typeparamref name="TAlias"/>
		/// </summary>
		/// <param name="baseValue">An object of type <typeparamref name="TBase"/></param>
		/// <returns>An object of type <typeparamref name="TAlias"/></returns>
		public abstract TAlias Convert (TBase baseValue);

		public override string GetFormat() => BaseParser.GetFormat();
	}

	/// <summary>
	/// A special kind of <see cref="AliasParser{TBase, TAlias}"/> that supports covariant generic parameters,
	/// that is, when <typeparamref name="TBase"/> is a subclass of <typeparamref name="TAlias"/>.
	/// </summary>
	/// <typeparam name="TBase"></typeparam>
	/// <typeparam name="TAlias"></typeparam>
	public abstract class CovariantParser <TBase, TAlias> : AliasParser<TBase, TAlias> where TBase : TAlias
	{
		public override TAlias Convert(TBase baseValue) => baseValue;
	}
}
