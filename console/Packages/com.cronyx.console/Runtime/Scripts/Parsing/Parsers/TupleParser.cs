using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing.Parsers
{
	/// <summary>
	/// Parses a tuple type with an arbitrary number of parameters using reflection.
	/// </summary>
	/// <typeparam name="T">A tuple type with an arbitrary number of generic arguments</typeparam>
	internal class TupleParser<T> : CompoundParser<T>
	{
		protected override T GetResult(object[] elements)
		{
			Type tupleType = null;
			if (TupleParser.IsTupleType(typeof(T))) tupleType = TupleParser.GetTupleType(GetTypes().ToArray());
			else if (TupleParser.IsValueTupleType(typeof(T))) tupleType = TupleParser.GetValueTupleType(GetTypes().ToArray());
			return (T) Activator.CreateInstance(tupleType, elements);
		}

		protected override IEnumerable<Type> GetTypes() => typeof(T).GetGenericArguments();

		public override string GetTypeName()
		{
			// Get type name for this tuple.
			// Type name will be different depending on whether this parser is parsing for a ValueTuple<> or a Tuple<>

			if (TupleParser.IsTupleType(typeof(T)))
			{
				// Return "Tuple<...>," filling in the type arguments
				StringBuilder sb = new StringBuilder("Tuple");
				if (Types.Count > 0)
				{
					sb.Append('<');
					for (int i = 0; i < Types.Count; i++)
					{
						sb.Append(Parser.GetParser(Types[i]).GetTypeName());
						if (i != Types.Count - 1) sb.Append(",");
					}
					sb.Append('>');
				}
				return sb.ToString();
			} else
			{
				// Return "(...)," filling in the type arguments
				StringBuilder sb = new StringBuilder("(");

				for (int i = 0; i < Types.Count; i++)
				{
					sb.Append(Parser.GetParser(Types[i]).GetTypeName());
					if (i != Types.Count - 1) sb.Append(",");
				}

				sb.Append(')');
				return sb.ToString();
			}
		}
	}

	internal static class TupleParser
	{
		private static readonly Dictionary<int, Func<Type[], Type>> mCreateValueTupleType = new Dictionary<int, Func<Type[], Type>>()
		{
			[0] = types => typeof(ValueTuple),
			[1] = types => typeof(ValueTuple<>).MakeGenericType(types),
			[2] = types => typeof(ValueTuple<,>).MakeGenericType(types),
			[3] = types => typeof(ValueTuple<,,>).MakeGenericType(types),
			[4] = types => typeof(ValueTuple<,,,>).MakeGenericType(types),
			[5] = types => typeof(ValueTuple<,,,,>).MakeGenericType(types),
			[6] = types => typeof(ValueTuple<,,,,,>).MakeGenericType(types),
			[7] = types => typeof(ValueTuple<,,,,,,>).MakeGenericType(types),
			[8] = types => typeof(ValueTuple<,,,,,,,>).MakeGenericType(types)
		};

		private static readonly Dictionary<int, Func<Type[], Type>> mCreateTupleType = new Dictionary<int, Func<Type[], Type>>()
		{
			[0] = types => typeof(Tuple),
			[1] = types => typeof(Tuple<>).MakeGenericType(types),
			[2] = types => typeof(Tuple<,>).MakeGenericType(types),
			[3] = types => typeof(Tuple<,,>).MakeGenericType(types),
			[4] = types => typeof(Tuple<,,,>).MakeGenericType(types),
			[5] = types => typeof(Tuple<,,,,>).MakeGenericType(types),
			[6] = types => typeof(Tuple<,,,,,>).MakeGenericType(types),
			[7] = types => typeof(Tuple<,,,,,,>).MakeGenericType(types),
			[8] = types => typeof(Tuple<,,,,,,,>).MakeGenericType(types)
		};

		public static Type GetValueTupleType(Type[] types) => mCreateValueTupleType[types.Length](types);
		public static Type GetTupleType(Type[] types) => mCreateTupleType[types.Length](types);

		// Readonly sets containing the Tuple Types
		private static readonly ISet<Type> tupleTypes = new HashSet<Type>()
		{
			typeof(Tuple), typeof(Tuple<>), typeof(Tuple<,>),
			typeof(Tuple<,,>), typeof(Tuple<,,,>), typeof(Tuple<,,,,>),
			typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>), typeof(Tuple<,,,,,,,>),
		};

		private static readonly ISet<Type> valueTupleTypes = new HashSet<Type>()
		{
			typeof(ValueTuple), typeof(ValueTuple<>), typeof(ValueTuple<,>),
			typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>), typeof(ValueTuple<,,,,>),
			typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>), typeof(ValueTuple<,,,,,,,>),
		};

		public static bool IsTupleType(Type type)
		{
			if (type == null || !type.IsGenericType) return false;
			return tupleTypes.Contains(type.GetGenericTypeDefinition());
		}

		public static bool IsValueTupleType(Type type)
		{
			if (type == null || !type.IsGenericType) return false;
			return valueTupleTypes.Contains(type.GetGenericTypeDefinition());
		}
	}
}
