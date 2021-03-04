using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using TMPro;

namespace Cronyx.Console.Editor
{
	internal class SettingsWindow : EditorWindow
	{
		private static class Styles
		{
			public static readonly GUIContent tmpResourcesNotFound = new GUIContent("It looks like the TextMeshPro Essential Resources have not been imported. " +
				"Please import them by going to Window > TextMeshPro > Import TMP Essential Resources, or by clicking 'Import TMP Essentials' in the dialog that just opened.");

			public static readonly GUIContent titleGeneral = new GUIContent("General Settings", "Settings that control the general function of the developer console.");
			public static readonly GUIContent settingEnabled = new GUIContent("Enabled", "Determines when to enable the in-game developer console.");
			public static readonly GUIContent settingKeyToOpen = new GUIContent("Open Key", "The default key that opens the in-game developer console.");
			public static readonly GUIContent settingRedirectUnityOutput = new GUIContent("Redirect Unity Output", "Determines whether the output of the Unity console should be mirrored in the in-game console.");
			public static readonly GUIContent settingRedirectOutput = new GUIContent("Log Console Output", "Determines whether the output of the in-game console should be logged to the Unity console.");
			public static readonly GUIContent settingSelectAllOnOpen = new GUIContent("Select All On Open", "If true, all inputted text will be selected when the console is opened.");
			public static readonly GUIContent settingMaxEntries = new GUIContent("Max Entries", "The maximum number of entries that can be stored in the console at any given time.");
			public static readonly GUIContent settingMaxInputHistory = new GUIContent("Max Input History", "The maximum number of past inputs that can be stored at a given time.");
			public static readonly GUIContent settingPauseOnOpen = new GUIContent("Pause Game", "Determines whether the game should be paused when opening the console by setting Time.timeScale to zero.");
			public static readonly GUIContent settingHomeDirectoryMode = new GUIContent("Home Directory", "Determines what the default home directory path should be.");
			public static readonly GUIContent settingHomeDirectoryPath = new GUIContent("Home Directory Path", "The path to the custom home directory. Environment variables like %HOMEPATH% will be expanded.");
			public static readonly GUIContent buttonHomeDirectoryBrowse = EditorGUIUtility.IconContent("d_Folder Icon", "Browse to a custom home directory path");

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

		private bool mTMPResourcesFound;

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
		private SerializedObject mTarget;
		private SerializedObject Target
		{
			get
			{
				if (mTarget != null) return mTarget;
				mTarget = new SerializedObject(Settings);
				return mTarget;
			}
		}

		private void OnEnable()
		{
			if (TMP_Settings.instance == null)
			{
				mTMPResourcesFound = false;
				TMPro_EventManager.RESOURCE_LOAD_EVENT.Add(() => mTMPResourcesFound = true);
			}
			else mTMPResourcesFound = true;
		}

		private void OnGUI()
		{
			if (!mTMPResourcesFound) EditorGUILayout.HelpBox(Styles.tmpResourcesNotFound.text, MessageType.Error);

			// Disable settings if TMP resources have not been loaded
			using (new EditorGUI.DisabledGroupScope(!mTMPResourcesFound))
			{
				Target.Update();

				using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
				{
					BasicSettings();
				}

				Settings.OnValidate();

				if (Target.ApplyModifiedProperties())
					EditorUtility.SetDirty(Settings);
			}
		}

		private void DoProp(string name, GUIContent label) => DoProp(GetProp(name), label);
		private void DoProp(SerializedProperty prop, GUIContent label) => EditorGUILayout.PropertyField(prop, label);
		private SerializedProperty GetProp(string name) => Target.FindProperty(name);

		private void BasicSettings ()
		{
			GUILayout.Label(Styles.titleGeneral, EditorStyles.boldLabel);
			EditorGUI.indentLevel = 1;

			DoProp(nameof(ConsoleSettings.mEnableConsole), Styles.settingEnabled);
			DoProp(nameof(ConsoleSettings.mConsoleOpenKey), Styles.settingKeyToOpen);
			DoProp(nameof(ConsoleSettings.mLogConsoleOutput), Styles.settingRedirectOutput);
			DoProp(nameof(ConsoleSettings.mRedirectUnityConsoleOutput), Styles.settingRedirectUnityOutput);
			DoProp(nameof(ConsoleSettings.mSelectAllOnOpen), Styles.settingSelectAllOnOpen);
			DoProp(nameof(ConsoleSettings.mMaxEntries), Styles.settingMaxEntries);
			DoProp(nameof(ConsoleSettings.mMaxInputHistory), Styles.settingMaxInputHistory);
			DoProp(nameof(ConsoleSettings.mPauseOnOpen), Styles.settingPauseOnOpen);
			DoProp(nameof(ConsoleSettings.mHomeDirectoryMode), Styles.settingHomeDirectoryMode);

			if (Settings.mHomeDirectoryMode == ConsoleSettings.HomeDirectoryType.Custom)
				using (new EditorGUILayout.HorizontalScope())
				{
					var homeDirPathProp = GetProp(nameof(ConsoleSettings.mCustomHomeDirectory));
					DoProp(homeDirPathProp, Styles.settingHomeDirectoryPath);

					if (GUILayout.Button(Styles.buttonHomeDirectoryBrowse, EditorStyles.miniButton, GUILayout.MaxWidth(27)))
					{
						var currentDirectory = homeDirPathProp.stringValue;
						var chosenPath = EditorUtility.OpenFolderPanel("Browse to Console Home Directory", Directory.Exists(currentDirectory) ? currentDirectory : Application.persistentDataPath, null);
						if (!string.IsNullOrWhiteSpace(chosenPath)) homeDirPathProp.stringValue = chosenPath;
					}
				}

			EditorGUI.indentLevel = 0;

			EditorGUILayout.Space();

			GUILayout.Label(Styles.titleVisual, EditorStyles.boldLabel);
			EditorGUI.indentLevel = 1;

			DoProp(nameof(ConsoleSettings.mConsoleFont), Styles.settingFontAsset);
			EditorGUILayout.Slider(GetProp(nameof(ConsoleSettings.mConsoleFontSize)), 8, 100, Styles.settingFontSize);
			EditorGUILayout.Slider(GetProp(nameof(ConsoleSettings.mConsoleOverlayAlpha)), 0, 1, Styles.settingOverlayAlpha);
			DoProp(nameof(ConsoleSettings.mConsoleFontColor), Styles.settingFontColor);
			DoProp(nameof(ConsoleSettings.mConsoleFontWarningColor), Styles.settingFontWarningColor);
			DoProp(nameof(ConsoleSettings.mConsoleFontErrorColor), Styles.settingFontErrorColor);
			DoProp(nameof(ConsoleSettings.mConsoleFilePathColor), Styles.settingFilePathColor);
			DoProp(nameof(ConsoleSettings.mConsolePrefixCharacter), Styles.settingPrefixCharacter);

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
