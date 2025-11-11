# StarryFramework API Quick Reference

|[中文](API速查手册.md)|[English](API_QUICK_REFERENCE.md)|

---

## 📖 Documentation Navigation

**For framework configuration and quick start, please refer to**: [README_EN.md](README_EN.md)

**Module Quick Jump**:
[Core](#core) | [Event](#event) | [Save](#save) | [Scene](#scene) | [Timer](#timer) | [FSM](#fsm) | [ObjectPool](#pool) | [Resource](#resource) | [UI](#ui) | [Audio](#audio) | [Utils](#utils)

[**Best Practices**](#best_pract)

---

## <a id="core">⚙️ Framework Core</a>

### 🔧 Basic Usage

All framework features are accessed through the `Framework` static class, requiring the `StarryFramework` namespace:

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

### 🔧 Framework Control

```csharp
// Exit application
Framework.ShutDown(ShutdownType.Quit);

// Restart framework
Framework.ShutDown(ShutdownType.Restart);
```

---

## <a id="event">📡 Event Module</a>

**Entry**: `Framework.EventComponent`

### 🔑 Core API

| Method | Description | Return Type |
|------|------|--------|
| `AddEventListener(string, UnityAction)` | Add no-param event listener | void |
| `AddEventListener<T>(string, UnityAction<T>)` | Add 1-param listener | void |
| `AddEventListener<T1,T2>(string, UnityAction<T1,T2>)` | Add 2-param listener | void |
| `AddEventListener<T1,T2,T3>(...)` | Add 3-param listener | void |
| `AddEventListener<T1,T2,T3,T4>(...)` | Add 4-param listener | void |
| `RemoveEventListener(string, UnityAction)` | Remove no-param event listener | void |
| `RemoveEventListener<T>(string, UnityAction<T>)` | Remove 1-param listener | void |
| `RemoveEventListener<T1,T2>(...)` | Remove 2-param listener | void |
| `RemoveEventListener<T1,T2,T3>(...)` | Remove 3-param listener | void |
| `RemoveEventListener<T1,T2,T3,T4>(...)` | Remove 4-param listener | void |
| `InvokeEvent(string)` | Trigger no-param event | void |
| `InvokeEvent<T>(string, T)` | Trigger 1-param event | void |
| `InvokeEvent<T1,T2>(string, T1, T2)` | Trigger 2-param event | void |
| `InvokeEvent<T1,T2,T3>(string, T1, T2, T3)` | Trigger 3-param event | void |
| `InvokeEvent<T1,T2,T3,T4>(...)` | Trigger 4-param event | void |
| `InvokeDelayedEvent(string, float, bool = false)` | Delayed trigger no-param event | void |
| `InvokeDelayedEvent<T>(string, T, float, bool = false)` | Delayed trigger 1-param event | void |
| `InvokeDelayedEvent<T1,T2>(string, T1, T2, float, bool = false)` | Delayed trigger 2-param event | void |
| `InvokeDelayedEvent<T1,T2,T3>(...)` | Delayed trigger 3-param event | void |
| `InvokeDelayedEvent<T1,T2,T3,T4>(...)` | Delayed trigger 4-param event | void |
| `ClearAllEventLinsteners(string)` | Clear all listeners of specified event | void |
| `ClearEventListeners(string)` | Clear no-param event listeners | void |
| `ClearEventListeners<T>(string)` | Clear 1-param event listeners | void |
| `ClearEventListeners<T1,T2>(string)` | Clear 2-param event listeners | void |
| `ClearEventListeners<T1,T2,T3>(string)` | Clear 3-param event listeners | void |
| `ClearEventListeners<T1,T2,T3,T4>(string)` | Clear 4-param event listeners | void |
| `GetAllEventsInfo()` | Get all events information | Dictionary<string, Dictionary<string, int>> |

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `LastEventName` | string | Last triggered event name |
| `LastEventParam` | string | Last triggered event parameter type |

---

## <a id="save">💾 Save Module</a>

**Entry**: `Framework.SaveComponent`

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `PlayerData` | PlayerData | Player game data (must load first) |
| `GameSettings` | GameSettings | Game settings data |
| `DefaultDataIndex` | int | Default save index |
| `CurrentLoadedDataIndex` | int | Currently loaded save index |
| `AutoSaveDataInterval` | float | Auto-save interval time |
| `LastAutoSaveTime` | float | Last auto-save time |
| `AutoSave` | bool | Whether auto-save is enabled |
| `AutoSaveInfo` | string | Auto-save information |
| `SaveInfoList` | List<string> | Save info list |
| `DataInfoDic` | Dictionary<int, PlayerDataInfo> | Save info dictionary |
| `PlayerDataLoaded` | bool | Whether player data is loaded |
| `GameSettingsLoaded` | bool | Whether game settings is loaded |

### 🔑 Core API

| Method | Description | Return Type |
|------|------|--------|
| `CreateNewData(bool, string = "")` | Create new save | void |
| `SaveData(string = "")` | Save current save | void |
| `SaveData(int, string = "")` | Save to specified index | void |
| `LoadData()` | Load default save | bool |
| `LoadData(int)` | Load specified index save | bool |
| `LoadDataInfo()` | Load default save info | PlayerDataInfo |
| `LoadDataInfo(int)` | Load specified save info | PlayerDataInfo |
| `UnloadData()` | Unload current save | bool |
| `DeleteData(int)` | Delete specified save | bool |
| `GetDataInfos()` | Get all save info | List<PlayerDataInfo> |
| `StartAutoSaveTimer()` | Start auto-save timer | void |
| `StopAutoSaveTimer()` | Stop auto-save timer | void |
| `SetSaveInfo(int)` | Set save comment index | void |
| `SetSaveInfo(string)` | Set save comment string | void |

### 🧩 Important Classes

#### PlayerData (Save Data Class)

**Location**: `StarryFramework/Runtime/Framework/Save Module/PlayerData.cs`

Users need to define their own save data structure in this file.

```csharp
[Serializable]
public sealed class PlayerData
{
    public int test = 0;
    
    // Auto-integrate event module: bool fields can be automatically set to true by triggering same-name events
    public bool event1;
    
    // Supports complex types like List, Dictionary
    public List<string> inventoryList = new();
    public CustomData customData = new();
}
```

#### GameSettings (Game Settings Class)

**Location**: `StarryFramework/Runtime/Framework/Save Module/GameSettings.cs`

Users need to define their game settings data structure in this file.

```csharp
[Serializable]
public sealed class GameSettings
{
    public float bgmVolume = 1f;
    public float soundVolume = 1f;
    public float uiVolume = 1f;
}
```

---

## <a id="scene">🎬 Scene Module</a>

**Entry**: `Framework.SceneComponent`

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `CurrentActiveScene` | Scene | Current active scene |
| `SceneLoadedTime` | float | Scene load timestamp |
| `SceneTime` | float | Scene runtime duration |

### 🔑 Core API

| Method | Description | Return Type |
|------|------|--------|
| `LoadScene(int, UnityAction = null, bool = true)` | Load scene (index) | AsyncOperation |
| `LoadScene(string, UnityAction = null, bool = true)` | Load scene (name) | AsyncOperation |
| `UnloadScene(UnityAction = null)` | Unload current active scene | void |
| `UnloadScene(int, UnityAction = null, bool = true)` | Unload scene (index) | void |
| `UnloadScene(string, UnityAction = null, bool = true)` | Unload scene (name) | void |
| `ChangeScene(int, int = -1, UnityAction = null)` | Change scene (index) | AsyncOperation |
| `ChangeScene(string, string = "", UnityAction = null)` | Change scene (name) | AsyncOperation |
| `LoadSceneDefault(int, UnityAction = null)` | Default fade load scene (index) | void |
| `LoadSceneDefault(string, UnityAction = null)` | Default fade load scene (name) | void |
| `LoadSceneProgressBar(int, GameObject, UnityAction = null)` | Progress bar load (index, GameObject) | void |
| `LoadSceneProgressBar(int, string, UnityAction = null)` | Progress bar load (index, path) | void |
| `LoadSceneProgressBar(string, GameObject, UnityAction = null)` | Progress bar load (name, GameObject) | void |
| `LoadSceneProgressBar(string, string, UnityAction = null)` | Progress bar load (name, path) | void |
| `ChangeSceneDefault(int, int = -1, UnityAction = null)` | Default fade change scene (index) | void |
| `ChangeSceneDefault(string, string = "", UnityAction = null)` | Default fade change scene (name) | void |
| `ChangeSceneProgressBar(int, int, GameObject, UnityAction = null)` | Progress bar change (index, GameObject) | void |
| `ChangeSceneProgressBar(int, int, string, UnityAction = null)` | Progress bar change (index, path) | void |
| `ChangeSceneProgressBar(string, string, GameObject, UnityAction = null)` | Progress bar change (name, GameObject) | void |
| `ChangeSceneProgressBar(string, string, string, UnityAction = null)` | Progress bar change (name, path) | void |
| `ProcessCoroutine(AsyncOperation, LoadProgressBase, UnityAction<AsyncOperation>, float = 0f, float = 1f)` | Progress bar coroutine | IEnumerator |

### 🧩 Important Base Classes

#### LoadProgressBase (Scene Loading Progress Bar Base Class)

**Location**: `StarryFramework/Runtime/Framework/Scene Module/LoadProgressBase.cs`

Custom progress bar UI needs to inherit this class.

```csharp
public abstract class LoadProgressBase : MonoBehaviour
{
    public float speed = 0.05f;  // Progress bar speed
    
    // Set progress value on UI component
    public abstract void SetProgressValue(float value);
    
    // Callback after loading complete, needs to call AllowSceneActivate() to activate scene
    public abstract void BeforeSetActive(AsyncOperation asyncOperation);
    
    protected void AllowSceneActivate(AsyncOperation asyncOperation)
    {
        asyncOperation.allowSceneActivation = true;
    }
}
```

**Example Implementation**:

```csharp
public class MyLoadBar : LoadProgressBase
{
    public Slider slider;
    
    public override void SetProgressValue(float value)
    {
        slider.value = value;
    }
    
    public override void BeforeSetActive(AsyncOperation asyncOperation)
    {
        StartCoroutine(Routine());
        return;

        IEnumerator Routine()
        {
            while(true)
            {
                if (Input.anyKeyDown)
                {
                    AllowSceneActivate(asyncOperation);
                    break;
                }
                yield return null;
            }
        }
    }
}
```

---

## <a id="timer">⏱️ Timer Module</a>

**Entry**: `Framework.TimerComponent`

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `Timers` | List<Timer> | All timers list |
| `TriggerTimers` | List<TriggerTimer> | All trigger timers list |
| `AsyncTimers` | List<AsyncTimer> | All async timers list |
| `ClearUnusedTriggerTimersInterval` | float | Clear unused trigger timers interval |
| `ClearUnusedAsyncTimersInterval` | float | Clear unused async timers interval |

### 🔑 Core API - Timer

| Method | Description | Return Type |
|------|------|--------|
| `RegisterTimer(bool = false, float = 0f, UnityAction = null)` | Register anonymous timer | ITimer |
| `RegisterTimer(string, bool = false, float = 0f)` | Register non-anonymous timer | void |
| `DeleteTimer(ITimer)` | Recycle anonymous timer | void |
| `DeleteTimer(string)` | Recycle non-anonymous timer | void |
| `BindUpdateAction(string, UnityAction)` | Bind Update event | void |
| `GetTimerState(string)` | Get timer state | TimerState |
| `GetTimerTime(string)` | Get timer time | float |
| `PauseTimer(string)` | Pause timer | void |
| `StopTimer(string)` | Stop timer | void |
| `ResumeTimer(string)` | Resume timer | void |
| `StartTimer(string)` | Start timer | void |
| `ResetTimer(string)` | Reset timer | void |

### 🔑 Core API - TriggerTimer

| Method | Description | Return Type |
|------|------|--------|
| `RegisterTriggerTimer(float, UnityAction, bool = false, string = "", bool = false)` | Register trigger timer | void |
| `DeleteTriggerTimer(string)` | Delete trigger timer | void |
| `GetTriggerTimerState(string)` | Get trigger timer state | TimerState |
| `PauseTriggerTimer(string)` | Pause trigger timer | void |
| `ResumeTriggerTimer(string)` | Resume trigger timer | void |
| `StopTriggerTimer(string)` | Stop trigger timer | void |
| `StartTriggerTimer(string)` | Start trigger timer | void |
| `ClearUnnamedTriggerTimers()` | Clear all anonymous trigger timers | void |

### 🔑 Core API - AsyncTimer

| Method | Description | Return Type |
|------|------|--------|
| `RegisterAsyncTimer(float, UnityAction, bool = false, string = "")` | Register async trigger timer | void |
| `DeleteAsyncTimer(string)` | Delete async trigger timer | void |
| `GetAsyncTimerState(string)` | Get async trigger timer state | TimerState |
| `StartAsyncTimer(string)` | Start async trigger timer | void |
| `StopAsyncTimer(string)` | Stop async trigger timer | void |
| `ClearUnnamedAsyncTimers()` | Clear all anonymous async trigger timers | void |

### 🧩 Important Interfaces

#### ITimer (Timer Interface)

**Location**: `StarryFramework/Runtime/Framework/Timer Module/ITimer.cs`

Normal timers return this interface.

```csharp
public interface ITimer
{
    float Time { get; }           // Current time
    TimerState TimerState { get; }  // Timer state
    
    void BindUpdateAction(UnityAction action);  // Bind update callback
    void Pause();    // Pause
    void Resume();   // Resume
    void Start();    // Start
    void Stop();     // Stop
    void Reset();    // Reset
}
```

**Usage Example**:

```csharp
ITimer cooldownTimer = Framework.TimerComponent.RegisterTimer();

void Update()
{
    if (cooldownTimer.Time >= 3f && Input.GetKeyDown(KeyCode.Space))
    {
        UseSkill();
        cooldownTimer.Reset();
        cooldownTimer.Start();
    }
}
```

---

## <a id="fsm">🔄 FSM Module</a>

**Entry**: `Framework.FSMComponent`

### 🔑 Core API

| Method | Description | Return Type |
|------|------|--------|
| `GetFSMCount()` | Get finite state machine count | int |
| `CreateFSM<T>(string, T, List<FSMState<T>>)` | Create FSM (List) | IFSM<T> |
| `CreateFSM<T>(string, T, FSMState<T>[])` | Create FSM (array) | IFSM<T> |
| `DestroyFSM<T>(string)` | Destroy FSM (name) | void |
| `DestroyFSM<T>(IFSM<T>)` | Destroy FSM (object) | void |
| `HasFSM<T>(string)` | Check if FSM exists | bool |
| `GetFSM<T>(string)` | Get specific FSM | IFSM<T> |
| `GetAllFSMs()` | Get all FSMs | FSMBase[] |

### 🧩 Important Base Classes

### FSMState\<T> (State Machine State Base Class)

**Location**: `StarryFramework/Runtime/Framework/FSM Module/FSMState.cs`

All FSM states need to inherit this class.

```csharp
public abstract class FSMState<T> where T : class
{
    protected internal virtual void OnInit(IFSM<T> fsm) { }         // When creating FSM
    protected internal virtual void OnEnter(IFSM<T> fsm) { }        // When entering state
    protected internal virtual void OnUpdate(IFSM<T> fsm) { }       // Per-frame update
    protected internal virtual void OnLeave(IFSM<T> fsm, bool isShutdown) { }  // When leaving state
    protected internal virtual void OnDestroy(IFSM<T> fsm) { }      // When destroying FSM
    
    // Change state
    protected internal virtual void ChangeState<S>(IFSM<T> fsm) where S : FSMState<T> { }
    protected internal virtual void ChangeState(IFSM<T> fsm, Type stateType) { }
}
```

**Example Implementation**:

```csharp
public class IdleState : FSMState<Enemy>
{
    protected internal override void OnEnter(IFSM<Enemy> fsm)
    {
        Debug.Log("Enter Idle State");
    }
    
    protected internal override void OnUpdate(IFSM<Enemy> fsm)
    {
        if (fsm.Owner.DetectedPlayer())
            ChangeState<AttackState>(fsm);
    }
    
    protected internal override void OnLeave(IFSM<Enemy> fsm, bool isShutdown)
    {
        Debug.Log("Leave Idle State");
    }
}
```

---

## <a id="pool">🎱 ObjectPool Module</a>

**Entry**: `Framework.ObjectPoolComponent`

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `ObjectPools` | List<ObjectPoolProperty> | Object pool properties list |

### 🔑 Core API

| Method | Description | Return Type |
|------|------|--------|
| `Register<T>(float, float, string = "")` | Register Object pool | void |
| `Register<T>(GameObject, float, float, GameObject = null, string = "")` | Register GameObject pool (GameObject) | void |
| `Register<T>(string, float, float, GameObject = null, string = "")` | Register GameObject pool (path) | void |
| `Require<T>(string = "")` | Get an object | T |
| `Recycle<T>(T, string = "")` | Recycle an object | void |
| `SetLocked<T>(bool, string = "")` | Lock or unlock object pool | void |
| `ReleaseObject<T>(T, string = "")` | Release an object | void |
| `ReleaseAllUnused<T>(string = "")` | Release all unused objects | void |
| `ReleaseAllObjects<T>(string = "")` | Release all objects | void |
| `ReleasePool<T>(string = "")` | Release object pool | void |

### 🧩 Important Base Classes

#### ObjectBase (Object Pool Object Base Class)

**Location**: `StarryFramework/Runtime/Framework/ObjectPool Module/ObjectBase.cs`

Normal C# class object pool needs to inherit this class.

```csharp
public abstract class ObjectBase : IObjectBase
{
    public float lastUseTime { get; set; }
    public bool inUse { get; set; }
    public bool releaseFlag { get; set; }
    
    public virtual void OnSpawn() { }      // When taken from pool
    public virtual void OnUnspawn() { }    // When recycled to pool
    public virtual void OnRelease() { }    // When released from pool
}
```

**Example Implementation**:

```csharp
public class BulletData : ObjectBase
{
    public int damage;
    public float speed;
    
    public override void OnSpawn()
    {
        damage = 10;
        speed = 5f;
    }
    
    public override void OnUnspawn()
    {
        damage = 0;
    }
}
```

#### GameObjectBase (GameObject Object Pool Base Class)

**Location**: `StarryFramework/Runtime/Framework/ObjectPool Module/GameObjectBase.cs`

GameObject object pool needs to inherit this MonoBehaviour.

```csharp
public abstract class GameObjectBase : MonoBehaviour, IObjectBase
{
    public float lastUseTime { get; set; }
    public bool inUse { get; set; }
    public bool releaseFlag { get; set; }
    
    public virtual void OnSpawn() { }      // When taken from pool
    public virtual void OnUnspawn() { }    // When recycled to pool
    public virtual void OnRelease() { }    // When released from pool
}
```

**Example Implementation**:

```csharp
public class Bullet : GameObjectBase
{
    public float damage = 10f;
    
    private void Update()
    {
        if(transform.position.y<-90)
            Framework.ObjectPoolComponent.Recycle(this);
    }
}
```

---

## <a id="resource">📦 Resource Module</a>

**Entry**: `Framework.ResourceComponent`

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `State` | LoadState | Load state |
| `Progress` | float | Load progress |
| `ResourcePath` | string | Resource path |
| `TargetType` | Type | Target type |

### 🔑 Core API - Resources

| Method | Description | Return Type |
|------|------|--------|
| `LoadRes<T>(string, bool = false)` | Synchronously load Resources asset | T |
| `LoadAllRes<T>(string)` | Synchronously load all assets under path | T[] |
| `LoadResAsync<T>(string, UnityAction<T>, bool = false)` | Asynchronously load Resources asset | ResourceRequest |
| `UnloadRes(Object)` | Unload non-GameObject asset | void |
| `UnloadUnusedRes()` | Unload all unused Resources assets | void |

### 🔑 Core API - Addressables

| Method | Description | Return Type |
|------|------|--------|
| `LoadAddressable<T>(string, bool = false)` | Synchronously load Addressable asset | T |
| `LoadAddressableAsync<T>(string, UnityAction<T>, bool = false)` | Asynchronously load Addressable asset | AsyncOperationHandle<T> |
| `InstantiateAddressable(string, Transform = null)` | Instantiate Addressable GameObject | AsyncOperationHandle<GameObject> |
| `ReleaseAddressableHandle(AsyncOperationHandle)` | Release Addressable handle | void |
| `ReleaseAddressableAsset<T>(T)` | Release Addressable asset object | void |
| `ReleaseAddressableInstance(GameObject)` | Release Addressable instance | void |
| `ReleaseAllAddressableHandles()` | Release all Addressable handles | void |

---

## <a id="ui">🖼️ UI Module</a>

**Entry**: `Framework.UIComponent`

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `UIGroupsDic` | Dictionary<string, UIGroup> | UI groups dictionary |
| `UIFormsCacheList` | LinkedList<UIForm> | UI forms cache list |

### 🔑 Core API - UIGroup

| Method | Description | Return Type |
|------|------|--------|
| `HasUIGroup(string)` | Check if UI group exists | bool |
| `GetUIGroup(string)` | Get specified UI group | UIGroup |
| `GetAllUIGroups()` | Get all UI groups | UIGroup[] |
| `AddUIGroup(string)` | Add UI group | void |
| `RemoveUIGroup(string)` | Remove UI group | void |

### 🔑 Core API - UIForm

| Method | Description | Return Type |
|------|------|--------|
| `HasUIForm(string)` | Check if UI form exists | bool |
| `GetUIForm(string)` | Get specified UI form | UIForm |
| `OpenUIForm(string, string, bool)` | Open UI form | AsyncOperationHandle<UIForm> |
| `CloseUIForm(string)` | Close UI form (asset name) | void |
| `CloseUIForm(UIForm)` | Close UI form (object) | void |
| `RefocusUIForm(string)` | Refocus UI form (asset name) | void |
| `RefocusUIForm(UIForm)` | Refocus UI form (object) | void |
| `CloseAndReleaseAllForms()` | Close and release all UI forms | void |

### 🧩 Important Interfaces/Base Classes

#### UIFormLogic (UI Lifecycle Interface)

**Location**: `StarryFramework/Runtime/Framework/UI Module/UIFormLogic.cs`

All UI interface scripts must implement this interface.

```csharp
public interface UIFormLogic
{
    void OnInit(GameObject uiPrefab);           // When resource is loaded
    void OnRelease();                           // When resource is released
    void OnOpen();                              // When interface opens
    void OnClose(bool isShutdown);              // When interface closes
    void OnCover();                             // When interface is covered
    void OnReveal();                            // When interface is revealed
    void OnPause();                             // When interface pauses
    void OnResume();                            // When interface resumes
    void OnUpdate();                            // Per-frame update
    void OnDepthChanged(int formCountInUIGroup, int depthInUIGroup);  // When depth changes
    void OnRefocus();                           // When refocused
}
```

#### UguiForm (UI Abstract Base Class)

**Location**: `StarryFramework/Runtime/Framework/UI Module/Examples/UguiForm.cs`

UGUI implementation example provided by the framework, users can inherit and use.

```csharp
public abstract class UguiForm : MonoBehaviour, UIFormLogic
{
    // Provides default implementations like fade in/out
    protected virtual void Awake() { }
    public virtual void OnInit(GameObject uiPrefab) { }
    public virtual void OnOpen() { }
    // ... other lifecycle methods
}
```

---

## <a id="audio">🔊 Audio Module</a>

**Entry**: `Framework.AudioComponent` (namespace: `using StarryFramework.Extentions;`)

### 📊 Properties

| Property | Type | Description |
|------|------|------|
| `CurrentBGM` | string | Current BGM |
| `BGMState` | AudioState | BGM state |
| `CurrentBGMList` | List<EventReference> | Current BGM list |

### 🔑 Core API - PlayOneShot

| Method | Description | Return Type |
|------|------|--------|
| `PlayOneShot(EventReference, Vector3 = default)` | Play one-shot sound | void |
| `PlayOneShot(string, Vector3 = default)` | Play one-shot sound (path) | void |
| `PlayOneShotAttached(EventReference, GameObject)` | Play one-shot sound and attach | void |
| `PlayOneShotAttached(string, GameObject)` | Play one-shot sound and attach (path) | void |

### 🔑 Core API - VCA

| Method | Description | Return Type |
|------|------|--------|
| `SetVolume(string, float)` | Set VCA audio group volume | void |
| `GetVolume(string)` | Get VCA audio group volume | float |

### 🔑 Core API - BGM

| Method | Description | Return Type |
|------|------|--------|
| `PlayBGM(int)` | Play BGM | void |
| `StopBGM(STOP_MODE)` | Stop BGM | void |
| `ChangeBGM(int, STOP_MODE)` | Change BGM | void |
| `SetBGMPause(bool)` | Set BGM pause/play state | void |
| `GetBGMState()` | Get BGM state | AudioState |
| `SetBGMParameter(PARAMETER_ID, float, bool = false)` | Set BGM parameter (ID) | void |
| `SetBGMParameter(string, float, bool = false)` | Set BGM parameter (name) | void |
| `SetBGMParameters(PARAMETER_ID[], float[], int, bool = false)` | Set BGM parameters group | void |
| `SetBGMParameterWithLabel(PARAMETER_ID, string, bool = false)` | Set BGM parameter with label (ID) | void |
| `SetBGMParameterWithLabel(string, string, bool = false)` | Set BGM parameter with label (name) | void |
| `GetBGMParameter(string)` | Get BGM parameter value (name) | float |
| `GetBGMParameter(PARAMETER_ID)` | Get BGM parameter value (ID) | float |
| `SetBGMProperty(EVENT_PROPERTY, float)` | Set BGM property | void |
| `GetBGMProperty(EVENT_PROPERTY)` | Get BGM property value | float |

### 🔑 Core API - Untagged Audio

| Method | Description | Return Type |
|------|------|--------|
| `PlayUntaggedAudio(string, float = 1f)` | Play untagged audio | void |
| `PlayUntaggedAudio(EventReference, float = 1f)` | Play untagged audio | void |
| `PlayUntaggedAudio(string, Transform, float = 1f)` | Play untagged audio and set position | void |
| `PlayUntaggedAudio(EventReference, Transform, float = 1f)` | Play untagged audio and set position | void |
| `PlayUntaggedAudio(string, Transform, Rigidbody, float = 1f)` | Play untagged audio and attach to 3D object | void |
| `PlayUntaggedAudio(EventReference, Transform, Rigidbody, float = 1f)` | Play untagged audio and attach to 3D object | void |
| `PlayUntaggedAudio(string, Transform, Rigidbody2D, float = 1f)` | Play untagged audio and attach to 2D object | void |
| `PlayUntaggedAudio(EventReference, Transform, Rigidbody2D, float = 1f)` | Play untagged audio and attach to 2D object | void |
| `StopUntaggedAudio(string, STOP_MODE)` | Stop untagged audio | void |
| `StopUntaggedAudio(EventReference, STOP_MODE)` | Stop untagged audio | void |
| `StopAndReleaseUntaggedAudio(string, STOP_MODE)` | Stop and release untagged audio | void |
| `StopAndReleaseUntaggedAudio(EventReference, STOP_MODE)` | Stop and release untagged audio | void |
| `SetUntaggedAudioPaused(string, bool)` | Set untagged audio paused state | void |
| `SetUntaggedAudioPaused(EventReference, bool)` | Set untagged audio paused state | void |
| `SetUntaggedAudioVolume(string, float)` | Set untagged audio volume | void |
| `SetUntaggedAudioVolume(EventReference, float)` | Set untagged audio volume | void |
| `ClearStoppedUntaggedAudio()` | Release all stopped untagged audio | void |
| `StopAndReleaseAllUntaggedAudio(STOP_MODE)` | Stop and release all untagged audio | void |
| `SetUntaggedAudioProperty(string, EVENT_PROPERTY, float)` | Set untagged audio property | void |
| `SetUntaggedAudioProperty(EventReference, EVENT_PROPERTY, float)` | Set untagged audio property | void |
| `SetUntaggedAudioParameter(string, string, float, bool = false)` | Set untagged audio parameter (name) | void |
| `SetUntaggedAudioParameter(EventReference, string, float, bool = false)` | Set untagged audio parameter (name) | void |
| `SetUntaggedAudioParameter(string, PARAMETER_ID, float, bool = false)` | Set untagged audio parameter (ID) | void |
| `SetUntaggedAudioParameter(EventReference, PARAMETER_ID, float, bool = false)` | Set untagged audio parameter (ID) | void |
| `SetUntaggedAudioParameterWithLabel(string, string, string, bool = false)` | Set untagged audio parameter with label (name) | void |
| `SetUntaggedAudioParameterWithLabel(EventReference, string, string, bool = false)` | Set untagged audio parameter with label (name) | void |
| `SetUntaggedAudioParameterWithLabel(string, PARAMETER_ID, string, bool = false)` | Set untagged audio parameter with label (ID) | void |
| `SetUntaggedAudioParameterWithLabel(EventReference, PARAMETER_ID, string, bool = false)` | Set untagged audio parameter with label (ID) | void |
| `SetUntaggedAudioParameters(string, PARAMETER_ID[], float[], int, bool = false)` | Set untagged audio parameters group | void |
| `SetUntaggedAudioParameters(EventReference, PARAMETER_ID[], float[], int, bool = false)` | Set untagged audio parameters group | void |

### 🔑 Core API - Tagged Audio

| Method | Description | Return Type |
|------|------|--------|
| `CreateAudio(string, string, bool = true)` | Create audio | void |
| `CreateAudio(EventReference, string, bool = true)` | Create audio | void |
| `PlayTaggedAudio(string)` | Play tagged audio | void |
| `StopTaggedAudio(string, STOP_MODE)` | Stop tagged audio | void |
| `ReleaseTaggedAudio(string)` | Release tagged audio | void |
| `StopAndReleaseTaggedAudio(string, STOP_MODE)` | Stop and release tagged audio | void |
| `StopAndReleaseAllTaggedAudio(STOP_MODE)` | Stop and release all tagged audio | void |
| `AttachedTaggedAudio(string, Transform)` | Set tagged audio position | void |
| `AttachedTaggedAudio(string, Transform, Rigidbody)` | Attach tagged audio to 3D object | void |
| `AttachTaggedAudio(string, Transform, Rigidbody2D)` | Attach tagged audio to 2D object | void |
| `DetachTaggedAudio(string)` | Detach audio | void |
| `SetTaggedAudioPaused(string, bool)` | Set tagged audio paused state | void |
| `GetTaggedAudioPaused(string)` | Get tagged audio paused state | bool |
| `GetTaggedAudioStage(string)` | Get tagged audio state | PLAYBACK_STATE |
| `SetTaggedAudioVolume(string, float)` | Set tagged audio volume | void |
| `GetTaggedAudioVolume(string)` | Get tagged audio volume | float |
| `SetTaggedAudioProperty(string, EVENT_PROPERTY, float)` | Set tagged audio property | void |
| `GetTaggedAudioProperty(string, EVENT_PROPERTY)` | Get tagged audio property | float |
| `SetTaggedAudioParameter(string, PARAMETER_ID, float, bool = false)` | Set tagged audio parameter (ID) | void |
| `SetTaggedAudioParameter(string, string, float, bool = false)` | Set tagged audio parameter (name) | void |
| `SetTaggedAudioParameters(string, PARAMETER_ID[], float[], int, bool = false)` | Set tagged audio parameters group | void |
| `SetTaggedAudioParameterWithLabel(string, PARAMETER_ID, string, bool = false)` | Set tagged audio parameter with label (ID) | void |
| `SetTaggedAudioParameterWithLabel(string, string, string, bool = false)` | Set tagged audio parameter with label (name) | void |
| `GetTaggedAudioParameter(string, string)` | Get tagged audio parameter (name) | float |
| `GetTaggedAudioParameter(string, PARAMETER_ID)` | Get tagged audio parameter (ID) | float |
| `ReleaseAllStoppedTaggedAudios()` | Release all stopped tagged audio | void |

---

## <a id="utils">🛠️ Attributes and Utility Classes </a>

### 🔧 Custom Attributes

```csharp
// Foldout group (group display in Inspector)
[FoldOutGroup("Group Name", foldEverything: false)]
public int value;

// Read-only (non-editable in Inspector)
[ReadOnly]
public float currentSpeed;

// Scene index (display scene selection dropdown)
[SceneIndex]
public int mainMenuScene;
```

### 🔧 Static Utility Class - Utilities

**Entry**: `Utilities.[MethodName]`

#### 🔑 Core API

| Method | Description | Return Type |
|------|------|--------|
| `DelayInvoke(float, UnityAction)` | Delayed invoke (coroutine implementation) | Coroutine |
| `ConditionallyInvoke(Func<bool>, UnityAction)` | Conditionally trigger invoke (coroutine implementation) | Coroutine |
| `StopCoroutine(Coroutine)` | Stop specified coroutine | void |
| `StopAllCoroutines()` | Stop all coroutines | void |
| `AsyncDelayInvoke(float, UnityAction)` | Async delayed invoke | void |
| `DictionaryFilter<T1, T2>(Dictionary<T1, T2>, Func<T1, T2, bool>, Action<T1, T2> = null)` | Dictionary filter | Dictionary<T1, T2> |
| `ScenePathToName(string)` | Scene path to scene name | string |

#### 💡 Quick Examples

```csharp
using StarryFramework;

// Delayed invoke (execute after 3 seconds)
Coroutine delayCoroutine = Utilities.DelayInvoke(3.0f, () => 
{
    Debug.Log("Execute after 3 seconds");
});

// Conditional trigger (execute when player health <= 0)
Utilities.ConditionallyInvoke(() => playerHealth <= 0, () => 
{
    Debug.Log("Player died");
});

// Stop coroutine
Utilities.StopCoroutine(delayCoroutine);

// Async delayed invoke (doesn't depend on MonoBehaviour)
Utilities.AsyncDelayInvoke(2.0f, () => 
{
    Debug.Log("Async delay 2 seconds");
});

// Dictionary filter
Dictionary<int, string> dict = new Dictionary<int, string>
{
    {1, "Apple"}, {2, "Banana"}, {3, "Cherry"}
};
var filtered = Utilities.DictionaryFilter(dict, 
    (key, value) => key > 1,  // Keep items with key>1
    (key, value) => Debug.Log($"Remove: {key}-{value}")  // Callback on remove
);

// Scene path to name
string sceneName = Utilities.ScenePathToName("Assets/Scenes/GameScene.unity");
// Returns: "GameScene"
```

------

### 🏗️ Singleton Base Class - MonoSingleton

**Purpose**: Global unique, cross-scene persistent MonoBehaviour singleton base class

#### 🔑 Core API

| Member | Type | Description |
|------|------|------|
| `GetInstance()` | Static Method | Get singleton instance |
| `OnSingletonDestroy` | Static Property | Callback triggered when singleton is destroyed |

#### 💡 Usage Example

```csharp
using StarryFramework;

// 1. Create singleton class
public class GameManager : MonoSingleton<GameManager>
{
    public int score = 0;
    
    protected override void Awake()
    {
        base.Awake();  // Must call
        // Initialization code
    }
}

// 2. Use singleton
public class Player : MonoBehaviour
{
    void Start()
    {
        // Get singleton instance
        GameManager.GetInstance().score += 10;
        
        // Register destroy callback
        GameManager.OnSingletonDestroy = () => 
        {
            Debug.Log("GameManager destroyed");
        };
    }
}
```

#### ⚠️ Important Features

- ✅ **DontDestroyOnLoad**: Automatically persists cross-scene
- ✅ **Auto-create**: Automatically creates GameObject on first call
- ✅ **Prevent duplicates**: Prohibits multiple instances of same type
- ✅ **Scene lookup**: Prioritizes finding existing instances in scene

------

### 🎬 Scene Singleton Base Class - SceneSingleton

**Purpose**: Scene-unique, destroyed when scene switches MonoBehaviour singleton base class

#### 🔑 Core API

| Member | Type | Description |
|------|------|------|
| `GetInstance()` | Static Method | Get singleton instance |
| `OnSingletonDestroy` | Static Property | Callback triggered when singleton is destroyed |

#### 💡 Usage Example

```csharp
using StarryFramework;

// 1. Create scene singleton class
public class LevelManager : SceneSingleton<LevelManager>
{
    public int enemyCount = 0;
    
    protected override void Awake()
    {
        base.Awake();  // Must call
        // Initialization code
    }
}

// 2. Use scene singleton
public class Enemy : MonoBehaviour
{
    void Start()
    {
        // Get scene singleton instance
        LevelManager.GetInstance().enemyCount++;
    }
    
    void OnDestroy()
    {
        LevelManager.GetInstance().enemyCount--;
    }
}
```

#### ⚠️ Important Features

- ✅ **Scene lifecycle**: Destroyed when scene unloads
- ✅ **Auto-create**: Automatically creates GameObject on first call
- ✅ **Scene unique**: Only guarantees uniqueness within current scene
- ✅ **Scene lookup**: Prioritizes finding existing instances in scene

------

### 📋 MonoSingleton vs SceneSingleton Comparison

| Feature | MonoSingleton | SceneSingleton |
|------|------|------|
| Lifecycle | Global persistent (DontDestroyOnLoad) | Within scene (destroyed with scene) |
| Use Cases | GameManager, AudioManager, etc. global managers | LevelManager, EnemySpawner, etc. scene managers |
| Cross-scene | ✅ Retained | ❌ Destroyed |
| Performance | Resident in memory | Released on scene switch |

---

## <a id="best_pract">📝 Best Practices</a>

### ✅ Recommended Practices

```csharp
using UnityEngine;
using StarryFramework;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // 1. Load save
        Framework.SaveComponent.LoadData(0);
        
        // 2. Register event listeners (use named methods)
        Framework.EventComponent.AddEventListener("GameStart", OnGameStart);
        Framework.EventComponent.AddEventListener<int>("ScoreChanged", OnScoreChanged);
        
        // 3. Register object pool
        Framework.ObjectPoolComponent.Register<Enemy>("Prefabs/Enemy", 10f, 30f);
        
        // 4. Start auto-save
        Framework.SaveComponent.StartAutoSaveTimer();
    }
    
    void OnDestroy()
    {
        // Remove event listeners
        Framework.EventComponent.RemoveEventListener("GameStart", OnGameStart);
        Framework.EventComponent.RemoveEventListener<int>("ScoreChanged", OnScoreChanged);
    }
    
    void OnGameStart() { }
    void OnScoreChanged(int score) { }
}
```

### ❌ Common Mistakes

```csharp
// ❌ Wrong: Using Lambda expressions to add event listeners
Framework.EventComponent.AddEventListener("Test", () => Debug.Log("Test"));

// ❌ Wrong: Accessing PlayerData without loading save
int gold = Framework.SaveComponent.PlayerData.gold;  // Error!

// ❌ Wrong: Addressables resource not released
var handle = Framework.ResourceComponent.LoadAddressableAsync<Sprite>("Icon", null);
// Forgot to call ReleaseAddressableHandle(handle)

// ❌ Wrong: Loop trigger timer not manually deleted
Framework.TimerComponent.RegisterTriggerTimer(1f, OnTick, true);  // Loop but no reference saved
```

### ✅ Correct Practices

```csharp
// ✅ Correct: Use named methods
Framework.EventComponent.AddEventListener("Test", OnTest);
void OnTest() { Debug.Log("Test"); }

// ✅ Correct: Load save first
if (Framework.SaveComponent.LoadData(0))
{
    int gold = Framework.SaveComponent.PlayerData.gold;
}

// ✅ Correct: Release Addressables resources
var handle = Framework.ResourceComponent.LoadAddressableAsync<Sprite>("Icon", (sprite) =>
{
    image.sprite = sprite;
});
// Release at appropriate time
Framework.ResourceComponent.ReleaseAddressableHandle(handle);

// ✅ Correct: Save named trigger timer
Framework.TimerComponent.RegisterTriggerTimer(1f, OnTick, true, "TickTimer");
// Delete at appropriate time
Framework.TimerComponent.DeleteTriggerTimer("TickTimer");
```

---

