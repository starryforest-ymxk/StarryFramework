<p align="center"><img width="501" height="106" src="./images/StarryFramework-Logo.png"></p>

<p align="center">
	<a href="https://github.com/starryforest-ymxk/StarryFramework/blob/master/LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue.svg" title="license-mit" /></a>
    <a href="https://github.com/starryforest-ymxk/StarryFramework/releases"><img src="https://img.shields.io/github/v/release/starryforest-ymxk/StarryFramework?color=green"/></a>
</p>

|[中文](README.md)|[English](README_EN.md)|

- [About StarryFramework](#about-starryframework)
  - [Core Features](#core-features)
  - [Folder Structure](#folder-structure)
  - [Basic Modules Introduction](#basic-modules-introduction)
  - [Extension Modules Introduction](#extension-modules-introduction)
  - [Framework Dependencies](#framework-dependencies)
- [Framework Download](#framework-download)
- [Quick Start](#quick-start)
  - [Basic Configuration](#basic-configuration)
  - [Extension Module Configuration](#extension-module-configuration)
  - [Simple Usage](#simple-usage)
- [Copyright and Acknowledgments](#copyright-and-acknowledgments)
- [Support](#support)

---

## About StarryFramework

StarryFramework is a lightweight Unity game development framework designed to provide a set of out-of-the-box functional modules that help developers quickly build high-quality game projects. The framework adopts the MOM (Manager-Of-Managers) architecture design, achieving zero coupling between modules and supporting flexible module composition and extension.

### Core Features

- **Modular Design**: All functions exist as independent modules, which can be freely combined according to project requirements
- **Zero Coupling Architecture**: Modules communicate through the event system, achieving loose coupling
- **Unified Entry Point**: Provides a unified API access point through the `Framework` static class
- **Easy to Extend**: Supports custom module development and easy integration of new features
- **Editor-Friendly**: Provides custom Inspector panels for each module for easy configuration and debugging
- **Lifecycle Management**: Strict module lifecycle control ensures proper initialization and destruction order

### Folder Structure

```
/Assets
├── /Plugins
│   └── /StarryFramework
│       ├── /Runtime
│       │   ├── /Attributes        # Custom Attributes
│       │   ├── /Framework         # Framework Core
│       │   └── /Scene
│       │       └── GameFramework.unity # Framework startup scene
│       ├── /Editor                # Module Editors
│       ├── /Extensions            # Extension Modules
│       ├── /Info                  # Framework Documentation
│       ├── /Resources
│       └── StarryFrameworkRootMarker.txt # Plugin root marker
```

### Plugin Root Discovery

- The framework locates its root directory via `StarryFrameworkRootMarker.txt` at the plugin root.
- Users can move the plugin folder to any `Assets` subfolder and paths will be resolved automatically.
- Do not remove this marker file, otherwise editor tools cannot determine the plugin root.

### Basic Modules Introduction

The current version includes eight basic modules:

1. **Event Module**: Event system based on delegates, supporting decoupled communication between modules.
2. **Save Module**: Complete save management system, supporting multiple saves and auto-save.
3. **FSM Module**: Flexible state machine management system, supporting multiple concurrent state machines.

4. **ObjectPool Module**: Efficient object pool management system, reducing frequent instantiation overhead.
5. **Timer Module**: Feature-rich timer management system, supporting multiple timer types.
6. **Scene Module**: Provides scene loading, unloading and switching functions, supporting scene transition animations.
7. **Resource Module**: Resource management system based on Resources and Addressables.
8. **UI Module**: Complete UI form management system, supporting UI grouping and hierarchical management.

### Extension Modules Introduction

The current version includes one extension module:

- **Audio Module**

  Considering that some projects use audio middleware to manage game audio in actual development, this framework provides an audio module for FMOD. This audio module encapsulates the API provided by FMOD, making the interface more concise and easier to use.

  The audio module provides audio and BGM hosting, supports volume control for multiple output channels, supports dynamically attaching audio to objects; supports automatic BGM playback for different scenes, audio preloading, etc.

  The audio module content is in `StarryFramework_AudioExtention.unitypackage`.

### Framework Dependencies

StarryFramework depends on the following Unity Package Manager (UPM) packages and third-party plugins:

- Required Dependencies
  - Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`)
  - Addressables (`com.unity.addressables`)
  - Unity UI (`com.unity.ugui`)
  - DOTween (third-party plugin, integrated in the framework package)

- Optional Dependencies
  - FMOD for Unity

After import, the editor automatically checks and installs missing required UPM dependencies (Addressables / Newtonsoft.Json / Unity UI).

---

## Framework Download

- Download unitypackage

  Go to the [Releases page](https://github.com/starryforest-ymxk/StarryFramework/releases) to download `StarryFramework.unitypackage`

  If your project uses FMOD and requires the framework's FMOD extension, also download `StarryFramework_AudioExtention.unitypackage`

- Or: Clone the project repository

  ``` 
  git clone https://github.com/starryforest-ymxk/StarryFramework.git
  ```

  The repository uses FMOD to manage audio files, so it includes the basic framework and extension content. Additionally, the `Assets/Test` directory in the repository contains some test code for various framework modules.

---

## Quick Start

### Basic Configuration

1. Import framework dependencies

  - If required dependencies are missing (`Newtonsoft.Json`, `Addressables`, `Unity UI`), the framework installs them automatically

2. Import the framework
   - Import the downloaded `StarryFramework.unitypackage` into your project

3. Configure the startup scene

  - Open the `Plugins/StarryFramework/Runtime/Scene/GameFramework.unity` scene

   - Select `File > Build Settings` from the Unity menu

   - Add `GameFramework.unity` to the `Scenes in Build` list

   - **Ensure the GameFramework scene has buildIndex 0** (drag it to the first position in the list)

   - This scene will serve as the game's startup scene and remain loaded throughout the game runtime


3. Adjust framework settings

  - Select `Tools > StarryFramework > Settings Panel` from the Unity menu to open the framework settings panel
   - To enable framework functionality in the game, use the `Framework Start` startup mode and set the framework's initial loading scene; for simple code functionality testing, use the `Normal Start` startup mode
     - Framework Start: After entering play mode, the framework first loads the GameFramework scene, then loads the initial scene
     - Normal Start: Unity's default behavior, remains in the current scene after entering play mode
   - The Modules list contains the framework's enabled modules
     - The order between modules represents their priority, modules closer to the front of the list have higher priority
     - Higher priority modules initialize earlier, destroy later, and are called earlier each frame in the lifecycle

   - On the editor panel corresponding to each submodule, you can configure specific settings for each module (Scene module, Timer module, Save module, etc.)

4. Configure scene camera

   - The `Camera` object in the `GameFramework` scene is attached with `SceneChangeCameraControl`:
     - This camera is used to render scene transition animations and is enabled by default only when playing transition animations; check `Is Main Camera` to use it as the global main camera


### Extension Module Configuration

**Audio Module (Audio Extension Module)**

The audio module is an optional extension module based on the FMOD audio middleware. If your project needs to use FMOD for audio management, follow these configuration steps:

**Prerequisites**

1. Install FMOD for Unity
   - Go to the [FMOD official website](https://www.fmod.com/) to download the FMOD for Unity plugin (Version 2.02.11 or later)
   - Import the FMOD plugin into your Unity project
   - Ensure FMOD is correctly configured and working properly

2. Import the audio extension module
   - Download `StarryFramework_AudioExtention.unitypackage`
   - Import the extension package into your project
  - The extension module will be automatically installed under the plugin root at `/Extensions/Runtime/Audio Module`

**Configuration Steps**

1. Add the audio module to the framework
  - Open the framework settings panel (`Tools > StarryFramework > Settings Panel`)
   - Add `AudioComponent` to the Modules list
   - Adjust the module's priority order as needed
   - Drag the Audio prefab from the Audio Module into the GameFramework scene as a child object
2. Configure audio module settings
   - Configure audio settings in the AudioComponent's editor panel, such as globally loaded audio banks, BGM for each scene, etc.




### Simple Usage

**Basic Calling Method**

All framework functions are accessed through the `Framework` static class, requiring the `StarryFramework` namespace:

```csharp
using StarryFramework;

public class Example : MonoBehaviour
{
    void Start()
    {
        // Trigger event
        Framework.EventComponent.InvokeEvent("GameStart");
        // Save data
        Framework.SaveComponent.SaveData("Manual Save");
        // Load scene
        Framework.SceneComponent.LoadScene("MainGame");
    }
}
```



**Event Module Example**

```csharp
// Add event listener
Framework.EventComponent.AddEventListener("OnPlayerDeath", OnPlayerDeath);

// Remove event listener
Framework.EventComponent.RemoveEventListener("OnPlayerDeath", OnPlayerDeath);

// Trigger event
Framework.EventComponent.InvokeEvent<int>("OnScoreChanged", 100);
```



**Save Module Example**

```csharp
// Create new save
Framework.SaveComponent.CreateNewData(true, "New Game");

// Modify player data
Framework.SaveComponent.PlayerData.playerName = "Player1";
Framework.SaveComponent.PlayerData.level = 5;

// Save data
Framework.SaveComponent.SaveData("Progress Save");
// Load save
bool success = Framework.SaveComponent.LoadData(0);
```



**Scene Module Example**

```csharp
// Load scene with default animation
Framework.SceneComponent.LoadSceneDefault("Level2");
// Load scene with custom progress bar
Framework.SceneComponent.LoadSceneProgressBar(
    "Level2", 
    "UI/LoadingScreen", 
    () => Debug.Log("Loading Complete")
);
```



**UI Module Example**

```csharp
// Open UI using options (request policy first)
Framework.UIComponent.OpenUIForm(new OpenUIFormOptions
{
    AssetName = "SettingsPanel",
    GroupName = "Dialog",
    PauseCoveredUIForm = true,
    OpenPolicy = UIOpenPolicy.SingleInstancePerGroup,
    InstanceKey = "Main"
});

// Multi-instance open for the same asset
Framework.UIComponent.OpenUIForm(new OpenUIFormOptions
{
    AssetName = "RewardPanel",
    GroupName = "Dialog",
    OpenPolicy = UIOpenPolicy.MultiInstanceGlobal,
    InstanceKey = "reward_001"
});

// Close a specific instance by serialId
UIForm[] rewardForms = Framework.UIComponent.GetUIFormsByInstanceKey("RewardPanel", "reward_001");
if (rewardForms.Length > 0)
{
    Framework.UIComponent.CloseUIForm(rewardForms[0].SerialID);
}

// Close topmost instance by asset + InstanceKey
Framework.UIComponent.CloseUIForm("SettingsPanel", "Main");

// Close all instances with the same asset name
Framework.UIComponent.CloseAllUIForms("RewardPanel");
```

---

## Copyright and Acknowledgments

StarryFramework is licensed under the [MIT License](https://github.com/starryforest-ymxk/StarryFramework/blob/master/LICENSE.md)

This project uses the following open-source project libraries:

- **Newtonsoft.Json**

  This project uses Newtonsoft.Json (Json.NET) for JSON serialization and deserialization. Developed by Newtonsoft, licensed under MIT. See the [Newtonsoft.Json official website](https://www.newtonsoft.com/json)

- **Unity Addressables**

  This project uses the Addressables system provided by Unity Technologies for resource management. See the [Unity Addressables documentation](https://docs.unity3d.com/Packages/com.unity.addressables@latest)

- **FMOD Studio**

  This project uses FMOD Studio and technology provided by Firelight Technologies Pty Ltd. Visit the [FMOD official website](https://www.fmod.com/) to obtain and integrate FMOD

- **DOTween** (included in `StarryFramework.unitypackage`)

  This project uses DOTween, developed by Daniele Giardini - Demigiant, for animation management. See the [DOTween official website](http://dotween.demigiant.com/)

Also, thanks to my friend [NoSLoofah](https://github.com/NoSLoofah) for the help provided while writing the framework.

---

## Support

**Documentation and Tutorials**

- [API Quick Reference Manual](API_QUICK_REFERENCE.md)



**Issue Feedback**

If you encounter problems while using StarryFramework, you can seek help through the following methods:

1. **Check Documentation**: First check this document and the API Quick Reference Manual, most common issues are covered
2. **Check Examples**: Refer to the test scenes and code examples in the `/Assets/Test` directory of the project files
3. **GitHub Issues**: Submit an Issue on the project's GitHub repository



**Contributing**

We welcome contributions in any form!

- 🐛 Report Bugs
- 💡 Suggest new features
- 📝 Improve documentation
- 🔧 Submit code optimizations
- ⭐ Star the project



**Get Updates**

- **GitHub Repository**: https://github.com/starryforest-ymxk/StarryFramework
- **Latest Version**: Check the [Releases](https://github.com/starryforest-ymxk/StarryFramework/releases) page

---

**Thank you for using StarryFramework! Wishing you smooth game development!** 🌟
