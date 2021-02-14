using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	/// <summary>
	/// A parser for a nullable valuetype. 
	/// </summary>
	/// <remarks>
	/// When parsing, simply parses and returns the value of the underlying type. No special handling is applied to nullable value types.
	/// </remarks>
	/// <typeparam name="T">A nullable value type.</typeparam>
	public class NullableParser<T> : ParameterParser<T?> where T : struct
	{
		public override bool TryParse(ArgumentInput input, out T? result)
		{
			result = null;
			if (!Parser.GetParser<T>().TryParse(input, out T value)) return false;
			result = value;
			return true;
		}

		public override string GetFormat() => Parser.GetParser<T>().GetFormat();

		public override string GetTypeName() => $"{Parser.GetTypeName<T>()}?";
	}
}
