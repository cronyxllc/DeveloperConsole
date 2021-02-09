using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	/// <summary>
	/// Designates a switch parameter, i.e. a parameter that <b>is</b> designated using dashes (such as <c>-f</c> or <c>--foo-bar</c>).
	/// </summary>
	public class SwitchAttribute : ParameterAttribute
	{
		private static readonly Regex longNameFormatRegex = new Regex(@"[\s]+", RegexOptions.None);

		private string mName;
		private char mShortName;
		public char ShortName => mShortName;
		
		/// <summary>
		/// Gets or sets whether this switch is required.
		/// </summary>
		/// <remarks>
		/// This value is meaningless if the attached type is a <see cref="bool"/> and <see cref="Flag"/> is set to true.
		/// </remarks>
		public bool Required { get; set; }

		/// <summary>
		/// Gets or sets whether this switch represents a boolean flag.
		/// </summary>
		/// <remarks>
		/// <para>If this attribute is attached to a <see cref="bool"/> parameter, indicates that this parameter represents a flag that is true when the switch is present (i.e. "-f") and false when it is absent.
		/// Defaults to true.</para>
		/// <para>This value is meaningless if attached to any type except <see cref="bool"/>.</para>
		/// </remarks>
		public bool Flag { get; set; } = true;

		/// <summary>
		/// Gets or sets the long name for this parameter which can be specified by using double dashes (i.e. <c>--long-name</c>).
		/// </summary>
		/// <remarks>
		/// If any spaces are found within this string (leading and trailing whitespace will be ignored), they will be replaced with dashes.
		/// </remarks>
		public string LongName
		{
			get => mName;
			set
			{
				if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException(nameof(LongName));
				mName = longNameFormatRegex.Replace(value.Trim(), "-");
			}
		}

		/// <summary>
		/// Constructs a <see cref="SwitchAttribute"/>
		/// </summary>
		/// <param name="shortName">The short name of this parameter, which can be specified by using a single dash the short name (i.e. <c>-f</c> or <c>-g</c>)</param>
		public SwitchAttribute(char shortName)
		{
			if (char.IsWhiteSpace(shortName)) throw new ArgumentException(nameof(shortName));
			mShortName = shortName;
		}
	}
}
