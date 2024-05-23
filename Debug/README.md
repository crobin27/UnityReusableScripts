# Bug

## Description

The `Bug` class provides enhanced control over debugging logs in Unity. It allows toggling debugging logs on or off and provides methods for logging messages, warnings, errors, and exceptions. This utility is helpful for managing debug output more effectively during development.

## Usage

### Overview

The `Bug` class uses a static `_isBugMode` flag to determine whether to log messages. You can enable or disable this mode via the `IsBugMode` property. The class also includes an `OnException` event for handling exceptions.
