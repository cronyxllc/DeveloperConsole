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

		public virtual string GetTypeName() => GetType().Name;

		protected ParameterParser<G> GetParser<G>() => Parser.GetParser<G>();
		protected string GetTypeName<G>() => Parser.GetTypeName<G>();
	}
}
