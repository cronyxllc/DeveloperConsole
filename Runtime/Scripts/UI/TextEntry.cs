using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Cronyx.Console.UI
{
	public class TextEntry : ConsoleEntry
	{
		[SerializeField] internal TextMeshProUGUI TextComponent;

		public string Text { get => TextComponent.text; set => TextComponent.text = value; }
		public Color TextColor { get => TextComponent.color; set => TextComponent.color = value; }

		public override void Configure(ViewSettings settings)
		{
			TextComponent.fontSize = settings.FontSize;
			TextComponent.font = settings.Font;
			TextColor = settings.FontColor;
		}
	}
}
