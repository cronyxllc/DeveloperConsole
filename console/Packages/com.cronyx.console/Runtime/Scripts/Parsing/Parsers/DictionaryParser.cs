using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	/// <summary>
	/// A parser for a <see cref="Dictionary{TKey, TValue}"/>.
	/// </summary>
	/// <remarks>
	/// If this parser finds any duplicate key names in the input, it will treat that input as malformed.
	/// </remarks>
	public class DictionaryParser<TKey, TValue> : ParameterParser<Dictionary<TKey, TValue>>
	{
		private class KVIEnumerableParser : IEnumerableParser<KeyValuePair<TKey, TValue>>
		{
			protected override (char Beginning, char End)[] GroupingChars => new[] { ('{', '}') };
		}

		private static readonly KVIEnumerableParser mParser = new KVIEnumerableParser();

		public override bool TryParse(ArgumentInput input, out Dictionary<TKey, TValue> result)
		{
			result = null;
			if (!mParser.TryParse(input, out var value)) return false;

			// Take the IEnumerable<KeyValuePair<TKey, TValue>> and convert it to a dictionary,
			// ensuring that no duplicate key names were used
			Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
			foreach (var pair in value)
			{
				if (dict.ContainsKey(pair.Key)) return false; // Duplicate key name
				dict[pair.Key] = pair.Value;
			}

			result = dict;

			return true;
		}

		public override string GetFormat() => $"{{{GetParser<KeyValuePair<TKey, TValue>>().GetFormat()} ...}}";
		public override string GetTypeName() => $"Dictionary<{GetTypeName<TKey>()},{GetTypeName<TValue>()}>";
	}

	public class IDictionaryParser<TKey, TValue> : CovariantParser<Dictionary<TKey, TValue>, IDictionary<TKey, TValue>>
	{
		public override string GetTypeName() => $"IDictionary<{GetTypeName<TKey>()},{GetTypeName<TValue>()}>";
	}

	public class IReadOnlyDictionaryParser<TKey, TValue> : CovariantParser<Dictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>>
	{
		public override string GetTypeName() => $"IReadOnlyDictionary<{GetTypeName<TKey>()},{GetTypeName<TValue>()}>";
	}
}
