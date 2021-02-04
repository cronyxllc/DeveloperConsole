using Cronyx.Console.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

[assembly: InternalsVisibleTo("Cronyx.Console.Editor")]

namespace Cronyx.Console
{
	public class ConsoleSettings : ScriptableObject
	{
		/// <summary>
		/// An enum representing whether or not a feature should be enabled in the editor and/or in the build player.
		/// </summary>
		[Flags]
		public enum FeatureMode
		{
			/// <summary>
			/// This feature should never be enabled.
			/// </summary>
			Never = 0,
			/// <summary>
			/// This feature should be enabled in the editor, but not in the built player.
			/// </summary>
			EditorOnly = 1,
			/// <summary>
			/// This feature should be enabled in the built player, but not in the editor.
			/// </summary>
			PlayerOnly = 2,
			/// <summary>
			/// This feature should always be enabled.
			/// </summary>
			Always = EditorOnly | PlayerOnly
		}

		private static ConsoleSettings mSettings;

		/// <summary>
		/// Gets the current <see cref="ConsoleSettings"/> instance.
		/// </summary>
		public static ConsoleSettings Settings
		{
			get
			{
				if (mSettings != null) return mSettings;
				mSettings = FindSettings() ?? CreateSettings();
				return mSettings;
			}
		}

		/// <summary>
		/// Gets whether or not the developer console should be enabled and accessible in the built player.
		/// </summary>
		public static FeatureMode EnableConsole => Settings.mEnableConsole;
		[SerializeField] internal FeatureMode mEnableConsole = FeatureMode.Always;

		/// <summary>
		/// Gets the keycode that will be used to open the developer console.
		/// </summary>
		public static KeyCode ConsoleOpenKey => Settings.mConsoleOpenKey;
		[SerializeField] internal KeyCode mConsoleOpenKey = KeyCode.BackQuote;

		/// <summary>
		/// Gets whether or not the output of the Unity console should be mirrored in the in-game developer console.
		/// </summary>
		public static FeatureMode RedirectUnityConsoleOutput => Settings.mRedirectUnityConsoleOutput;
		[SerializeField] internal FeatureMode mRedirectUnityConsoleOutput = FeatureMode.Always;

		/// <summary>
		/// Gets whether or not the output of the in-game developer console should be mirrored in the Unity console.
		/// </summary>
		public static FeatureMode RedirectConsoleOutput => Settings.mRedirectConsoleOutput;
		[SerializeField] internal FeatureMode mRedirectConsoleOutput = FeatureMode.Never;

		/// <summary>
		/// Gets the maximum number of entries that can be stored in the console at a given time.
		/// </summary>
		public static int MaxEntries => Settings.mMaxEntries;
		[SerializeField] internal int mMaxEntries = 200;

		/// <summary>
		/// Gets the maximum number of past inputs that can be stored in the console at a given time.
		/// </summary>
		public static int MaxInputHistory => Settings.mMaxInputHistory;
		[SerializeField] internal int mMaxInputHistory = 50;

		/// <summary>
		/// Gets whether or not the game should be paused when the console is opened by setting <see cref="Time.timeScale"/> to <c>0</c>.
		/// </summary>
		public static FeatureMode PauseOnOpen => Settings.mPauseOnOpen;
		[SerializeField] internal FeatureMode mPauseOnOpen = FeatureMode.Always;

		/// <summary>
		/// Gets the TextMeshPro font asset that will be used to render the output of the console.
		/// </summary>
		public static TMP_FontAsset ConsoleFont => Settings.mConsoleFont;
		[SerializeField] internal TMP_FontAsset mConsoleFont;

		/// <summary>
		/// Gets the font size that will be used to render the output of the console.
		/// </summary>
		public static float ConsoleFontSize => Settings.mConsoleFontSize;
		[SerializeField] internal float mConsoleFontSize = 15;

		/// <summary>
		/// Gets the font color that will be used to render the output of the console. 
		/// </summary>
		public static Color ConsoleFontColor => Settings.mConsoleFontColor;
		[SerializeField] internal Color mConsoleFontColor = Color.white;

		/// <summary>
		/// Gets the font color that will be used to render any warnings in the console.
		/// </summary>
		public static Color ConsoleFontWarningColor => Settings.mConsoleFontWarningColor;
		[SerializeField] internal Color mConsoleFontWarningColor = new Color(222 / 255f, 172 / 255f, 73 / 255f);

		/// <summary>
		/// Gets the font color that will be used to render any errors in the console.
		/// </summary>
		public static Color ConsoleFontErrorColor => Settings.mConsoleFontErrorColor;
		[SerializeField] internal Color mConsoleFontErrorColor = new Color(204 / 255f, 82 / 255f, 84 / 255f);

		/// <summary>
		/// Gets the font color that will be used to render the console filepath.
		/// </summary>
		public static Color ConsoleFilePathColor => Settings.mConsoleFilePathColor;
		[SerializeField] internal Color mConsoleFilePathColor = new Color(171 / 255f, 205 / 255f, 173 / 255f);

		/// <summary>
		/// Gets the default alpha of the overlay that appears when opening the console.
		/// </summary>
		public static float ConsoleOverlayAlpha => Settings.mConsoleOverlayAlpha;
		[SerializeField] internal float mConsoleOverlayAlpha = 0.3f;

		/// <summary>
		/// Gets the character representing the console prefix.
		/// </summary>
		public static char ConsolePrefixCharacter => Settings.mConsolePrefixCharacter;
		[SerializeField] internal char mConsolePrefixCharacter = 'λ';

		/// <summary>
		/// If true, all text in the console's input field will be selected when the console opens.
		/// </summary>
		public static FeatureMode SelectAllOnOpen => Settings.mSelectAllOnOpen;
		[SerializeField] internal FeatureMode mSelectAllOnOpen = FeatureMode.Always;

		internal void OnValidate()
		{
			if (mMaxEntries < 1) mMaxEntries = 1;
			if (mMaxInputHistory < 1) mMaxInputHistory = 1;
		}

		internal static ConsoleSettings FindSettings()
		{
			var instances = Resources.LoadAll<ConsoleSettings>(string.Empty);

#if UNITY_EDITOR
			if (instances != null && instances.Length > 1)
				Logger.Warn($"Multiple {nameof(ConsoleSettings)} resources were found." +
					$" There should be at most one {nameof(ConsoleSettings)} per project." +
					$" Using the first {nameof(ConsoleSettings)} found at {UnityEditor.AssetDatabase.GetAssetPath(instances[0].GetInstanceID())}");
#endif
			if (instances == null || instances.Length == 0) return null;
			else return instances[0];
		}

		/// <summary>
		/// Creates and returns a new <see cref="ConsoleSettings"/> instance and initializes it with any default values.
		/// </summary>
		/// <returns></returns>
		internal static ConsoleSettings CreateSettings ()
		{
			var settings = CreateInstance<ConsoleSettings>();

			const string defaultFontAssetResourcesPath = "Developer Console/CourierPrimeAsset";
			settings.mConsoleFont = Resources.Load<TMP_FontAsset>(defaultFontAssetResourcesPath);
			return settings;
		}

		internal static ViewSettings GetViewSettings() => new ViewSettings()
		{
			Font = Settings.mConsoleFont,
			FontSize = Settings.mConsoleFontSize,
			FontColor = Settings.mConsoleFontColor,
			ErrorColor = Settings.mConsoleFontErrorColor,
			WarningColor = Settings.mConsoleFontWarningColor
		};
	}

	public static class SettingsExtensions
	{
		public static bool IsEnabled(this ConsoleSettings.FeatureMode mode)
		{
#if UNITY_EDITOR
			if (!mode.HasFlag(ConsoleSettings.FeatureMode.EditorOnly)) return false;
#elif UNITY_STANDALONE
			if (!mode.HasFlag(FeatureMode.InPlayer)) return false;
#endif
			return true;
		}
	}
}
