using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	public class SwitchAttribute : ParameterAttribute
	{
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
		/// If this attribute is attached to a <see cref="bool"/> parameter, indicates that this parameter represents a flag that is true when the switch is present (i.e. "-f") and false when it is absent.
		/// Defaults to true.
		/// </remarks>
		public bool Flag { get; set; } = true;

		public SwitchAttribute(char shortName)
		{
			mShortName = shortName;
		}
	}
}
