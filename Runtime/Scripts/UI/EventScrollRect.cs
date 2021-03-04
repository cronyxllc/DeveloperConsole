using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cronyx.Console.UI
{
	/// <summary>
	/// <see cref="ScrollRect"/> wrapper with support for various callbacks.
	/// </summary>
	internal class EventScrollRect : ScrollRect
	{
		public event Action<PointerEventData> onInitializePotentialDrag;
		public event Action<PointerEventData> onBeginDrag;
		public event Action<PointerEventData> onDrag;
		public event Action<PointerEventData> onEndDrag;
		public event Action<PointerEventData> onScroll;

		public override void OnInitializePotentialDrag(PointerEventData eventData)
		{
			base.OnInitializePotentialDrag(eventData);
			onInitializePotentialDrag?.Invoke(eventData);
		}

		public override void OnBeginDrag(PointerEventData eventData)
		{
			base.OnBeginDrag(eventData);
			onBeginDrag?.Invoke(eventData);
		}

		public override void OnDrag(PointerEventData eventData)
		{
			base.OnDrag(eventData);
			onDrag?.Invoke(eventData);
		}

		public override void OnEndDrag(PointerEventData eventData)
		{
			base.OnEndDrag(eventData);
			onEndDrag?.Invoke(eventData);
		}

		public override void OnScroll(PointerEventData data)
		{
			base.OnScroll(data);
			onScroll?.Invoke(data);
		}
	}
}
