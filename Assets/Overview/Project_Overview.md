# StarryFramework 项目概览文档

## 项目基本信息

**项目名称**: Traditional Chinese Medicine  
**Unity版本**: 2022.3  
**框架名称**: StarryFramework  
**框架版本**: 开源游戏开发框架  
**框架许可**: MIT License  
**渲染管线**: Built-in Render Pipeline  
**项目类型**: 2D游戏开发框架

---

## 项目简介

StarryFramework 是一个轻量化的Unity开发框架，提供了一系列开箱即用的方法和模块，旨在加快游戏开发速度、提高代码质量并保证项目的可维护性。框架采用MOM（Manager-Of-Managers）架构组织各个模块，实现模块间的零耦合设计，支持灵活的模块组合和扩展。

### 核心设计理念

1. **模块化设计**: 所有功能以独立模块形式存在，可根据项目需求自由组合
2. **零耦合架构**: 采用MOM架构，模块间通过事件系统通信，实现松耦合
3. **统一入口**: 通过`Framework`静态类提供统一的API入口
4. **易于扩展**: 支持自定义模块开发，可轻松集成新功能
5. **编辑器友好**: 为每个模块提供自定义Inspector面板，方便配置和调试

---

## 项目结构

### 顶层目录结构

```
/Assets
├── /StarryFramework           # 框架核心目录
│   ├── /Runtime               # 运行时代码
│   ├── /Editor                # 编辑器扩展代码
│   ├── /Extensions            # 框架扩展模块
│   └── /Info                  # 框架文档和资源
├── /Plugins                   # 第三方插件
│   ├── /Demigiant/DOTween     # 动画补间插件
│   ├── /FMOD                  # 音频中间件
│   └── /Borodar/RainbowFolders # 编辑器工具
├── /Test                      # 模块测试示例
├── /Overview                  # 项目文档目录
└── /Scenes                    # 游戏场景

```

### StarryFramework 详细结构

```
/Assets/StarryFramework
├── /Runtime                    # 运行时核心代码
│   ├── /Attributes             # 自定义特性
│   │   ├── FoldOutGroupAttribute.cs
│   │   ├── ReadOnlyAttribute.cs
│   │   └── SceneIndexAttribute.cs
│   ├── /Framework              # 框架模块实现
│   │   ├── /Base               # 基础类和接口
│   │   ├── /Static             # 静态类和枚举
│   │   ├── /Utilities          # 工具函数
│   │   ├── /Event Module       # 事件系统模块
│   │   ├── /Save Module        # 存档系统模块
│   │   ├── /Scene Module       # 场景管理模块
│   │   ├── /FSM Module         # 有限状态机模块
│   │   ├── /ObjectPool Module  # 对象池模块
│   │   ├── /Timer Module       # 计时器模块
│   │   ├── /Resource Module    # 资源管理模块
│   │   └── /UI Module          # UI管理模块
│   └── /Scene                  # 框架启动场景
│       └── GameFramework.unity
├── /Editor                     # 编辑器扩展
│   ├── /Inspector              # 自定义Inspector
│   ├── /Window                 # 编辑器窗口
│   ├── /Logic                  # 编辑器逻辑
│   └── /AttributesDrawer       # 特性绘制器
└── /Extensions                 # 扩展模块
    └── /Runtime
        └── /Audio Module       # FMOD音频扩展模块

```

---

## 核心架构

### 框架生命周期

StarryFramework 遵循严格的生命周期管理，确保模块按正确顺序初始化和销毁：

```
1. BeforeAwake   → 框架预初始化
2. Awake         → 组件注册、管理器创建
3. Init          → 模块初始化
4. AfterInit     → 初始化完成，进入运行时
5. Runtime       → 正常运行状态
6. Update        → 每帧更新（按优先级顺序）
7. BeforeShutDown → 准备关闭
8. ShutDown      → 清理资源、注销模块
```

### 核心类说明

#### 1. Framework（静态入口类）
- **位置**: `/Runtime/Framework/Static/Framework.cs`
- **功能**: 提供所有模块的统一访问入口
- **使用示例**:
```csharp
using StarryFramework;

Framework.EventComponent.InvokeEvent("GameStart");
Framework.SaveComponent.SaveData();
Framework.SceneComponent.LoadScene("MainGame");
```

#### 2. FrameworkManager（框架管理器）
- **位置**: `/Runtime/Framework/Base/FrameworkManager.cs`
- **功能**: 管理所有模块的生命周期、优先级和状态
- **特点**: 内部类，不对外暴露，通过BaseComponent间接调用

#### 3. MainComponent（主组件）
- **位置**: `/Runtime/Framework/Base/MainComponent.cs`
- **功能**: 框架的MonoBehaviour入口，挂载在GameFramework场景的根对象上
- **配置项**:
  - 帧率设置
  - 游戏速度
  - 后台运行
  - 启动场景
  - 模块列表和优先级

#### 4. BaseComponent（组件基类）
- **位置**: `/Runtime/Framework/Base/BaseComponent.cs`
- **功能**: 所有模块Component的基类，负责组件注册和生命周期

#### 5. IManager（管理器接口）
- **位置**: `/Runtime/Framework/Base/IManager.cs`
- **功能**: 定义所有Manager必须实现的接口
- **方法**: Awake、Init、Update、ShutDown、SetSettings

---

## 模块详解

### 1. Event Module（事件模块）

**核心文件**: `EventComponent.cs`, `EventManager.cs`

**功能特性**:
- 基于委托的事件系统
- 支持0-4个参数的泛型事件
- 支持延迟事件触发
- 与Save模块集成，自动触发存档事件

**主要API**:
```c#
Framework.EventComponent.AddEventListener(string eventName, UnityAction action)
Framework.EventComponent.AddEventListener<T>(string eventName, UnityAction<T> action)
Framework.EventComponent.InvokeEvent(string eventName)
Framework.EventComponent.InvokeDelayedEvent(string eventName, float delayTime)
Framework.EventComponent.ClearAllEventLinsteners(string eventName)
```

**使用场景**:
- UI交互响应
- 游戏逻辑解耦
- 系统间通信
- 成就和任务系统

**特殊功能**: 
- PlayerData中的布尔字段可通过触发同名事件自动设置为true（反射实现）

---

### 2. Save Module（存档模块）

**核心文件**: `SaveComponent.cs`, `SaveManager.cs`, `PlayerData.cs`, `GameSettings.cs`

**功能特性**:
- 自动存档和手动存档
- 多存档管理（创建、删除、覆盖）
- 存档注释和信息管理
- JSON格式存储玩家数据
- PlayerPrefs存储游戏设置

**主要API**:
```csharp
Framework.SaveComponent.CreateNewData(bool isNewGame, string note)
Framework.SaveComponent.SaveData(string note)
Framework.SaveComponent.LoadData(int index)
Framework.SaveComponent.DeleteData(int index)
Framework.SaveComponent.StartAutoSaveTimer()
Framework.SaveComponent.PlayerData
Framework.SaveComponent.GameSettings
```

**数据结构**:
- **PlayerData**: ScriptableObject，存储玩家游戏数据
- **GameSettings**: ScriptableObject，存储游戏设置（音量等）
- **PlayerDataInfo**: 存档元信息（时间、注释等）

**使用场景**:
- RPG游戏存档系统
- 关卡进度保存
- 玩家设置管理
- 成就系统数据持久化

---

### 3. Scene Module（场景管理模块）

**核心文件**: `SceneComponent.cs`, `SceneManager.cs`, `LoadProgressBase.cs`

**功能特性**:
- 场景加载、卸载、切换
- 场景过渡动画（淡入淡出）
- 自定义加载进度条
- 多异步操作进度统一管理
- 自动场景BGM管理（配合Audio模块）

**主要API**:
```csharp
Framework.SceneComponent.LoadScene(string sceneName)
Framework.SceneComponent.LoadSceneAsync(string sceneName)
Framework.SceneComponent.UnloadScene(string sceneName)
Framework.SceneComponent.LoadSceneWithAnimation(string sceneName, bool useProgressBar)
```

**特殊组件**:
- **SceneChangeCameraControl**: 用于渲染场景切换动画的相机控制
- **LoadProgressBase**: 自定义进度条的基类
- **ExampleLoadBar**: 示例进度条实现

**使用场景**:
- 关卡切换
- 场景预加载
- 开场动画
- 加载界面

---

### 4. FSM Module（有限状态机模块）

**核心文件**: `FSMComponent.cs`, `FSMManager.cs`, `FSM.cs`, `FSMState.cs`

**功能特性**:
- 泛型状态机支持
- 多状态机管理
- 状态参数系统
- 状态生命周期回调

**主要API**:
```csharp
Framework.FSMComponent.CreateFSM<T>(string name, T owner, List<FSMState<T>> states)
Framework.FSMComponent.DestroyFSM<T>(string name)
Framework.FSMComponent.GetFSM<T>(string name)
Framework.FSMComponent.HasFSM<T>(string name)
```

**状态生命周期**:
```csharp
OnInit(IFSM<T> fsm)      // 状态初始化
OnEnter(IFSM<T> fsm)     // 进入状态
OnUpdate(IFSM<T> fsm)    // 状态更新
OnLeave(IFSM<T> fsm)     // 离开状态
OnDestroy(IFSM<T> fsm)   // 状态销毁
ChangeState<S>(IFSM<T> fsm) // 切换状态
```

**使用场景**:
- AI行为控制
- 角色状态管理
- 游戏流程控制
- UI状态机

---

### 5. ObjectPool Module（对象池模块）

**核心文件**: `ObjectPoolComponent.cs`, `ObjectPoolManager.cs`, `ObjectPool.cs`

**功能特性**:
- GameObject对象池
- 普通类对象池
- 自动过期对象释放
- 对象池锁定机制
- 支持同类型多对象池（通过key区分）

**主要API**:
```csharp
Framework.ObjectPoolComponent.Register<T>(GameObject prefab, float autoReleaseInterval, float expireTime)
Framework.ObjectPoolComponent.Register<T>(float autoReleaseInterval, float expireTime, string key)
Framework.ObjectPoolComponent.Require<T>(string key)
Framework.ObjectPoolComponent.Recycle<T>(T obj, string key)
Framework.ObjectPoolComponent.ReleasePool<T>(string key)
```

**基类**:
- **ObjectBase**: 普通类对象池基类
- **GameObjectBase**: GameObject对象池基类
- **IObjectBase**: 对象池对象接口

**使用场景**:
- 子弹系统
- 特效管理
- 敌人生成
- UI元素复用

---

### 6. Timer Module（计时器模块）

**核心文件**: `TimerComponent.cs`, `TimerManager.cs`, `Timer.cs`, `TriggerTimer.cs`, `AsyncTimer.cs`

**功能特性**:
- 普通计时器（持续计时）
- 触发计时器（倒计时触发事件）
- 异步计时器（独立线程）
- 命名和匿名计时器
- 自动清理过期计时器
- 时间缩放支持

**主要API**:
```csharp
Framework.TimerComponent.RegisterTimer(string name, bool ignoreTimeScale, float startValue)
Framework.TimerComponent.GetTimerTime(string name)
Framework.TimerComponent.PauseTimer(string name)
Framework.TimerComponent.ResumeTimer(string name)
Framework.TimerComponent.RegisterTriggerTimer(float timeDelta, UnityAction action, bool repeat, string name)
Framework.TimerComponent.RegisterAsyncTimer(float timeDelta, UnityAction action, bool repeat, string name)
```

**计时器类型**:
1. **Timer**: 普通计时器，持续计时，可暂停/恢复
2. **TriggerTimer**: 倒计时触发事件，支持循环
3. **AsyncTimer**: 异步计时器，不受Unity时间控制

**使用场景**:
- 技能冷却
- Buff持续时间
- 倒计时功能
- 性能统计

---

### 7. Resource Module（资源管理模块）

**核心文件**: `ResourceComponent.cs`, `ResourceManager.cs`

**功能特性**:
- 同步/异步加载Resources资源
- 同步/异步加载Addressables资源
- GameObject自动实例化
- 资源卸载管理
- 加载进度追踪
- Addressable资源句柄管理

#### Resources加载API

用于加载项目中`Resources`文件夹下的资源：

```csharp
Framework.ResourceComponent.LoadRes<T>(string path, bool instantiate)
Framework.ResourceComponent.LoadAsync<T>(string path, UnityAction<T> callback, bool instantiate)
Framework.ResourceComponent.LoadAllRes<T>(string path)
Framework.ResourceComponent.Unload(UnityEngine.Object obj)
Framework.ResourceComponent.UnloadUnused()
```

**使用示例**:
```csharp
GameObject prefab = Framework.ResourceComponent.LoadRes<GameObject>("Prefabs/Player");
GameObject instance = Framework.ResourceComponent.LoadRes<GameObject>("Prefabs/Enemy", true);
Sprite[] sprites = Framework.ResourceComponent.LoadAllRes<Sprite>("Sprites/UI");

Framework.ResourceComponent.LoadAsync<AudioClip>("Audio/BGM", (clip) => 
{
    if (clip != null) Debug.Log($"Loaded: {clip.name}");
});
```

#### Addressables加载API

用于加载通过Addressables系统管理的资源：

```csharp
Framework.ResourceComponent.LoadAddressable<T>(string address, bool instantiate)
Framework.ResourceComponent.LoadAddressableAsync<T>(string address, UnityAction<T> callback, bool instantiate)
Framework.ResourceComponent.InstantiateAddressable(string address, Transform parent)
Framework.ResourceComponent.ReleaseAddressableHandle(AsyncOperationHandle handle)
Framework.ResourceComponent.ReleaseAddressableAsset<T>(T asset)
Framework.ResourceComponent.ReleaseAddressableInstance(GameObject instance)
Framework.ResourceComponent.ReleaseAllAddressableHandles()
```

**使用示例**:
```csharp
GameObject prefab = Framework.ResourceComponent.LoadAddressable<GameObject>("Player");

var handle = Framework.ResourceComponent.LoadAddressableAsync<AudioClip>("BGM_Main", (clip) => 
{
    if (clip != null) Debug.Log($"Loaded: {clip.name}");
});

var instanceHandle = Framework.ResourceComponent.InstantiateAddressable("Enemy", transform);

Framework.ResourceComponent.ReleaseAddressableHandle(handle);
```

**重要说明**:
- **Resources加载**: 适用于小型项目或原型开发，资源打包在应用中，无法动态更新
- **Addressables加载**: 适用于大型项目，支持资源热更新、按需加载、远程资源等高级功能
- 使用Addressables时，记得在不需要资源时调用Release方法释放资源，避免内存泄漏
- 框架会在ShutDown时自动释放所有未释放的Addressable句柄

**使用场景**:
- 预制体加载
- 音频资源加载
- 配置文件读取
- 纹理和材质加载
- 远程资源下载（Addressables）
- 资源热更新（Addressables）

---

### 8. UI Module（UI管理模块）

**核心文件**: `UIComponent.cs`, `UIManager.cs`, `UIForm.cs`, `UIGroup.cs`

**功能特性**:
- UI分组管理
- UI窗体生命周期管理
- UI层级控制
- UI暂停/恢复机制
- 基于Addressables的异步加载

**主要API**:
```csharp
Framework.UIComponent.AddUIGroup(string groupName)
Framework.UIComponent.OpenUIForm(string formAssetName, string groupName, bool pauseCovered)
Framework.UIComponent.CloseUIForm(string formAssetName)
Framework.UIComponent.RefocusUIForm(string formAssetName)
Framework.UIComponent.GetUIForm(string formAssetName)
```

**核心概念**:
- **UIGroup**: UI分组，管理同一类型的UI窗体
- **UIForm**: UI窗体，代表一个具体的UI界面
- **UIFormLogic**: UI逻辑基类，处理UI业务逻辑

**使用场景**:
- 主菜单系统
- HUD界面
- 弹窗管理
- UI状态管理

---

## 扩展模块

### Audio Module（音频扩展模块）

**位置**: `/Assets/StarryFramework/Extensions/Runtime/Audio Module`

**核心文件**: `AudioComponent.cs`, `AudioManager.cs`, `AudioSettings.cs`

**依赖**: FMOD Studio Unity Integration

**功能特性**:
- FMOD音频系统封装
- BGM自动管理
- 多音频通道管理（BGM、音效、UI音效）
- 音频动态附着到GameObject
- 场景BGM自动播放
- 音频预加载

**说明**: 
该模块为可选扩展，需要项目使用FMOD音频中间件。由于许可限制，FMOD不包含在框架分发版本中，需自行下载FMOD Unity Integration。

**使用场景**:
- 背景音乐管理
- 音效播放
- 3D空间音频
- 动态音频混合

---

## 工具和特性

### 自定义特性（Attributes）

#### 1. FoldOutGroupAttribute
```csharp
[FoldOutGroup("分组名称")]
public int value;
```
在Inspector中创建折叠分组，提高可读性

#### 2. ReadOnlyAttribute
```csharp
[ReadOnly]
public string debugInfo;
```
在Inspector中显示只读字段

#### 3. SceneIndexAttribute
```csharp
[SceneIndex]
public int sceneIndex;
```
在Inspector中提供场景选择下拉菜单

### 工具函数（Utilities）

**位置**: `/Runtime/Framework/Utilities/Utilities.cs`

**主要功能**:
```csharp
Utilities.DelayInvoke(float time, UnityAction action)
Utilities.ConditionallyInvoke(Func<bool> condition, UnityAction action)
Utilities.AsyncDelayInvoke(float time, UnityAction action)
Utilities.DictionaryFilter<T1, T2>(Dictionary<T1, T2> dic, Func<T1, T2, bool> filter)
Utilities.ScenePathToName(string scenePath)
```

### 单例基类

**MonoSingleton**: MonoBehaviour单例基类
**SceneSingleton**: 场景单例基类

---

## 编辑器工具

### 自定义Inspector

每个模块都有对应的自定义Inspector，位于`/Editor/Inspector`目录：

- **EventComponentInspector**: 显示当前注册的事件和监听器数量
- **SaveComponentInspector**: 显示存档信息、自动存档状态
- **TimerComponentInspector**: 显示所有计时器状态
- **FSMComponentInspector**: 显示状态机数量和状态
- **ObjectPoolComponentInspector**: 显示对象池信息
- **SceneComponentInspector**: 场景加载配置
- **UIComponentInspector**: UI组和窗体状态

### 设置窗口

**位置**: `/Editor/Window/SettingsWindow.cs`

提供框架全局设置的编辑器窗口

### 编辑器逻辑

**SceneSetupOnPlay**: 进入Play模式时自动设置场景

---

## 使用指南

### 快速开始

1. **导入框架**
   - 将StarryFramework导入Unity项目
   - 确保GameFramework场景的Build Index为0

2. **配置框架**
   - 打开GameFramework场景
   - 在MainComponent上配置启动场景和启用的模块
   - 根据需要配置各模块的Settings

3. **使用框架**
```csharp
using StarryFramework;

public class GameController : MonoBehaviour
{
    void Start()
    {
        Framework.EventComponent.AddEventListener("PlayerDeath", OnPlayerDeath);
        Framework.SaveComponent.LoadData();
    }
    
    void OnPlayerDeath()
    {
        Framework.SaveComponent.SaveData("玩家死亡存档");
        Framework.SceneComponent.LoadSceneWithAnimation("GameOver");
    }
}
```

### 创建自定义模块

1. 创建Manager类，继承IManager接口
2. 创建Component类，继承BaseComponent
3. 在Enums.cs中添加ModuleType枚举
4. 在Framework.cs中添加组件访问属性
5. 在MainComponent中添加到模块列表

---

## 最佳实践

### 1. 模块优先级设置

建议优先级顺序（从高到低）：
```
Scene → Event → Save → Resource → Timer → ObjectPool → FSM → UI → Audio
```

### 2. 事件命名规范

使用常量定义事件名，避免拼写错误：
```csharp
public static class GameEvents
{
    public const string PlayerSpawn = "PlayerSpawn";
    public const string EnemyDeath = "EnemyDeath";
    public const string LevelComplete = "LevelComplete";
}
```

### 3. 对象池使用建议

- 频繁创建销毁的对象使用对象池
- 合理设置过期时间和释放间隔
- 为不同用途的同类对象使用不同key

### 4. 存档数据组织

- PlayerData存储游戏进度数据
- GameSettings存储用户设置
- 使用布尔字段配合事件系统实现标志位

### 5. 状态机设计

- 保持状态单一职责
- 使用状态参数传递数据
- 合理使用状态生命周期回调

---

## 依赖和兼容性

### Unity包依赖

- **com.unity.addressables**: 1.21.21（UI模块）
- **com.unity.textmeshpro**: 3.0.9（UI文本）
- **com.unity.nuget.newtonsoft-json**: 3.2.1（JSON序列化）
- **com.unity.ugui**: 1.0.0（UI系统）

### 第三方插件

- **DOTween**: 动画补间插件（可选）
- **FMOD**: 音频中间件（Audio扩展模块必需）
- **RainbowFolders**: 编辑器美化工具（可选）

### 兼容性

- Unity 2022.3 及以上版本
- 支持所有平台（PC、移动、主机）
- Built-in渲染管线

---

## 测试和示例

### 测试场景

项目包含完整的测试示例，位于`/Assets/Test`目录：

- **TestEvent**: 事件系统测试
- **TestSave**: 存档系统测试
- **TestFSM**: 状态机测试
- **TestObjectPool**: 对象池测试
- **TestTimer**: 计时器测试
- **TestScene**: 场景管理测试
- **TestUI**: UI系统测试
- **TestAudio**: 音频系统测试

每个测试场景都包含对应的测试脚本和示例代码。

---

## 性能优化建议

1. **对象池**: 为频繁创建的GameObject使用对象池
2. **事件系统**: 及时移除不需要的事件监听器
3. **计时器**: 使用命名计时器方便管理，及时删除无用计时器
4. **资源管理**: 定期调用UnloadUnused释放未使用资源
5. **UI管理**: 合理使用UI暂停机制，避免不必要的更新

---

## 常见问题

### Q: 如何添加新的模块？
A: 参考"创建自定义模块"章节，实现IManager接口并继承BaseComponent。

### Q: 为什么一定要从GameFramework场景启动？
A: GameFramework是框架的入口场景，负责初始化所有模块和管理器，其他场景通过Scene模块动态加载。

### Q: 如何处理场景间的数据传递？
A: 使用Save模块的PlayerData或通过事件系统传递数据。

### Q: 对象池如何设置合适的参数？
A: autoReleaseInterval建议设置为60-120秒，expireTime根据对象使用频率设置，频繁使用可设置较大值。

### Q: 如何调试框架运行状态？
A: 在MainComponent中设置Debug Type，使用自定义Inspector查看各模块状态。

---

## 项目配置参考

### 推荐项目设置

```
Graphics:
- Color Space: Linear
- Anti Aliasing: 2x/4x Multi Sampling

Quality:
- VSync Count: Every V Blank
- Shadow Resolution: High

Player:
- API Compatibility Level: .NET Standard 2.1
- Scripting Backend: IL2CPP (发布版本)
```

### 标签配置

- **GameFramework**: 框架根对象标签
- **Player**: 玩家对象标签
- **MainCamera**: 主相机标签

### 层级配置

- **Default**: 默认层
- **UI**: UI层
- **TransparentFX**: 透明特效层
- **Ignore Raycast**: 忽略射线检测层

---

## 版本历史

当前使用版本的主要特性：
- MOM架构，模块化设计
- 8个核心模块 + 1个扩展模块
- 完整的编辑器工具支持
- 丰富的测试示例
- Unity 2022.3 LTS支持

---

## 许可证

StarryFramework使用MIT许可证，允许自由使用、修改和分发。

详见项目仓库: https://github.com/starryforest-ymxk/StarryFramework

---

## 技术支持

- **GitHub仓库**: https://github.com/starryforest-ymxk/StarryFramework
- **问题反馈**: GitHub Issues
- **文档**: README.md 和 README_EN.md

---

## 总结

StarryFramework 是一个成熟、易用、可扩展的Unity游戏开发框架。通过模块化设计和MOM架构，提供了事件、存档、场景、状态机、对象池、计时器、资源管理、UI管理等核心功能，并支持FMOD音频扩展。框架适用于各类Unity游戏项目，特别是需要完整系统架构的中大型项目。

本项目采用该框架作为基础，可以快速构建游戏系统，专注于游戏玩法和内容开发，而无需从零开始搭建底层架构。

---

*文档生成日期: 2024*  
*框架版本: StarryFramework*  
*Unity版本: 2022.3*
