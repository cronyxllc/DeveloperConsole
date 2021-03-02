# DeveloperConsole

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
  <p>Highly customizable <code>GetOpt</code> command syntax and automatic help text generation:</p>
  
  <pre lang=csharp>
[Command("showcase", Description = "Showcases the flexibility of the command parser.")]
public static void CustomCommand(
  [Positional(Description = "The first positional", Meta = "FOO")] Vector3 posOne,
  [Positional(Description = "The second positional", Max = 4, Meta = "BAR", Optional = true)] IEnumerable<string> posTwo,
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
  <p>A detailed documentation of all of these features and more over at <a href="https://github.com/cronyxllc/DeveloperConsole/wiki">the wiki</a>!</p>
  </li>
</ul>
