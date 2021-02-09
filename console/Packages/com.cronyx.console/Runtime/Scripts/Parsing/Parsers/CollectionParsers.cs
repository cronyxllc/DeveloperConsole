using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	/// <summary>
	/// A parser for an array of <see cref="T"/> objects.
	/// </summary>
	public class ArrayParser<T> : AliasParser<IEnumerable<T>, T[]>
	{
		public override T[] Convert(IEnumerable<T> baseValue) => baseValue.ToArray();
		public override string GetTypeName() => $"{GetTypeName<T>()}[]";
	}

	/// <summary>
	/// A parser for a <see cref="List{T}"/>.
	/// </summary>
	public class ListParser<T> : AliasParser<IEnumerable<T>, List<T>>
	{
		public override List<T> Convert(IEnumerable<T> baseValue) => baseValue.ToList();
		public override string GetTypeName() => $"List<{GetTypeName<T>()}>";
	}

	public class IListParser<T> : CovariantParser<List<T>, IList<T>> 
	{
		public override string GetTypeName() => $"IList<{GetTypeName<T>()}>";
	}

	public class IReadOnlyListParser<T> : CovariantParser<List<T>, IReadOnlyList<T>>
	{
		public override string GetTypeName() => $"IReadOnlyList<{GetTypeName<T>()}>";
	}

	public class ICollectionParaser<T> : CovariantParser<List<T>, ICollection<T>>
	{
		public override string GetTypeName() => $"ICollection<{GetTypeName<T>()}>";
	}

	public class IReadOnlyCollectionParser<T> : CovariantParser<List<T>, IReadOnlyCollection<T>>
	{
		public override string GetTypeName() => $"IReadOnlyCollection<{GetTypeName<T>()}>";
	}

	/// <summary>
	/// A parser for a <see cref="Queue{T}"/>
	/// </summary>
	/// <remarks>
	/// Elements that are parsed first will be situated at the front of the queue.
	/// For instance, if <c>[A B C]</c> was parsed, <c>A</c> would be the first element to be dequeued by calling <see cref="Queue{T}.Dequeue"/>
	/// </remarks>
	public class QueueParser<T> : AliasParser<IEnumerable<T>, Queue<T>>
	{
		public override Queue<T> Convert(IEnumerable<T> baseValue)
		{
			Queue<T> queue = new Queue<T>();
			foreach (var element in baseValue) queue.Enqueue(element);
			return queue;
		}

		public override string GetTypeName() => $"Queue<{GetTypeName<T>()}>";
	}

	/// <summary>
	/// A parser for a <see cref="Queue{T}"/>
	/// </summary>
	/// <remarks>
	/// Elements that are parsed last will be situated at the top of the stack.
	/// For instance, if <c>[A B C]</c> was parsed, <c>C</c> would be first element to be popped by calling <see cref="Stack{T}.Pop"/>
	/// </remarks>
	public class StackParser<T> : AliasParser<IEnumerable<T>, Stack<T>>
	{
		public override Stack<T> Convert(IEnumerable<T> baseValue)
		{
			Stack<T> stack = new Stack<T>();
			foreach (var element in baseValue) stack.Push(element);
			return stack;
		}

		public override string GetTypeName() => $"Stack<{GetTypeName<T>()}>";
	}

	/// <summary>
	/// A parser for a <see cref="HashSet{T}"/>
	/// </summary>
	/// <remarks>
	/// This parser does not disallow multiple of the same item to be present in the raw input. Rather, after parsing all items individually, duplicate items are removed.
	/// For instance, parsing <c>{A B C C}</c> will produce, in effect, <c>{A B C}</c>
	/// </remarks>
	public class HashSetParser<T> : ParameterParser<HashSet<T>>
	{
		private class HashSetIEnumerableParser<G> : IEnumerableParser<G>
		{
			protected override (char Beginning, char End)[] GroupingChars => new[] { ('{', '}') };
		}

		private static readonly HashSetIEnumerableParser<T> mParser = new HashSetIEnumerableParser<T>();

		public override bool TryParse(ArgumentInput input, out HashSet<T> result)
		{
			result = null;
			if (!mParser.TryParse(input, out var value)) return false;
			result = new HashSet<T>(value);
			return true;
		}

		public override string GetFormat() => $"{{{GetParser<T>().GetFormat() ?? "foo bar"} ...}}";
		public override string GetTypeName() => $"HashSet<{GetTypeName<T>()}>";
	}

	public class ISetParser<T> : CovariantParser<HashSet<T>, ISet<T>>
	{
		public override string GetTypeName() => $"ISet<{GetTypeName<T>()}>";
	}


}
