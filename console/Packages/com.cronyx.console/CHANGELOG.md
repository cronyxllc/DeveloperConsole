# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## Unreleased
### Added
- Added a public overload for `DeveloperConsole.RegisterCommand` that takes an `IConsoleCommand` instance.

### Changed
- Made the formatting of the `help` command more rigorous, supporting multi-line command descriptions and coloring built-in commands blue.

## [1.1.1] - 2021-03-23
### Changed
- Changed `package.json` version to reflect actual, current version

## [1.1.0] - 2021-03-22
### Added
- Added new built-in commands
    - `rm`: Removes files and directories
    - `mkdir`: Creates directories

### Changed
- Changed `Third Party Notices.md` markdown

## [1.0.0] - 2021-03-02
### Added
- Initial project architecture
    - Support for automatic and manual parsing
    - Automatic parser types for basic C# and Unity types
- DeveloperConsole class which exposes major components of console API
- README that contains outline of features and information about installation, getting started, and contributing
