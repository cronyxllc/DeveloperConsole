using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Disable 'Field is never asigned to, and will always have its default value null'
#pragma warning disable CS0649

namespace Cronyx.Console.UI
{
	public class ConsoleView : MonoBehaviour
	{
		[Header("Components")]
		[SerializeField] private CanvasGroup GlobalGroup;
		[SerializeField] private CanvasGroup Overlay;
		[SerializeField] private RectTransform RootEntriesTransform;
		[SerializeField] private EventScrollRect Scroll;
		[SerializeField] private TMP_InputField InputField;
		[SerializeField] private TextEntry InputFieldPrefix;
 
		[Header("Prefabs")]
		[SerializeField] private GameObject TextEntryPrefab;
		[SerializeField] private GameObject EventSystemPrefab;

		private ViewSettings mViewSettings;

		internal IEnumerable<ConsoleEntry> Entries => mEntries;
		private List<ConsoleEntry> mEntries = new List<ConsoleEntry>();

		private List<string> mInputHistory = new List<string>();
		private List<string> mInputHistoryDraft = new List<string>();
		private int mInputHistoryEdittingIndex = -1;
		private string mInputHistoryCache;

		private int mCachedCaretPosition;

		private bool mEntriesLayoutDirty;

		private const string kUIResourcesPath = "Developer Console/ConsoleView";
		private string mPrefixFormat;

		internal static ConsoleView CreateUI (Transform parent)
		{
			GameObject uiPrefab = Resources.Load<GameObject>(kUIResourcesPath);
			var instantiatedPrefab = Instantiate(uiPrefab);
			instantiatedPrefab.transform.SetParent(parent, false);
			return instantiatedPrefab.GetComponent<ConsoleView>();
		}

		private void Awake()
		{
			DeveloperConsole.OnConsoleOpened += OnConsoleOpened;
			DeveloperConsole.OnConsoleClosed += OnConsoleClosed;
			DeveloperConsole.OnDirectoryChanged += OnDirectoryChanged;

			// Check if there is currently an EventSystem active.
			// If not, the UI will not work, and we must instantiate one
			if (EventSystem.current == null)
			{
				var eventSystemInstance = Instantiate(EventSystemPrefab);
				EventSystem.current = eventSystemInstance.GetComponent<EventSystem>();
			}

			mViewSettings = ConsoleSettings.GetViewSettings();
			Overlay.alpha = ConsoleSettings.ConsoleOverlayAlpha;
			InputField.fontAsset = mViewSettings.Font;
			InputField.textComponent.color = mViewSettings.FontColor;
			InputField.pointSize = mViewSettings.FontSize;

			// Configure input field prefix visuals
			var mPrefixRightMarginSizeAdjustment = InputFieldPrefix.TextComponent.margin.z / InputFieldPrefix.TextComponent.fontSize;
			InputFieldPrefix.Configure(mViewSettings);
			InputFieldPrefix.TextComponent.margin = new Vector4(0, 0, mViewSettings.FontSize * mPrefixRightMarginSizeAdjustment, 0);

			// Get format for the console prefix
			mPrefixFormat = $"<color=#{ColorUtility.ToHtmlStringRGB(ConsoleSettings.ConsoleFilePathColor)}>{{0}}</color>";
			if (ConsoleSettings.ConsolePrefixCharacter != ' ')
				mPrefixFormat += $" <color=#{ColorUtility.ToHtmlStringRGB(ConsoleSettings.ConsoleFontColor)}>{ConsoleSettings.ConsolePrefixCharacter}</color>";

			InputField.onSubmit.AddListener(OnInputSubmitted);
			InputField.onValueChanged.AddListener(OnInputValueChanged);

			Scroll.onBeginDrag += _ => mCachedCaretPosition = InputField.caretPosition;
			Scroll.onEndDrag += _ =>
			{
				InputField.ActivateInputField();
				InputField.caretPosition = mCachedCaretPosition;
			};

			OnConsoleClosed();
		}

		internal TextEntry CreateTextEntry() => CreateEntry<TextEntry>(TextEntryPrefab);

		internal T CreateEntry<T> (GameObject entryPrefab) where T : ConsoleEntry
		{
			if (entryPrefab == null) throw new NullReferenceException($"A {nameof(ConsoleEntry)} prefab cannot be null.");

			// First instantiate the prefab and try to find a component of type T
			var entryObject = Instantiate(entryPrefab);
			var entry = entryObject.GetComponent<T>();

			// Ensure that the instantiated prefab has the console entry component
			if (entry == null)
			{
				Destroy(entryObject);
				throw new NullReferenceException($"Failed to instantiate {nameof(T)} console entry, as the supplied prefab instance did not have an attached {nameof(T)} on its root GameObject.");
			}

			// Ensure that the instantiated prefab is a UI element of some kind
			if (entryObject.GetComponent<RectTransform>() == null)
			{
				Destroy(entryObject);
				throw new NullReferenceException($"Failed to instantiate {nameof(T)} console entry, as the supplied prefab is not a UI element (the root GameObject should have a {nameof(RectTransform)} component).");
			}

			// Allow the entry to configure any visual options
			entry.Configure(mViewSettings);

			// Move the entry into the console UI
			entryObject.transform.SetParent(RootEntriesTransform, false);
			entryObject.transform.SetSiblingIndex(RootEntriesTransform.childCount - 2); // The index of the created entry should be second to last, as the final entry is the input field.
			mEntries.Insert(0, entry);

			// Check if we have created too many entries, as defined in the console settings, and delete any if necessary
			if (mEntries.Count > ConsoleSettings.MaxEntries)
			{
				var toDelete = mEntries[mEntries.Count - 1];
				mEntries.RemoveAt(mEntries.Count - 1);
				toDelete.OnRemoved();
				Destroy(toDelete.gameObject);
			}

			// Finally, call this entry's creation callback
			entry.OnCreated();

			// Mark this layout as dirty and needs rebuild
			mEntriesLayoutDirty = true;

			return entry;
		}

		private void Update()
		{
			if (mEntriesLayoutDirty)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(RootEntriesTransform);
				mEntriesLayoutDirty = false;
			}

			if (DeveloperConsole.IsOpen)
			{
				if (Input.GetKeyDown(KeyCode.UpArrow))
					CycleInputHistory(1);
				else if (Input.GetKeyDown(KeyCode.DownArrow))
					CycleInputHistory(-1);

				if (EventSystem.current.currentSelectedGameObject == null)
					InputField.ActivateInputField();
			}
		}

		private void OnInputSubmitted (string input)
		{
			// First check that some input was submitted
			// If input is only whitespace, just clear the input and do nothing
			if (string.IsNullOrWhiteSpace(input))
			{
				InputField.text = string.Empty;
				InputField.ActivateInputField();
				return;
			}

			DeveloperConsole.Log($"{InputFieldPrefix.Text} {InputField.text}");
			
			mInputHistoryCache = string.Empty;
			mInputHistory.Insert(0, input);
			if (mInputHistory.Count > ConsoleSettings.MaxInputHistory) mInputHistory.RemoveAt(mInputHistory.Count - 1);

			// Copy input into the draft array
			mInputHistoryDraft.Clear();
			mInputHistoryDraft.AddRange(mInputHistory);

			mInputHistoryEdittingIndex = -1;
			InputField.text = null;
			InputField.ActivateInputField();

			// Send the user's input to the console
			DeveloperConsole.Console.OnInputReceived(input);
		}

		private void OnInputValueChanged (string input)
		{
			// Forcibly scroll to the bottom of the log
			Scroll.verticalNormalizedPosition = 0;

			if (mInputHistoryEdittingIndex < 0) mInputHistoryCache = input;
			else mInputHistoryDraft[mInputHistoryEdittingIndex] = input;
		}

		// Call to cycle input history (i.e. with the arrow keys)
		// Pass +1 to go backward in time (less recent), -1 to go forward (more recent)
		private void CycleInputHistory (int direction)
		{
			// We can only cycle through history if we have history...
			if (mInputHistory.Count > 0)
				mInputHistoryEdittingIndex += direction;
			else return;

			if (mInputHistoryEdittingIndex < 0)
			{
				mInputHistoryEdittingIndex = -1;
				InputField.text = mInputHistoryCache;
			} else
			{
				if (mInputHistoryEdittingIndex >= mInputHistory.Count) mInputHistoryEdittingIndex = mInputHistory.Count - 1;
				InputField.text = mInputHistoryDraft[mInputHistoryEdittingIndex];
			}

			// Set caret position to the end of the current string, for simplicity
			InputField.caretPosition = InputField.text.Length;
		}

		private void OnConsoleOpened ()
		{
			GlobalGroup.alpha = 1;
			GlobalGroup.blocksRaycasts = true;
			GlobalGroup.interactable = true;

			// Select all input text on console open if desired
			if (ConsoleSettings.SelectAllOnOpen.IsEnabled() && InputField.text != null)
			{
				InputField.selectionAnchorPosition = 0;
				InputField.selectionFocusPosition = InputField.text.Length;
			}

			InputField.Select();
		}

		private void OnConsoleClosed ()
		{
			GlobalGroup.alpha = 0;
			GlobalGroup.blocksRaycasts = false;
			GlobalGroup.interactable = false;

			if (EventSystem.current != null)
				EventSystem.current.SetSelectedGameObject(null);
		}

		private void OnDirectoryChanged(string directory)
		{
			// When the console's working directory has changed, we must update the prefix
			// that displays the current working directory.

			// If the directory is the home path, or a subdirectory, the working directory should display
			// relative to that path
			if (ConsoleUtilities.TryGetRelative(DeveloperConsole.HomeDirectory, directory, out string formatted))
				formatted = Path.Combine("~", formatted);

			InputFieldPrefix.Text = string.Format(mPrefixFormat, formatted);
		}

		~ConsoleView()
		{
			DeveloperConsole.OnConsoleOpened -= OnConsoleOpened;
			DeveloperConsole.OnConsoleClosed -= OnConsoleClosed;
			DeveloperConsole.OnDirectoryChanged -= OnDirectoryChanged;
		}
	}
}
