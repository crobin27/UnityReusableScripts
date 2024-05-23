# Bootstrapper

## Description

The `Bootstrapper` class is designed to initialize essential systems before the first scene loads in a Unity project. By loading and instantiating a predefined `Systems` prefab, it ensures that critical systems are available and persist across scene loads.

## Usage

### Overview

The `Bootstrapper` class leverages Unity's `[RuntimeInitializeOnLoadMethod]` attribute to execute code before any scene is loaded. It attempts to load a `Systems` prefab from the `Resources` folder, instantiate it, and mark it to not be destroyed on load.
