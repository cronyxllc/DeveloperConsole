using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace Cronyx.Console.Editor
{
	internal class SettingsWindow : EditorWindow
	{
		private static class Styles
		{
			public static readonly GUIContent titleGeneral = new GUIContent("General Settings", "Settings that control the general function of the developer console.");
			public static readonly GUIContent settingEnabled = new GUIContent("Enabled", "Determines when to enable the in-game developer console.");
			public static readonly GUIContent settingKeyToOpen = new GUIContent("Open Key", "The default key that opens the in-game developer console.");
			public static readonly GUIContent settingRedirectUnityOutput = new GUIContent("Redirect Unity Output", "Determines whether the output of the Unity console should be mirrored in the in-game console.");
			public static readonly GUIContent settingRedirectOutput = new GUIContent("Log Console Output", "Determines whether the output of the in-game console should be logged to the Unity console.");
			public static readonly GUIContent settingSelectAllOnOpen = new GUIContent("Select All On Open", "If true, all inputted text will be selected when the console is opened.");
			public static readonly GUIContent settingMaxEntries = new GUIContent("Max Entries", "The maximum number of entries that can be stored in the console at any given time.");
			public static readonly GUIContent settingMaxInputHistory = new GUIContent("Max Input History", "The maximum number of past inputs that can be stored at a given time.");
			public static readonly GUIContent settingPauseOnOpen = new GUIContent("Pause Game", "Determines whether the game should be paused when opening the console by setting Time.timeScale to zero.");

			public static readonly GUIContent titleVisual = new GUIContent("Visual Settings", "Settings that control the appearance of the developer console.");
			public static readonly GUIContent settingFontAsset = new GUIContent("Font", "The TextMeshPro font asset that the console uses.");
			public static readonly GUIContent settingFontSize = new GUIContent("Font Size", "The size that the console font will be rendered.");
			public static readonly GUIContent settingOverlayAlpha = new GUIContent("Overlay Alpha", "The alpha (transparency) of the console overlay. (0 is invisible and 1 is black).");
			public static readonly GUIContent settingFontColor = new GUIContent("Font Color", "The console's text color.");
			public static readonly GUIContent settingFontWarningColor = new GUIContent("Warning Color", "The console's warning text color.");
			public static readonly GUIContent settingFontErrorColor = new GUIContent("Error Color", "The console's error text color.");
			public static readonly GUIContent settingFilePathColor = new GUIContent("Path Color", "The color of text indicating the current file path.");
			public static readonly GUIContent settingPrefixCharacter = new GUIContent("Prefix Character", "The prefix character used within the console.");
		}

		[MenuItem("Window/Developer Console/Settings")]
		internal static void OpenSettingsWindow() => GetWindow<SettingsWindow>("Developer Console Settings");

		private static readonly string kSettingsPath = Path.Combine("Assets", "Developer Console", "Resources", "ConsoleSettings.asset");
		private static readonly string kSettingsContainerPath = Path.GetDirectoryName(kSettingsPath);

		private ConsoleSettings mSettings;
		private ConsoleSettings Settings
		{
			get
			{
				if (mSettings != null) return mSettings;
				mSettings = GetOrCreateSettings();
				return mSettings;
			}
		}
		private SerializedObject mSerializedObject;
		private SerializedObject SerializedObject
		{
			get
			{
				if (mSerializedObject != null) return mSerializedObject;
				mSerializedObject = new SerializedObject(Settings);
				return mSerializedObject;
			}
		}

		private void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
			{
				BasicSettings();
			}

			Settings.OnValidate();
			EditorUtility.SetDirty(Settings);
		}

		private void BasicSettings ()
		{
			GUILayout.Label(Styles.titleGeneral, EditorStyles.boldLabel);
			EditorGUI.indentLevel = 1;
			//EditorGUILayout.PropertyField(SerializedObject.FindProperty(nameof(Settings.mEnableConsole)), Styles.settingEnabled);
			Settings.mEnableConsole = FeatureField(Styles.settingEnabled, Settings.mEnableConsole);
			Settings.mConsoleOpenKey = (KeyCode) EditorGUILayout.EnumPopup(Styles.settingKeyToOpen, Settings.mConsoleOpenKey);
			Settings.mRedirectConsoleOutput = FeatureField(Styles.settingRedirectOutput, Settings.mRedirectConsoleOutput);
			Settings.mRedirectUnityConsoleOutput = FeatureField(Styles.settingRedirectUnityOutput, Settings.mRedirectUnityConsoleOutput);
			Settings.mSelectAllOnOpen = FeatureField(Styles.settingSelectAllOnOpen, Settings.mSelectAllOnOpen);
			Settings.mMaxEntries = EditorGUILayout.IntField(Styles.settingMaxEntries, Settings.mMaxEntries);
			Settings.mMaxInputHistory = EditorGUILayout.IntField(Styles.settingMaxInputHistory, Settings.mMaxInputHistory);
			Settings.mPauseOnOpen = FeatureField(Styles.settingPauseOnOpen, Settings.mPauseOnOpen);
			EditorGUI.indentLevel = 0;

			EditorGUILayout.Space();

			GUILayout.Label(Styles.titleVisual, EditorStyles.boldLabel);
			EditorGUI.indentLevel = 1;
			Settings.mConsoleFont = (TMPro.TMP_FontAsset) EditorGUILayout.ObjectField(Styles.settingFontAsset, Settings.mConsoleFont, typeof(TMPro.TMP_FontAsset), false);
			Settings.mConsoleFontSize = EditorGUILayout.Slider(Styles.settingFontSize, Settings.mConsoleFontSize, 8, 100);
			Settings.mConsoleOverlayAlpha = EditorGUILayout.Slider(Styles.settingOverlayAlpha, Settings.mConsoleOverlayAlpha, 0, 1);
			Settings.mConsoleFontColor = EditorGUILayout.ColorField(Styles.settingFontColor, Settings.mConsoleFontColor);
			Settings.mConsoleFontWarningColor = EditorGUILayout.ColorField(Styles.settingFontWarningColor, Settings.mConsoleFontWarningColor);
			Settings.mConsoleFontErrorColor = EditorGUILayout.ColorField(Styles.settingFontErrorColor, Settings.mConsoleFontErrorColor);
			Settings.mConsoleFilePathColor = EditorGUILayout.ColorField(Styles.settingFilePathColor, Settings.mConsoleFilePathColor);

			var prefixChar = EditorGUILayout.TextField(Styles.settingPrefixCharacter, Settings.mConsolePrefixCharacter.ToString());
			if (string.IsNullOrWhiteSpace(prefixChar)) Settings.mConsolePrefixCharacter = ' ';
			else Settings.mConsolePrefixCharacter = prefixChar.ToCharArray()[0];

			EditorGUI.indentLevel = 0;
		}

		private ConsoleSettings.FeatureMode FeatureField (GUIContent label, ConsoleSettings.FeatureMode selected)
			=> (ConsoleSettings.FeatureMode)EditorGUILayout.EnumPopup(label, selected);

		// Finds a valid ConsoleSettings instance, or, if none exist, creates one and returns it.
		private ConsoleSettings GetOrCreateSettings ()
		{
			var settings = ConsoleSettings.FindSettings();

			if (settings == null)
			{
				// No settings object was found in project, so let's create one in a specified path
				settings = ConsoleSettings.CreateSettings();

				// Ensure parent directory of asset exists
				if (!Directory.Exists(kSettingsContainerPath)) Directory.CreateDirectory(kSettingsContainerPath);
				
				AssetDatabase.CreateAsset(settings, kSettingsPath);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			return settings;
		}
	}
}
