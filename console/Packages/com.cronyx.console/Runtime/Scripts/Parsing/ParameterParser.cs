using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	internal interface IParameterParser
	{
		string GetTypeName();
		string GetFormat();
		bool TryParse(ArgumentInput input, out object result);
	}

	public abstract class ParameterParser<T> : IParameterParser
	{
		bool IParameterParser.TryParse(ArgumentInput input, out object result)
		{
			var success = TryParse(input, out T res);
			result = res;
			return success;
		}

		public abstract bool TryParse(ArgumentInput input, out T result);

		public virtual string GetFormat() => null;
		public virtual string GetTypeName() => typeof(T).Name;

		protected ParameterParser<G> GetParser<G>() => Parser.GetParser<G>();
		protected string GetTypeName<G>() => Parser.GetTypeName<G>();
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
				mParser = GetParser<TBase>();
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
