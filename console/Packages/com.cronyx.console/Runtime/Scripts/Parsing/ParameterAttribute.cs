using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public abstract class ParameterAttribute : Attribute
	{
		private int mMin = -1;
		private int mMax = -1;

		private string mName;
		private string mDescription;
		private string mMetaVar;
		private Type mParser;

		public string Meta
		{
			get => mMetaVar;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentException(nameof(Meta));
				mMetaVar = value;
			}
		}

		public int Min
		{
			get => mMin;
			set
			{
				if (value < 0) throw new ArgumentException(nameof(Min));
				mMin = value;
			}
		}

		public int Max
		{
			get => mMax;
			set
			{
				if (value < 0) throw new ArgumentException(nameof(Max));
				mMax = value;
			}
		}

		public string Name
		{
			get => mName;
			set
			{
				if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(Name));
				mName = value;
			}
		}

		public string Description
		{
			get => mDescription;
			set
			{
				if (string.IsNullOrWhiteSpace(Description)) throw new ArgumentException(nameof(Description));
				mDescription = value;
			}
		}

		public Type Parser
		{
			get => mParser;
			set
			{
				if (value == null) throw new ArgumentException(nameof(Parser));
				mParser = value;
			}
		}
	}
}
