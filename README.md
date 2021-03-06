<p align="center"><img src="https://raw.githubusercontent.com/cronyxllc/DeveloperConsole/main/docs/images/Screenshot.PNG"></p>

# DeveloperConsole

[![Release](https://img.shields.io/github/v/release/cronyxllc/DeveloperConsole)](https://github.com/cronyxllc/DeveloperConsole/releases)
[![openupm](https://img.shields.io/npm/v/com.cronyx.console?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.cronyx.console/) [![Release](https://github.com/cronyxllc/DeveloperConsole/actions/workflows/release.yml/badge.svg)](https://github.com/cronyxllc/DeveloperConsole/actions/workflows/release.yml)

A lightweight, in-game developer console for Unity

## Features

<ul>
  <li>Easy to use and extendable in-game developer console!</li>
  <li>
    <p>Quickly add new commands by marking a method with <code>[Command]</code>:</p>
    <pre lang="csharp">
[Command("cube")]
public static void CreateCube (Vector3 pos, float scale)
{
  var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
  cube.transform.position = pos;
  cube.transform.localScale = Vector3.one * scale;
}
&nbsp;
// $ cube (1 2 3) 2
// Creates a cube at (1, 2, 3) with scale 2.0f
</pre>
  </li>
  <li>
    <p>Parsing support for all basic C# types, <code>IEnumerable&lt;T&gt;</code>, <code>List&lt;T&gt;</code>, <code>Dictionary&lt;TKey, TValue&gt;</code>, and <a href="https://github.com/cronyxllc/DeveloperConsole/wiki/Supported-parameter-types">many more</a>.</p>
  </li>
  
  <li>
  <p>Fully customizable <code>GetOpt</code> command syntax and automatic help text generation:</p>
  
  <pre lang=csharp>
[Command("showcase", Description = "Showcases the flexibility of the command parser.")]
public static void CustomCommand(
  [Positional(Description = "The first positional", Meta = "FOO")] Vector3 posOne,
  [Positional(Description = "The second positional", Max = 4, Meta = "BAR", Optional = true)] IEnumerable&lt;string&gt; posTwo,
  [Switch('a', LongName = "flag", Description = "A flag")] bool flag,
  [Switch('b', LongName = "switch", Description = "A switch", Meta = "STR")] string @switch)
{
  // Command implementation
}

// Automatically generated help text:
// $ help showcase
// showcase: Showcases the flexibility of the command parser
// 
// usage: showcase FOO [BAR] [-a] [-b STR]
// Format:
//     FOO            Vector3                (x y z)
//     BAR            IEnumerable<string>    [foo bar ...]
//     STR            string                 
// 
// Mandatory Parameters:
//     FOO            The first positional
// 
// Optional Parameters:
//     BAR            The second positional
//                    Can have at most 4 elements
//     -a, --flag     A flag
//     -b, --switch   A switch
</pre>
  </li>
  
  <li>
    <p>Seamless parsing support for nested generic types, such as <code>List&lt;List&lt;T&gt;&gt;</code>.</p>
  </li>
  <li>
    <p>Define parsers for custom types by extending the <code>ParameterParser</code>.</p>
  </li>
  <li>
    <p>Add custom widgets, images, and media by extending the <code>ConsoleEntry</code> class.</p>
  </li>
  <li>
  <p>Implement custom command line parsing through the <code>IConsoleCommand</code> interface.</p>
  <pre lang="csharp">
[Command("cmd")]
public class MyCommand : IConsoleCommand
{
  public void Invoke(string data)
  {
    // Parse command line input passed to this command
  }
}
</pre>
  </li>
  <li>
  Access filesystem through built-in commands like <code>ls</code> and <code>pwd</code>.
  </li>
  <li>
  <p>A detailed documentation of all of these features and more over at <a href="https://github.com/cronyxllc/DeveloperConsole/wiki">the wiki</a>!</p>
  </li>
</ul>

## Installation

### Prerequisites

1. Unity `2020.2` or greater
2. TextMeshPro package `3.0.1` or greater installed in your project. Comes built-in with Unity `2020.2` or greater.

### Installation Guides

<details>
  <summary><b>Via PackageInstaller (drag-and-drop)</b></summary>

1. Download the [installer `.unitypackage`](https://package-installer.glitch.me/v1/installer/OpenUPM/com.cronyx.console?registry=https%3A%2F%2Fpackage.openupm.com) to your machine.
2. Import the `.unitypackage` by dragging and dropping it onto the Unity window or by going to <kbd>Assets > Import Package > Custom Package...</kbd> and selecting the package.
3. Import everything by clicking <kbd>Import</kbd>.
4. Give the installer a moment to add the appropriate OpenUPM registries to your project.
5. You're all set!

<sup><a href="https://github.com/cronyxllc/DeveloperConsole/wiki/Installing-via-OpenUPM">See more information here!</a></sup>
</details>

<details>
  <summary><b>Via OpenUPM</b></summary>

Run:

```
~/MyProject $ openupm add com.cronyx.console
```

from within your project directory.

<sup><a href="https://github.com/cronyxllc/DeveloperConsole/wiki/Installing-via-PackageInstaller">See more information here!</a></sup>
</details>

<details>
<summary><b>Via UPM (Tarball)</b></summary>
<ol>
  <li>Navigate to <a href="https://github.com/cronyxllc/DeveloperConsole/releases">Releases</a> and choose a release.</li>
  <li>Download the DeveloperConsole_v*.tar.gz file for that release.</li>
  <li>Open the Package Manager window (<kbd>Window > Package Manager</kbd>), click the ➕, and then click <code>Add package from tarball...</code></li>
<img src="https://raw.githubusercontent.com/cronyxllc/DeveloperConsole/main/docs/images/Install_UPMTarball.PNG" width=300px/>
  <li>Select the tarball file you just downloaded, and then click <kbd>Open</kbd>.</li>
  <li>You're all set!</li>
</ol>

<sup><a href="https://github.com/cronyxllc/DeveloperConsole/wiki/Installing-via-UPM-(Tarball)">See more information here!</a></sup>
</details>

<details>
<summary><b>Via UPM (Git)</b></summary>
<ol>
  <li>Open the Package Manager window (<kbd>Window > Package Manager</kbd>), click the ➕, and then click <code>Add package from git...</code></li>
<img src="https://raw.githubusercontent.com/cronyxllc/DeveloperConsole/main/docs/images/Install_UPMGit_URL.PNG" width=300px/>
  <li>Enter <code>https://github.com/cronyxllc/DeveloperConsole.git#upm</code> for the URL when prompted.</li>
  <li>Click <kbd>Add</kbd> and wait a moment.</li>
  <li>You're all set!</li>
</ol>

<sup><a href="https://github.com/cronyxllc/DeveloperConsole/wiki/Installing-via-UPM-(Git)">See more information here!</a></sup>
</details>

## Getting started

### Using the console

By default, you can open the in-game console by pressing the backquote key (`` ` ``) (or tilde, `~`, on QWERTY keyboards). This will open the console UI and allow you to start entering commands. For your first command, try printing the working directory:

```
~ $ pwd
C:\Users\MyUser\AppData\LocalLow\DefaultCompany\DeveloperConsole
~ $ █
```

Just like in a Bash console, past inputs can be cycled through using the up and down arrow keys. For a list of all commands that can be called, enter `$ help`.

### Customizing the console

To customize the console's settings, go to <kbd>Window > DeveloperConsole > Settings</kbd>. This will create a `ConsoleSettings` asset in your projects directory. Hover over any of the settings to get a description of what that feature does. For a more detailed description of the console's settings, see the [settings documentation](Console-settings).

### Adding commands to the console

The `DeveloperConsole` package was created with the intention that you would extend its functionality by creating console commands specific to your own project. Creating a console command can be as simple as tagging a static method with a `CommandAttribute`:

```csharp
[Command("cmd")]
public static void MyCommand (Vector3 v)
{
    // Your command's code
}
```

and calling it like so:

```
~ $ cmd (1 2 3)
~ $ █
```

or as complicated as creating a custom class that manually parses command-line arguments. See the documentation on [console commands](Console-Commands) for more information on creating your own commands. 

## Contributing

Please feel free to contribute to this project! If you'd like to contribute, please do any of the following:

* ‼️ Let others know about this project! We'd like to get the word out!
* 🐛 [Open an issue with a bug report or feature request.](https://github.com/cronyxllc/DeveloperConsole/issues/new/choose)
* ➕ Open a pull request with a change or new feature. Please let us know before you start working to prevent development conflicts.
