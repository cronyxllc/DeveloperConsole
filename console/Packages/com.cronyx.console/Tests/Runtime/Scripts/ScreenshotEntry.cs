using Cronyx.Console;
using Cronyx.Console.Commands;
using Cronyx.Console.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotEntry : ConsoleEntry
{
	[SerializeField]
	private RawImage Image; // A raw image that will store the screenshot texture

	[SerializeField]
	private TextMeshProUGUI Caption; // A text component that will show a caption for the screenshot

	private Texture2D mTexture; // A texture object to store the screenshot data (so we can delete it later)

	// Use the Configure method to update this component so that it conforms to the console's visual settings,
	// such as the font, font color, or font size
	public override void Configure(ViewSettings settings)
	{
		Caption.font = settings.Font;
		Caption.fontSize = settings.FontSize;
		Caption.color = settings.FontColor;

		// Make caption's height equal to the font height
		Caption.rectTransform.sizeDelta = new Vector2(Caption.rectTransform.sizeDelta.x, settings.FontSize);
	}

	// Use the OnCreated method to handle any initialization code.
	// This method is guaranteed to be called after Configure.
	public override void OnCreated()
	{
	}

	// OnRemoved is called just before this component is destroyed.
	// Use it to perform any cleanup, release any resources, etc.
	public override void OnRemoved()
	{
		// Destroy the texture object to free up memory
		if (mTexture != null) Destroy(mTexture);
	}

	public void SetData (Texture2D texture, string caption)
	{
		mTexture = texture;
		Image.texture = mTexture;
		Caption.text = caption;
	}
}

[Command("screenshot")]
public class ScreenshotCommand : MonoBehaviour, IConsoleCommand
{
	public string Help => "Takes and displays a screenshot";

	private Coroutine mCoroutine;

	public void Invoke(string data)
	{
		if (mCoroutine != null) StopCoroutine(mCoroutine);
		mCoroutine = StartCoroutine(ShowScreenshot());
	}

	// An IEnumerator that will take and show the screenshot
	private IEnumerator ShowScreenshot()
	{
		yield return new WaitForEndOfFrame();

		// Take a screenshot (in the form of a Texture2D)
		Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		texture.Apply();

		// Get a caption for the image containing the date
		var now = DateTime.Now;
		string caption = $"Taken on {now.ToString("dddd, dd MMMM yyyy")} at {now.ToString("HH:mm:ss")}";

		// Create ScreenShot entry
		var entry = DeveloperConsole.AppendEntry<ScreenshotEntry>(Resources.Load<GameObject>("Developer Console/Tests/ScreenshotEntry"));
		entry.SetData(texture, caption);
	}
}
