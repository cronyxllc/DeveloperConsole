using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cronyx.Console.Parsing
{
	/// <summary>
	/// Designates a positional parameter, i.e. a parameter that is <b>not</b> designated using dashes (such as <c>-f</c> or <c>--foo-bar</c>).
	/// </summary>
	public class PositionalAttribute : ParameterAttribute
	{
		/// <summary>
		/// Gets or sets a value indicating whether or not this positional parameter is optional.
		/// </summary>
		/// <remarks>
		/// Positional parameters that are marked as optional will be sorted last, and will come after any required (non-optional) positional parameters.
		/// </remarks>
		public bool Optional { get; set; }
	}
}
