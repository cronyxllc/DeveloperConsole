using TMPro;
using UnityEngine;

namespace Cronyx.Console.UI
{
	/// <summary>
	/// A set of values that represent the current visual settings of the console, to be used by all <see cref="ConsoleEntry"/> instances.
	/// </summary>
	public class ViewSettings
	{
		/// <summary>
		/// The default font used for any console text.
		/// </summary>
		public TMP_FontAsset Font { get; internal set; }

		/// <summary>
		/// The default font size (in TextMeshPro point units) used for any console text.
		/// </summary>
		public float FontSize { get; internal set; }

		/// <summary>
		/// The default text color for any console text.
		/// </summary>
		public Color FontColor { get; internal set; }

		/// <summary>
		/// The default text color for any warning text.
		/// </summary>
		public Color WarningColor { get; internal set; }

		/// <summary>
		/// The default text color for any error text.
		/// </summary>
		public Color ErrorColor { get; internal set; }
	}
}
