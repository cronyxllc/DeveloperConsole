using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cronyx.Console.Parsing.Parsers;

namespace Cronyx.Console.Parsing
{
	/// <summary>
	/// Base class for <see cref="PositionalAttribute"/> and <see cref="SwitchAttribute"/>. Cannot be directly applied to a parameter.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
	public abstract class ParameterAttribute : Attribute
	{
		private int mMin = -1;
		private int mMax = -1;

		private string mDescription;
		private string mMetaVar;
		private Type mParser;
		private object mDefaultValue;

		/// <summary>
		/// Gets or sets the default value of this parameter.
		/// </summary>
		/// <remarks>
		/// <para>If this property is set to a string value and the type of the parameter this attribute is applied to is not a string,
		/// the value will be parsed into an object using the default parser or the parser supplied by <see cref="Parser"/>.</para>
		/// <para>If the string representation of the object cannot be parsed, this value will be ignored.</para>
		/// <para>If this parameter is required (in the case that either <see cref="PositionalAttribute.Optional"/> is false or <see cref="SwitchAttribute.Required"/> is true),
		/// the value of this property will be meaningless.</para>
		/// </remarks>
		public object Default
		{
			get => mDefaultValue;
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(Default));
				mDefaultValue = value;
			}
		}

		/// <summary>
		/// Gets or sets a short string for this parameter that appears in usage and help text.
		/// </summary>
		public string Meta
		{
			get => mMetaVar;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentException(nameof(Meta));
				mMetaVar = value.Trim();
			}
		}

		/// <summary>
		/// If this attribute is applied to a parameter whose type is <see cref="IEnumerable{T}"/> or a subclass of it (such as <see cref="List{T}"/> or <see cref="Dictionary{TKey, TValue}"/>),
		/// gets or sets the minimum number of elements that can appear in that enumeration.
		/// </summary>
		/// <remarks>
		/// If not set, this limit is not enforced.
		/// </remarks>
		public int Min
		{
			get => mMin;
			set
			{
				if (value < 0) throw new ArgumentException(nameof(Min));
				mMin = value;
			}
		}

		/// <summary>
		/// If this attribute is applied to a parameter whose type is <see cref="IEnumerable{T}"/> or a subclass of it (such as <see cref="List{T}"/> or <see cref="Dictionary{TKey, TValue}"/>),
		/// gets or sets the maximum number of elements that can appear in that enumeration.
		/// </summary>
		/// <remarks>
		/// If not set, this limit is not enforced.
		/// </remarks>
		public int Max
		{
			get => mMax;
			set
			{
				if (value < 0) throw new ArgumentException(nameof(Max));
				mMax = value;
			}
		}

		/// <summary>
		/// Gets or sets a short description of this parameter that can appear in help text.
		/// </summary>
		public string Description
		{
			get => mDescription;
			set
			{
				if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(Description));
				mDescription = value.Trim();
			}
		}

		/// <summary>
		/// Gets or sets an optional parser type that can be used to parse this parameter.
		/// </summary>
		/// <remarks>
		/// <para>If not specified, the default parser for this parameter's type will be used. If no such parser exists, an exception will be thrown.</para>
		/// <para>
		/// If specified, the following restrictions apply to the parser type:
		/// <list type="bullet">
		/// <item>The type may not be null.</item>
		/// <item>The type must be a subclass of <see cref="ParameterParser{T}"/>.</item>
		/// <item>The type cannot be an open generic type. For instance, <c>typeof(IEnumerableParser&lt;string&gt;)</c> would be a valid parser type, but <c>typeof(<see cref="IEnumerableParser{T}"/>)</c> wouldn't.</item>
		/// <item>The type cannot represent an abstract <see cref="ParameterParser{T}"/>. That is, it must be instantiable.</item>
		/// <item>The parser specified by the type must produce a result (in its <see cref="ParameterParser{T}.TryParse(ArgumentInput, out T)"/> method) that exactly matches the parameter type this attribute is applied to, or a subclass of that type.
		/// For instance, if this attribute is applied to a parameter whose type is <c>IEnumerable&lt;string&gt;</c>, the parser must produce
		/// a <c>IEnumerableParser&lt;string&gt;</c> object or a relevant subclass, such as <c>List&lt;string&gt;</c>.</item>
		/// </list>
		/// </para>
		/// </remarks>
		public Type Parser
		{
			get => mParser;
			set
			{
				if (value == null) throw new ArgumentException(nameof(Parser));
				if (!typeof(IParameterParser).IsAssignableFrom(value)) throw new ArgumentException($"{value.Name}: Custom parser must inherit from ParameterParser.");
				if (value.ContainsGenericParameters) throw new ArgumentException($"{value.Name}: Custom parser must not contain any generic parameters.");
				mParser = value;
			}
		}
	}
}
