# StarryFramework API 速查手册

|[中文](API速查手册.md)|[English](API_QUICK_REFERENCE.md)|

---

## 📖 文档导航

**框架配置和快速开始请参阅**: [README.md](README.md)

**模块快速跳转**：
[Core](#core) | [Event](#event) | [Save](#save) | [Scene](#scene) | [Timer](#timer) | [FSM](#fsm) | [ObjectPool](#pool) | [Resource](#resource) | [UI](#ui) | [Audio](#audio) | [Utils](#utils)

[**最佳实践**](#best_pract)

---

## <a id="core">⚙️ Framework Core</a>

### 🔧基础用法

所有框架功能通过 `Framework` 静态类访问，需要引用 `StarryFramework` 命名空间：

```csharp
using StarryFramework;

public class Example : MonoBehaviour
{
    void Start()
    {
        // 触发事件
        Framework.EventComponent.InvokeEvent("GameStart");
        // 保存数据
        Framework.SaveComponent.SaveData("手动存档");
        // 加载场景
        Framework.SceneComponent.LoadScene("MainGame");
    }
}
```

### 🔧框架控制

```csharp
// 退出应用
Framework.ShutDown(ShutdownType.Quit);

// 重启框架
Framework.ShutDown(ShutdownType.Restart);
```

---

## <a id="event">📡 Event Module</a>

**入口**: `Framework.EventComponent`

### 🔑 核心API

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `AddEventListener(string, UnityAction)` | 添加无参事件监听 | void |
| `AddEventListener<T>(string, UnityAction<T>)` | 添加1参数监听 | void |
| `AddEventListener<T1,T2>(string, UnityAction<T1,T2>)` | 添加2参数监听 | void |
| `AddEventListener<T1,T2,T3>(...)` | 添加3参数监听 | void |
| `AddEventListener<T1,T2,T3,T4>(...)` | 添加4参数监听 | void |
| `RemoveEventListener(string, UnityAction)` | 移除无参事件监听 | void |
| `RemoveEventListener<T>(string, UnityAction<T>)` | 移除1参数监听 | void |
| `RemoveEventListener<T1,T2>(...)` | 移除2参数监听 | void |
| `RemoveEventListener<T1,T2,T3>(...)` | 移除3参数监听 | void |
| `RemoveEventListener<T1,T2,T3,T4>(...)` | 移除4参数监听 | void |
| `InvokeEvent(string)` | 触发无参事件 | void |
| `InvokeEvent<T>(string, T)` | 触发1参数事件 | void |
| `InvokeEvent<T1,T2>(string, T1, T2)` | 触发2参数事件 | void |
| `InvokeEvent<T1,T2,T3>(string, T1, T2, T3)` | 触发3参数事件 | void |
| `InvokeEvent<T1,T2,T3,T4>(...)` | 触发4参数事件 | void |
| `InvokeDelayedEvent(string, float, bool = false)` | 延迟触发无参事件 | void |
| `InvokeDelayedEvent<T>(string, T, float, bool = false)` | 延迟触发1参数事件 | void |
| `InvokeDelayedEvent<T1,T2>(string, T1, T2, float, bool = false)` | 延迟触发2参数事件 | void |
| `InvokeDelayedEvent<T1,T2,T3>(...)` | 延迟触发3参数事件 | void |
| `InvokeDelayedEvent<T1,T2,T3,T4>(...)` | 延迟触发4参数事件 | void |
| `ClearAllEventLinsteners(string)` | 清空指定事件所有监听 | void |
| `ClearEventListeners(string)` | 清空无参事件监听 | void |
| `ClearEventListeners<T>(string)` | 清空1参数事件监听 | void |
| `ClearEventListeners<T1,T2>(string)` | 清空2参数事件监听 | void |
| `ClearEventListeners<T1,T2,T3>(string)` | 清空3参数事件监听 | void |
| `ClearEventListeners<T1,T2,T3,T4>(string)` | 清空4参数事件监听 | void |
| `GetAllEventsInfo()` | 获取所有事件信息 | Dictionary<string, Dictionary<string, int>> |

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `LastEventName` | string | 最后触发的事件名 |
| `LastEventParam` | string | 最后触发的事件参数类型 |

---

## <a id="save">💾 Save Module</a>

**入口**: `Framework.SaveComponent`

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `PlayerData` | PlayerData | 玩家游戏数据（需先加载） |
| `GameSettings` | GameSettings | 游戏设置数据 |
| `DefaultDataIndex` | int | 默认存档索引 |
| `CurrentLoadedDataIndex` | int | 当前加载的存档索引 |
| `AutoSaveDataInterval` | float | 自动存档间隔时间 |
| `LastAutoSaveTime` | float | 上次自动存档时间 |
| `AutoSave` | bool | 是否启用自动存档 |
| `AutoSaveInfo` | string | 自动存档信息 |
| `SaveInfoList` | List<string> | 存档信息列表 |
| `DataInfoDic` | Dictionary<int, PlayerDataInfo> | 存档信息字典 |
| `PlayerDataLoaded` | bool | 玩家数据是否已加载 |
| `GameSettingsLoaded` | bool | 游戏设置是否已加载 |

### 🔑 核心API

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `CreateNewData(bool, string = "")` | 创建新存档 | void |
| `SaveData(string = "")` | 保存当前存档 | void |
| `SaveData(int, string = "")` | 保存到指定索引存档 | void |
| `LoadData()` | 加载默认存档 | bool |
| `LoadData(int)` | 加载指定索引存档 | bool |
| `LoadDataInfo()` | 加载默认存档信息 | PlayerDataInfo |
| `LoadDataInfo(int)` | 加载指定存档信息 | PlayerDataInfo |
| `UnloadData()` | 卸载当前存档 | bool |
| `DeleteData(int)` | 删除指定存档 | bool |
| `GetDataInfos()` | 获取所有存档信息 | List<PlayerDataInfo> |
| `StartAutoSaveTimer()` | 启动自动存档计时器 | void |
| `StopAutoSaveTimer()` | 停止自动存档计时器 | void |
| `SetSaveInfo(int)` | 设置存档注释索引 | void |
| `SetSaveInfo(string)` | 设置存档注释字符串 | void |

### 🧩 重要类

#### PlayerData（存档数据类）

**位置**: `StarryFramework/Runtime/Framework/Save Module/PlayerData.cs`

用户需在此文件中定义自己的存档数据结构。

```csharp
[Serializable]
public sealed class PlayerData
{
    public int test = 0;
    
    // 自动集成事件模块：bool字段可通过触发同名事件自动设为true
    public bool event1;
    
    // 支持List、Dictionary等复杂类型
    public List<string> inventoryList = new();
    public CustomData customData = new();
}
```

#### GameSettings（游戏设置类）

**位置**: `StarryFramework/Runtime/Framework/Save Module/GameSettings.cs`

用户需在此文件中定义游戏设置数据结构。

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

**入口**: `Framework.SceneComponent`

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `CurrentActiveScene` | Scene | 当前激活场景 |
| `SceneLoadedTime` | float | 场景加载时间戳 |
| `SceneTime` | float | 场景运行时长 |

### 🔑 核心API

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `LoadScene(int, UnityAction = null, bool = true)` | 加载场景（索引） | AsyncOperation |
| `LoadScene(string, UnityAction = null, bool = true)` | 加载场景（名称） | AsyncOperation |
| `UnloadScene(UnityAction = null)` | 卸载当前激活场景 | void |
| `UnloadScene(int, UnityAction = null, bool = true)` | 卸载场景（索引） | void |
| `UnloadScene(string, UnityAction = null, bool = true)` | 卸载场景（名称） | void |
| `ChangeScene(int, int = -1, UnityAction = null)` | 切换场景（索引） | AsyncOperation |
| `ChangeScene(string, string = "", UnityAction = null)` | 切换场景（名称） | AsyncOperation |
| `LoadSceneDefault(int, UnityAction = null)` | 默认淡入淡出加载场景（索引） | void |
| `LoadSceneDefault(string, UnityAction = null)` | 默认淡入淡出加载场景（名称） | void |
| `LoadSceneProgressBar(int, GameObject, UnityAction = null)` | 进度条加载（索引，GameObject） | void |
| `LoadSceneProgressBar(int, string, UnityAction = null)` | 进度条加载（索引，路径） | void |
| `LoadSceneProgressBar(string, GameObject, UnityAction = null)` | 进度条加载（名称，GameObject） | void |
| `LoadSceneProgressBar(string, string, UnityAction = null)` | 进度条加载（名称，路径） | void |
| `ChangeSceneDefault(int, int = -1, UnityAction = null)` | 默认淡入淡出切换场景（索引） | void |
| `ChangeSceneDefault(string, string = "", UnityAction = null)` | 默认淡入淡出切换场景（名称） | void |
| `ChangeSceneProgressBar(int, int, GameObject, UnityAction = null)` | 进度条切换（索引，GameObject） | void |
| `ChangeSceneProgressBar(int, int, string, UnityAction = null)` | 进度条切换（索引，路径） | void |
| `ChangeSceneProgressBar(string, string, GameObject, UnityAction = null)` | 进度条切换（名称，GameObject） | void |
| `ChangeSceneProgressBar(string, string, string, UnityAction = null)` | 进度条切换（名称，路径） | void |
| `ProcessCoroutine(AsyncOperation, LoadProgressBase, UnityAction<AsyncOperation>, float = 0f, float = 1f)` | 进度条协程 | IEnumerator |

### 🧩 重要基类

#### LoadProgressBase（场景加载进度条基类）

**位置**: `StarryFramework/Runtime/Framework/Scene Module/LoadProgressBase.cs`

自定义进度条UI需继承此类。

```csharp
public abstract class LoadProgressBase : MonoBehaviour
{
    public float speed = 0.05f;  // 进度条速度
    
    // 将进度值设置在UI组件上
    public abstract void SetProgressValue(float value);
    
    // 加载完成后的回调，需调用AllowSceneActivate()激活场景
    public abstract void BeforeSetActive(AsyncOperation asyncOperation);
    
    protected void AllowSceneActivate(AsyncOperation asyncOperation)
    {
        asyncOperation.allowSceneActivation = true;
    }
}
```

**示例实现**:

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

**入口**: `Framework.TimerComponent`

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `Timers` | List<Timer> | 所有计时器列表 |
| `TriggerTimers` | List<TriggerTimer> | 所有触发计时器列表 |
| `AsyncTimers` | List<AsyncTimer> | 所有异步计时器列表 |
| `ClearUnusedTriggerTimersInterval` | float | 清理未使用触发计时器间隔 |
| `ClearUnusedAsyncTimersInterval` | float | 清理未使用异步计时器间隔 |

### 🔑 核心API - Timer

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `RegisterTimer(bool = false, float = 0f, UnityAction = null)` | 注册匿名计时器 | ITimer |
| `RegisterTimer(string, bool = false, float = 0f)` | 注册非匿名计时器 | void |
| `DeleteTimer(ITimer)` | 回收匿名计时器 | void |
| `DeleteTimer(string)` | 回收非匿名计时器 | void |
| `BindUpdateAction(string, UnityAction)` | 绑定Update事件 | void |
| `GetTimerState(string)` | 查看计时器状态 | TimerState |
| `GetTimerTime(string)` | 查看计时器时间 | float |
| `PauseTimer(string)` | 暂停计时器 | void |
| `StopTimer(string)` | 停止计时器 | void |
| `ResumeTimer(string)` | 恢复计时器 | void |
| `StartTimer(string)` | 启动计时器 | void |
| `ResetTimer(string)` | 重置计时器 | void |

### 🔑 核心API - TriggerTimer

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `RegisterTriggerTimer(float, UnityAction, bool = false, string = "", bool = false)` | 注册触发计时器 | void |
| `DeleteTriggerTimer(string)` | 删除触发计时器 | void |
| `GetTriggerTimerState(string)` | 获得触发计时器状态 | TimerState |
| `PauseTriggerTimer(string)` | 暂停触发计时器 | void |
| `ResumeTriggerTimer(string)` | 恢复触发计时器 | void |
| `StopTriggerTimer(string)` | 停止触发计时器 | void |
| `StartTriggerTimer(string)` | 启动触发计时器 | void |
| `ClearUnnamedTriggerTimers()` | 清除所有匿名触发计时器 | void |

### 🔑 核心API - AsyncTimer

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `RegisterAsyncTimer(float, UnityAction, bool = false, string = "")` | 注册异步触发计时器 | void |
| `DeleteAsyncTimer(string)` | 删除异步触发计时器 | void |
| `GetAsyncTimerState(string)` | 获得异步触发计时器状态 | TimerState |
| `StartAsyncTimer(string)` | 启动异步触发计时器 | void |
| `StopAsyncTimer(string)` | 停止异步触发计时器 | void |
| `ClearUnnamedAsyncTimers()` | 清除所有匿名异步触发计时器 | void |

### 🧩 重要接口

#### ITimer（计时器接口）

**位置**: `StarryFramework/Runtime/Framework/Timer Module/ITimer.cs`

普通计时器返回此接口。

```csharp
public interface ITimer
{
    float Time { get; }           // 当前时间
    TimerState TimerState { get; }  // 计时器状态
    
    void BindUpdateAction(UnityAction action);  // 绑定更新回调
    void Pause();    // 暂停
    void Resume();   // 恢复
    void Start();    // 开始
    void Stop();     // 停止
    void Reset();    // 重置
}
```

**使用示例**:

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

**入口**: `Framework.FSMComponent`

### 🔑 核心API

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `GetFSMCount()` | 获取有限状态机数量 | int |
| `CreateFSM<T>(string, T, List<FSMState<T>>)` | 创建有限状态机（List） | IFSM<T> |
| `CreateFSM<T>(string, T, FSMState<T>[])` | 创建有限状态机（数组） | IFSM<T> |
| `DestroyFSM<T>(string)` | 注销有限状态机（名称） | void |
| `DestroyFSM<T>(IFSM<T>)` | 注销有限状态机（对象） | void |
| `HasFSM<T>(string)` | 查询是否拥有某状态机 | bool |
| `GetFSM<T>(string)` | 获得某状态机 | IFSM<T> |
| `GetAllFSMs()` | 获取所有状态机 | FSMBase[] |

### 🧩 重要基类

### FSMState\<T>（状态机状态基类）

**位置**: `StarryFramework/Runtime/Framework/FSM Module/FSMState.cs`

所有FSM状态需继承此类。

```csharp
public abstract class FSMState<T> where T : class
{
    protected internal virtual void OnInit(IFSM<T> fsm) { }         // 创建状态机时
    protected internal virtual void OnEnter(IFSM<T> fsm) { }        // 进入状态时
    protected internal virtual void OnUpdate(IFSM<T> fsm) { }       // 每帧更新
    protected internal virtual void OnLeave(IFSM<T> fsm, bool isShutdown) { }  // 离开状态时
    protected internal virtual void OnDestroy(IFSM<T> fsm) { }      // 注销状态机时
    
    // 切换状态
    protected internal virtual void ChangeState<S>(IFSM<T> fsm) where S : FSMState<T> { }
    protected internal virtual void ChangeState(IFSM<T> fsm, Type stateType) { }
}
```

**示例实现**:

```csharp
public class IdleState : FSMState<Enemy>
{
    protected internal override void OnEnter(IFSM<Enemy> fsm)
    {
        Debug.Log("进入Idle状态");
    }
    
    protected internal override void OnUpdate(IFSM<Enemy> fsm)
    {
        if (fsm.Owner.DetectedPlayer())
            ChangeState<AttackState>(fsm);
    }
    
    protected internal override void OnLeave(IFSM<Enemy> fsm, bool isShutdown)
    {
        Debug.Log("离开Idle状态");
    }
}
```

---

## <a id="pool">🎱 ObjectPool Module</a>

**入口**: `Framework.ObjectPoolComponent`

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `ObjectPools` | List<ObjectPoolProperty> | 对象池属性列表 |

### 🔑 核心API

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `Register<T>(float, float, string = "")` | 注册Object对象池 | void |
| `Register<T>(GameObject, float, float, GameObject = null, string = "")` | 注册GameObject对象池（GameObject） | void |
| `Register<T>(string, float, float, GameObject = null, string = "")` | 注册GameObject对象池（路径） | void |
| `Require<T>(string = "")` | 获取某个物体 | T |
| `Recycle<T>(T, string = "")` | 回收某个物体 | void |
| `SetLocked<T>(bool, string = "")` | 锁定或解除锁定对象池 | void |
| `ReleaseObject<T>(T, string = "")` | 释放某个物体 | void |
| `ReleaseAllUnused<T>(string = "")` | 释放所有未使用的物体 | void |
| `ReleaseAllObjects<T>(string = "")` | 释放所有物体 | void |
| `ReleasePool<T>(string = "")` | 释放对象池 | void |

### 🧩 重要基类

#### ObjectBase（对象池对象基类）

**位置**: `StarryFramework/Runtime/Framework/ObjectPool Module/ObjectBase.cs`

普通C#类对象池需继承此类。

```csharp
public abstract class ObjectBase : IObjectBase
{
    public float lastUseTime { get; set; }
    public bool inUse { get; set; }
    public bool releaseFlag { get; set; }
    
    public virtual void OnSpawn() { }      // 从池中取出时
    public virtual void OnUnspawn() { }    // 回收到池中时
    public virtual void OnRelease() { }    // 从池中释放时
}
```

**示例实现**:

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

#### GameObjectBase（GameObject对象池基类）

**位置**: `StarryFramework/Runtime/Framework/ObjectPool Module/GameObjectBase.cs`

GameObject对象池需继承此MonoBehaviour。

```csharp
public abstract class GameObjectBase : MonoBehaviour, IObjectBase
{
    public float lastUseTime { get; set; }
    public bool inUse { get; set; }
    public bool releaseFlag { get; set; }
    
    public virtual void OnSpawn() { }      // 从池中取出时
    public virtual void OnUnspawn() { }    // 回收到池中时
    public virtual void OnRelease() { }    // 从池中释放时
}
```

**示例实现**:

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

**入口**: `Framework.ResourceComponent`

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `State` | LoadState | 加载状态 |
| `Progress` | float | 加载进度 |
| `ResourcePath` | string | 资源路径 |
| `TargetType` | Type | 目标类型 |

### 🔑 核心API - Resources

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `LoadRes<T>(string, bool = false)` | 同步加载Resources资源 | T |
| `LoadAllRes<T>(string)` | 同步加载路径下所有资源 | T[] |
| `LoadResAsync<T>(string, UnityAction<T>, bool = false)` | 异步加载Resources资源 | ResourceRequest |
| `UnloadRes(Object)` | 卸载非GameObject资源 | void |
| `UnloadUnusedRes()` | 释放所有未使用的Resources资源 | void |

### 🔑 核心API - Addressables

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `LoadAddressable<T>(string, bool = false)` | 同步加载Addressable资源 | T |
| `LoadAddressableAsync<T>(string, UnityAction<T>, bool = false)` | 异步加载Addressable资源 | AsyncOperationHandle<T> |
| `InstantiateAddressable(string, Transform = null)` | 实例化Addressable GameObject | AsyncOperationHandle<GameObject> |
| `ReleaseAddressableHandle(AsyncOperationHandle)` | 释放Addressable句柄 | void |
| `ReleaseAddressableAsset<T>(T)` | 释放Addressable资源对象 | void |
| `ReleaseAddressableInstance(GameObject)` | 释放Addressable实例 | void |
| `ReleaseAllAddressableHandles()` | 释放所有Addressable句柄 | void |

---

## <a id="ui">🖼️ UI Module</a>

**入口**: `Framework.UIComponent`

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `UIGroupsDic` | Dictionary<string, UIGroup> | UI组字典 |
| `UIFormsCacheList` | LinkedList<UIForm> | UI窗体缓存列表 |

### 🔑 核心API - UIGroup

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `HasUIGroup(string)` | 检查是否存在UI组 | bool |
| `GetUIGroup(string)` | 获取指定UI组 | UIGroup |
| `GetAllUIGroups()` | 获取所有UI组 | UIGroup[] |
| `AddUIGroup(string)` | 添加UI组 | void |
| `RemoveUIGroup(string)` | 移除UI组 | void |

### 🔑 核心API - UIForm

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `HasUIForm(string)` | 检查是否存在UI窗体 | bool |
| `GetUIForm(string)` | 获取指定UI窗体 | UIForm |
| `OpenUIForm(string, string, bool)` | 打开UI窗体 | AsyncOperationHandle<UIForm> |
| `CloseUIForm(string)` | 关闭UI窗体（资源名） | void |
| `CloseUIForm(UIForm)` | 关闭UI窗体（对象） | void |
| `RefocusUIForm(string)` | 重新聚焦UI窗体（资源名） | void |
| `RefocusUIForm(UIForm)` | 重新聚焦UI窗体（对象） | void |
| `CloseAndReleaseAllForms()` | 关闭并释放所有UI窗体 | void |

### 🧩 重要接口/基类

#### UIFormLogic（UI生命周期接口）

**位置**: `StarryFramework/Runtime/Framework/UI Module/UIFormLogic.cs`

所有UI界面脚本必须实现此接口。

```csharp
public interface UIFormLogic
{
    void OnInit(GameObject uiPrefab);           // 资源加载时
    void OnRelease();                           // 资源释放时
    void OnOpen();                              // 界面打开时
    void OnClose(bool isShutdown);              // 界面关闭时
    void OnCover();                             // 界面被覆盖时
    void OnReveal();                            // 界面揭露显示时
    void OnPause();                             // 界面暂停时
    void OnResume();                            // 界面恢复时
    void OnUpdate();                            // 每帧更新
    void OnDepthChanged(int formCountInUIGroup, int depthInUIGroup);  // 深度改变时
    void OnRefocus();                           // 重新聚焦时
}
```

#### UguiForm（UI抽象基类）

**位置**: `StarryFramework/Runtime/Framework/UI Module/Examples/UguiForm.cs`

框架提供的UGUI实现示例，用户可继承使用。

```csharp
public abstract class UguiForm : MonoBehaviour, UIFormLogic
{
    // 提供淡入淡出等默认实现
    protected virtual void Awake() { }
    public virtual void OnInit(GameObject uiPrefab) { }
    public virtual void OnOpen() { }
    // ... 其他生命周期方法
}
```

---

## <a id="audio">🔊 Audio Module</a>

**入口**: `Framework.AudioComponent` (命名空间: `using StarryFramework.Extentions;`)

### 📊 属性

| 属性 | 类型 | 说明 |
|------|------|------|
| `CurrentBGM` | string | 当前BGM |
| `BGMState` | AudioState | BGM状态 |
| `CurrentBGMList` | List<EventReference> | 当前BGM列表 |

### 🔑 核心API - PlayOneShot

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `PlayOneShot(EventReference, Vector3 = default)` | 触发一次声音 | void |
| `PlayOneShot(string, Vector3 = default)` | 触发一次声音（路径） | void |
| `PlayOneShotAttached(EventReference, GameObject)` | 触发一次声音并附着 | void |
| `PlayOneShotAttached(string, GameObject)` | 触发一次声音并附着（路径） | void |

### 🔑 核心API - VCA

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `SetVolume(string, float)` | 设置VCA音频组音量 | void |
| `GetVolume(string)` | 获得VCA音频组音量 | float |

### 🔑 核心API - BGM

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `PlayBGM(int)` | 播放BGM | void |
| `StopBGM(STOP_MODE)` | 停止BGM | void |
| `ChangeBGM(int, STOP_MODE)` | 切换BGM | void |
| `SetBGMPause(bool)` | 设置BGM暂停/播放状态 | void |
| `GetBGMState()` | 获得BGM状态 | AudioState |
| `SetBGMParameter(PARAMETER_ID, float, bool = false)` | 设置BGM参数（ID） | void |
| `SetBGMParameter(string, float, bool = false)` | 设置BGM参数（名称） | void |
| `SetBGMParameters(PARAMETER_ID[], float[], int, bool = false)` | 设置BGM一组参数 | void |
| `SetBGMParameterWithLabel(PARAMETER_ID, string, bool = false)` | 用标签设置BGM参数（ID） | void |
| `SetBGMParameterWithLabel(string, string, bool = false)` | 用标签设置BGM参数（名称） | void |
| `GetBGMParameter(string)` | 获得BGM参数值（名称） | float |
| `GetBGMParameter(PARAMETER_ID)` | 获得BGM参数值（ID） | float |
| `SetBGMProperty(EVENT_PROPERTY, float)` | 设置BGM属性 | void |
| `GetBGMProperty(EVENT_PROPERTY)` | 获得BGM属性值 | float |

### 🔑 核心API - Untagged Audio

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `PlayUntaggedAudio(string, float = 1f)` | 播放未标记音频 | void |
| `PlayUntaggedAudio(EventReference, float = 1f)` | 播放未标记音频 | void |
| `PlayUntaggedAudio(string, Transform, float = 1f)` | 播放未标记音频并设置位置 | void |
| `PlayUntaggedAudio(EventReference, Transform, float = 1f)` | 播放未标记音频并设置位置 | void |
| `PlayUntaggedAudio(string, Transform, Rigidbody, float = 1f)` | 播放未标记音频并附着到3D物体 | void |
| `PlayUntaggedAudio(EventReference, Transform, Rigidbody, float = 1f)` | 播放未标记音频并附着到3D物体 | void |
| `PlayUntaggedAudio(string, Transform, Rigidbody2D, float = 1f)` | 播放未标记音频并附着到2D物体 | void |
| `PlayUntaggedAudio(EventReference, Transform, Rigidbody2D, float = 1f)` | 播放未标记音频并附着到2D物体 | void |
| `StopUntaggedAudio(string, STOP_MODE)` | 停止播放未标记音频 | void |
| `StopUntaggedAudio(EventReference, STOP_MODE)` | 停止播放未标记音频 | void |
| `StopAndReleaseUntaggedAudio(string, STOP_MODE)` | 停止并释放未标记音频 | void |
| `StopAndReleaseUntaggedAudio(EventReference, STOP_MODE)` | 停止并释放未标记音频 | void |
| `SetUntaggedAudioPaused(string, bool)` | 设置未标记音频暂停状态 | void |
| `SetUntaggedAudioPaused(EventReference, bool)` | 设置未标记音频暂停状态 | void |
| `SetUntaggedAudioVolume(string, float)` | 设置未标记音频音量 | void |
| `SetUntaggedAudioVolume(EventReference, float)` | 设置未标记音频音量 | void |
| `ClearStoppedUntaggedAudio()` | 释放所有已停止的未标记音频 | void |
| `StopAndReleaseAllUntaggedAudio(STOP_MODE)` | 停止并释放所有未标记音频 | void |
| `SetUntaggedAudioProperty(string, EVENT_PROPERTY, float)` | 设置未标记音频属性 | void |
| `SetUntaggedAudioProperty(EventReference, EVENT_PROPERTY, float)` | 设置未标记音频属性 | void |
| `SetUntaggedAudioParameter(string, string, float, bool = false)` | 设置未标记音频参数（名称） | void |
| `SetUntaggedAudioParameter(EventReference, string, float, bool = false)` | 设置未标记音频参数（名称） | void |
| `SetUntaggedAudioParameter(string, PARAMETER_ID, float, bool = false)` | 设置未标记音频参数（ID） | void |
| `SetUntaggedAudioParameter(EventReference, PARAMETER_ID, float, bool = false)` | 设置未标记音频参数（ID） | void |
| `SetUntaggedAudioParameterWithLabel(string, string, string, bool = false)` | 用标签设置未标记音频参数（名称） | void |
| `SetUntaggedAudioParameterWithLabel(EventReference, string, string, bool = false)` | 用标签设置未标记音频参数（名称） | void |
| `SetUntaggedAudioParameterWithLabel(string, PARAMETER_ID, string, bool = false)` | 用标签设置未标记音频参数（ID） | void |
| `SetUntaggedAudioParameterWithLabel(EventReference, PARAMETER_ID, string, bool = false)` | 用标签设置未标记音频参数（ID） | void |
| `SetUntaggedAudioParameters(string, PARAMETER_ID[], float[], int, bool = false)` | 设置未标记音频一组参数 | void |
| `SetUntaggedAudioParameters(EventReference, PARAMETER_ID[], float[], int, bool = false)` | 设置未标记音频一组参数 | void |

### 🔑 核心API - Tagged Audio

| 方法 | 说明 | 返回值 |
|------|------|--------|
| `CreateAudio(string, string, bool = true)` | 创建音频 | void |
| `CreateAudio(EventReference, string, bool = true)` | 创建音频 | void |
| `PlayTaggedAudio(string)` | 播放已标记音频 | void |
| `StopTaggedAudio(string, STOP_MODE)` | 停止已标记音频 | void |
| `ReleaseTaggedAudio(string)` | 释放已标记音频 | void |
| `StopAndReleaseTaggedAudio(string, STOP_MODE)` | 停止并释放已标记音频 | void |
| `StopAndReleaseAllTaggedAudio(STOP_MODE)` | 停止并释放所有已标记音频 | void |
| `AttachedTaggedAudio(string, Transform)` | 设置已标记音频位置 | void |
| `AttachedTaggedAudio(string, Transform, Rigidbody)` | 附着已标记音频到3D物体 | void |
| `AttachTaggedAudio(string, Transform, Rigidbody2D)` | 附着已标记音频到2D物体 | void |
| `DetachTaggedAudio(string)` | 取消音频附着 | void |
| `SetTaggedAudioPaused(string, bool)` | 设置已标记音频暂停状态 | void |
| `GetTaggedAudioPaused(string)` | 获得已标记音频暂停状态 | bool |
| `GetTaggedAudioStage(string)` | 获得已标记音频状态 | PLAYBACK_STATE |
| `SetTaggedAudioVolume(string, float)` | 设置已标记音频音量 | void |
| `GetTaggedAudioVolume(string)` | 获得已标记音频音量 | float |
| `SetTaggedAudioProperty(string, EVENT_PROPERTY, float)` | 设置已标记音频属性 | void |
| `GetTaggedAudioProperty(string, EVENT_PROPERTY)` | 获得已标记音频属性 | float |
| `SetTaggedAudioParameter(string, PARAMETER_ID, float, bool = false)` | 设置已标记音频参数（ID） | void |
| `SetTaggedAudioParameter(string, string, float, bool = false)` | 设置已标记音频参数（名称） | void |
| `SetTaggedAudioParameters(string, PARAMETER_ID[], float[], int, bool = false)` | 设置已标记音频一组参数 | void |
| `SetTaggedAudioParameterWithLabel(string, PARAMETER_ID, string, bool = false)` | 用标签设置已标记音频参数（ID） | void |
| `SetTaggedAudioParameterWithLabel(string, string, string, bool = false)` | 用标签设置已标记音频参数（名称） | void |
| `GetTaggedAudioParameter(string, string)` | 获得已标记音频参数（名称） | float |
| `GetTaggedAudioParameter(string, PARAMETER_ID)` | 获得已标记音频参数（ID） | float |
| `ReleaseAllStoppedTaggedAudios()` | 释放所有已停止的已标记音频 | void |

---

## <a id="utils">🛠️ 特性和工具类 </a>

### 🔧 自定义Attributes

```csharp
// 折叠组（在Inspector中分组显示）
[FoldOutGroup("组名", foldEverything: false)]
public int value;

// 只读（Inspector中不可编辑）
[ReadOnly]
public float currentSpeed;

// 场景索引（显示场景选择下拉列表）
[SceneIndex]
public int mainMenuScene;
```

### 🔧 静态工具类 - Utilities

**入口**: `Utilities.[方法名]`

#### 🔑 核心API

| 方法                                                         | 说明                         | 返回值             |
| ------------------------------------------------------------ | ---------------------------- | ------------------ |
| `DelayInvoke(float, UnityAction)`                            | 延时调用（协程实现）         | Coroutine          |
| `ConditionallyInvoke(Func<bool>, UnityAction)`               | 根据条件触发调用（协程实现） | Coroutine          |
| `StopCoroutine(Coroutine)`                                   | 停止指定协程                 | void               |
| `StopAllCoroutines()`                                        | 停止所有协程                 | void               |
| `AsyncDelayInvoke(float, UnityAction)`                       | 异步延时调用                 | void               |
| `DictionaryFilter<T1, T2>(Diction...2, bool>, Action<T1, T2> = null)` | 字典筛选器                   | Dictionary<T1, T2> |
| `ScenePathToName(string)`                                    | 场景路径转场景名             | string             |

#### 💡 快速示例

```csharp
using StarryFramework;

// 延时调用（3秒后执行）
Coroutine delayCoroutine = Utilities.DelayInvoke(3.0f, () => 
{
    Debug.Log("3秒后执行");
});

// 条件触发（当玩家血量<=0时执行）
Utilities.ConditionallyInvoke(() => playerHealth <= 0, () => 
{
    Debug.Log("玩家死亡");
});

// 停止协程
Utilities.StopCoroutine(delayCoroutine);

// 异步延时调用（不依赖MonoBehaviour）
Utilities.AsyncDelayInvoke(2.0f, () => 
{
    Debug.Log("异步延时2秒");
});

// 字典筛选
Dictionary<int, string> dict = new Dictionary<int, string>
{
    {1, "Apple"}, {2, "Banana"}, {3, "Cherry"}
};
var filtered = Utilities.DictionaryFilter(dict, 
    (key, value) => key > 1,  // 保留key>1的项
    (key, value) => Debug.Log($"移除: {key}-{value}")  // 移除时回调
);

// 场景路径转名称
string sceneName = Utilities.ScenePathToName("Assets/Scenes/GameScene.unity");
// 返回: "GameScene"
```

------

### 🏗️ 单例基类 - MonoSingleton

**用途**: 全局唯一、跨场景持久化的MonoBehaviour单例基类

#### 🔑 核心API

| 成员                 | 类型     | 说明                 |
| -------------------- | -------- | -------------------- |
| `GetInstance()`      | 静态方法 | 获取单例实例         |
| `OnSingletonDestroy` | 静态属性 | 单例销毁时触发的回调 |

#### 💡 使用示例

```csharp
using StarryFramework;

// 1. 创建单例类
public class GameManager : MonoSingleton<GameManager>
{
    public int score = 0;
    
    protected override void Awake()
    {
        base.Awake();  // 必须调用
        // 初始化代码
    }
}

// 2. 使用单例
public class Player : MonoBehaviour
{
    void Start()
    {
        // 获取单例实例
        GameManager.GetInstance().score += 10;
        
        // 注册销毁回调
        GameManager.OnSingletonDestroy = () => 
        {
            Debug.Log("GameManager被销毁");
        };
    }
}
```

#### ⚠️ 重要特性

- ✅ **DontDestroyOnLoad**: 自动跨场景持久化
- ✅ **自动创建**: 首次调用时自动创建GameObject
- ✅ **防重复**: 禁止同一类型多个实例
- ✅ **场景查找**: 优先查找场景中已存在的实例

------

### 🎬 场景单例基类 - SceneSingleton

**用途**: 场景内唯一、场景切换时销毁的MonoBehaviour单例基类

#### 🔑 核心API

| 成员                 | 类型     | 说明                 |
| -------------------- | -------- | -------------------- |
| `GetInstance()`      | 静态方法 | 获取单例实例         |
| `OnSingletonDestroy` | 静态属性 | 单例销毁时触发的回调 |

#### 💡 使用示例

```csharp
using StarryFramework;

// 1. 创建场景单例类
public class LevelManager : SceneSingleton<LevelManager>
{
    public int enemyCount = 0;
    
    protected override void Awake()
    {
        base.Awake();  // 必须调用
        // 初始化代码
    }
}

// 2. 使用场景单例
public class Enemy : MonoBehaviour
{
    void Start()
    {
        // 获取场景单例实例
        LevelManager.GetInstance().enemyCount++;
    }
    
    void OnDestroy()
    {
        LevelManager.GetInstance().enemyCount--;
    }
}
```

#### ⚠️ 重要特性

- ✅ **场景生命周期**: 随场景卸载而销毁
- ✅ **自动创建**: 首次调用时自动创建GameObject
- ✅ **场景唯一**: 仅在当前场景内保证唯一
- ✅ **场景查找**: 优先查找场景中已存在的实例

------

### 📋 MonoSingleton vs SceneSingleton 对比

| 特性     | MonoSingleton                         | SceneSingleton                         |
| -------- | ------------------------------------- | -------------------------------------- |
| 生命周期 | 全局持久（DontDestroyOnLoad）         | 场景内（随场景销毁）                   |
| 使用场景 | GameManager、AudioManager等全局管理器 | LevelManager、EnemySpawner等场景管理器 |
| 跨场景   | ✅ 保留                                | ❌ 销毁                                 |
| 性能     | 常驻内存                              | 场景切换时释放                         |

---

## <a id="best_pract">📝 最佳实践</a>

### ✅ 推荐做法

```csharp
using UnityEngine;
using StarryFramework;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // 1. 加载存档
        Framework.SaveComponent.LoadData(0);
        
        // 2. 注册事件监听（使用具名方法）
        Framework.EventComponent.AddEventListener("GameStart", OnGameStart);
        Framework.EventComponent.AddEventListener<int>("ScoreChanged", OnScoreChanged);
        
        // 3. 注册对象池
        Framework.ObjectPoolComponent.Register<Enemy>("Prefabs/Enemy", 10f, 30f);
        
        // 4. 启动自动存档
        Framework.SaveComponent.StartAutoSaveTimer();
    }
    
    void OnDestroy()
    {
        // 移除事件监听
        Framework.EventComponent.RemoveEventListener("GameStart", OnGameStart);
        Framework.EventComponent.RemoveEventListener<int>("ScoreChanged", OnScoreChanged);
    }
    
    void OnGameStart() { }
    void OnScoreChanged(int score) { }
}
```

### ❌ 常见错误

```csharp
// ❌ 错误：使用Lambda表达式添加事件监听
Framework.EventComponent.AddEventListener("Test", () => Debug.Log("Test"));

// ❌ 错误：未加载存档就访问PlayerData
int gold = Framework.SaveComponent.PlayerData.gold;  // 错误！

// ❌ 错误：Addressables资源未释放
var handle = Framework.ResourceComponent.LoadAddressableAsync<Sprite>("Icon", null);
// 忘记调用 ReleaseAddressableHandle(handle)

// ❌ 错误：循环触发计时器未手动删除
Framework.TimerComponent.RegisterTriggerTimer(1f, OnTick, true);  // 循环但未保存引用
```

### ✅ 正确做法

```csharp
// ✅ 正确：使用具名方法
Framework.EventComponent.AddEventListener("Test", OnTest);
void OnTest() { Debug.Log("Test"); }

// ✅ 正确：先加载存档
if (Framework.SaveComponent.LoadData(0))
{
    int gold = Framework.SaveComponent.PlayerData.gold;
}

// ✅ 正确：释放Addressables资源
var handle = Framework.ResourceComponent.LoadAddressableAsync<Sprite>("Icon", (sprite) =>
{
    image.sprite = sprite;
});
// 在适当时机释放
Framework.ResourceComponent.ReleaseAddressableHandle(handle);

// ✅ 正确：保存命名触发计时器
Framework.TimerComponent.RegisterTriggerTimer(1f, OnTick, true, "TickTimer");
// 在适当时机删除
Framework.TimerComponent.DeleteTriggerTimer("TickTimer");
```

---

