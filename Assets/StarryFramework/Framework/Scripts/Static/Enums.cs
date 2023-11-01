using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public enum ModuleType { Scene, Timer, Event, Save, Resource, Audio, ObjectPool, FSM}

    [Serializable]
    public enum FrameworkState { Stop, Awake, Init, Runtime, ShutDown }

    [Serializable]
    public enum ShutdownType { Quit, Restart, None }

    [Serializable]
    public enum TimerState {Null, Ready, Active, Pause, Stop}

    [Serializable]
    public enum ContinueGame {Allow, Locked}

    [Serializable]
    public enum SaveState { Unloaded, Loaded }

    [Serializable]
    public enum LoadState { Loading, Idle }

    [Serializable]
    public enum AudioState { Stop, Pause, Playing }
}
