using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Cronyx.Console.UI
{
	public abstract class ConsoleEntry : MonoBehaviour
	{
		/// <summary>
		/// Called when the console is setting up this <see cref="ConsoleEntry"/> before it is fully rendered. Use to update any relevant visual content within the object based on the default settings contained in <paramref name="settings"/>.
		/// </summary>
		/// <param name="settings">A <see cref="ViewSettings"/> instance passed containing a bundle of console-wide visual settings.</param>
		public virtual void Configure(ViewSettings settings) { }

		/// <summary>
		/// Called after all initialization has completed and this <see cref="ConsoleEntry"/> is visible within the console.
		/// </summary>
		public virtual void OnCreated () { }

		/// <summary>
		/// Called before the console removes this <see cref="ConsoleEntry"/> from view. Use to release any resources claimed by this object.
		/// </summary>
		/// <remarks>
		/// There is no need to call any Unity destruction methods, such as <see cref="UnityEngine.Object.Destroy(UnityEngine.Object)"/>.
		/// The console will automatically take care of destroying the GameObject(s) associated with this component.
		/// </remarks>
		public virtual void OnRemoved () { }
	}
}
