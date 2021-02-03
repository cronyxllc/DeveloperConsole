using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Cronyx.Console.Editor
{
	[CustomEditor(typeof(ConsoleSettings))]
	public class SettingsEditor : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			if (GUILayout.Button("Open Settings Window", GUILayout.MinHeight(40)))
				SettingsWindow.OpenSettingsWindow();
		}
	}
}
