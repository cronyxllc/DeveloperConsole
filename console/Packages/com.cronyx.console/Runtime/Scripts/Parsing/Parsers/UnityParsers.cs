using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Cronyx.Console.Parsing.Parsers
{
	public class Vector2Parser : CompoundParser<float, float, Vector2>
	{
		protected override Vector2 GetResult(float t0, float t1) => new Vector2(t0, t1);

		public override string GetFormat() => $"{GroupingChars[0].Beginning}x y{GroupingChars[0].End}";
	}
	
	public class Vector2IntParser : CompoundParser<int, int, Vector2Int>
	{
		protected override Vector2Int GetResult(int t0, int t1) => new Vector2Int(t0, t1);

		public override string GetFormat() => $"{GroupingChars[0].Beginning}x y{GroupingChars[0].End}";
	}

	public class Vector3Parser : CompoundParser<float, float, float, Vector3>
	{
		protected override Vector3 GetResult(float t0, float t1, float t2=0.0f) => new Vector3(t0, t1, t2);

		public override string GetFormat() => $"{GroupingChars[0].Beginning}x y z{GroupingChars[0].End}";
	}

	public class Vector3IntParser : CompoundParser<int, int, int, Vector3Int>
	{
		protected override Vector3Int GetResult(int t0, int t1, int t2=0) => new Vector3Int(t0, t1, t2);

		public override string GetFormat() => $"{GroupingChars[0].Beginning}x y z{GroupingChars[0].End}";
	}

	public class Vector4Parser : CompoundParser<float, float, float, float, Vector4>
	{
		protected override Vector4 GetResult(float t0, float t1, float t2=0, float t3=0) => new Vector4(t0, t1, t2, t3);

		public override string GetFormat() => $"{GroupingChars[0].Beginning}x y z w{GroupingChars[0].End}";
	}

	public class QuaternionParser : CompoundParser<float, float, float, float, Quaternion>
	{
		protected override Quaternion GetResult(float t0, float t1, float t2, float t3) => new Quaternion(t0, t1, t2, t3);

		public override string GetFormat() => $"{GroupingChars[0].Beginning}x y z w{GroupingChars[0].End}";
	}

	public class ColorParser : CompoundParser<float, float, float, float, Color>
	{
		protected override Color GetResult(float t0, float t1, float t2, float t3 = 1)
		{
			if (t0 < 0 || t0 > 1) throw new ArgumentException("r");
			if (t1 < 0 || t1 > 1) throw new ArgumentException("g");
			if (t2 < 0 || t2 > 1) throw new ArgumentException("b");
			if (t3 < 0 || t3 > 1) throw new ArgumentException("a");
			return new Color(t0, t1, t2, t3);
		}

		public override string GetFormat() => $"{GroupingChars[0].Beginning}r g b a{GroupingChars[0].End}";
	}

	public class Color32Parser : CompoundParser<byte, byte, byte, byte, Color32>
	{
		protected override Color32 GetResult(byte t0, byte t1, byte t2, byte t3=255) => new Color32(t0, t1, t2, t3);

		public override string GetFormat() => $"{GroupingChars[0].Beginning}r g b a{GroupingChars[0].End}";
	}
}
