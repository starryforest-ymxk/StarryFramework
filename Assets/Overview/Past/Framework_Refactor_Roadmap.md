# StarryFramework 重构建议清单（Roadmap）

> 文档目的：整理当前框架中“值得重构”的部分，按优先级给出问题、风险、建议方案与落地顺序，便于后续迭代规划。  
> 整理日期：2026-02-26

---

## 1. 背景与范围

本清单基于当前框架配置系统与核心模块代码的实际实现整理，重点覆盖：

- 框架配置与模块配置的一致性
- 模块初始化/热更新语义
- 生命周期与注册机制的健壮性
- 存档、计时器、音频等模块的运行时稳定性

不包含：

- 具体游戏业务逻辑
- UI 美术或场景资源内容

---

## 2. 已完成的小修复（近期）

以下问题已完成修复，可作为后续重构的基础：

1. `SceneSetupOnPlay` 重复绑定回调问题（改为先解绑再绑定，确保幂等）
2. `Save / Timer / UI / Audio` 模块配置“真热更新”
   - 运行时通过 `OnValidate -> SetSettings(...)` 可以立即生效（而非仅保存引用）

---

## 2.1 Status Update (2026-02-26, P1)

- `P1-1` Completed: split config API (`IManager` + `IConfigurableManager`) and removed empty `SetSettings(...)` implementations from non-configurable managers.
- `P1-2` Completed: centralized `FrameworkSettings` default creation paths/modules and reused them from runtime/editor creation flows.
- `P1-3` Completed: added shared `FrameworkSettingsValidator` and reused it in runtime `SettingCheck()` and editor `SettingsWindow`.
- `P1-4` Completed: unified module `settings` null-safety/default initialization for `Scene/Save/Timer/UI/Audio`.
- `P1-5` Completed (scene-config boundary path): clarified `FrameworkSettings` scope vs module component settings in editor UI, and removed misleading `SceneComponent -> SceneManager.SetSettings(...)`.

---

## 2.2 Status Update (2026-02-26, P2)

- `P2-1` Completed: migrated save-file storage root from `Application.dataPath/SaveData` to `Application.persistentDataPath/SaveData`, added one-time legacy folder migration, and extracted save path helpers in `SaveManager`.
- `P2-2` Completed (low-risk helper-base path): added `ConfigurableComponent` helper base to remove duplicated `Awake/OnValidate` settings binding templates in `Save/Timer/UI/Audio` components.
- `P2-3` Completed (SceneComponent-local path): clarified `SceneSettings` is consumed locally by `SceneComponent`, centralized default animation timing reads inside `SceneComponent`, and added inspector messaging in `SceneComponentInspector`.

---

## 3. 重构优先级总览

### P0（优先处理：正确性/稳定性风险）

1. 线程模型与主线程安全（`AsyncTimer` / `AudioManager`）
2. 模块注册与激活机制去“字符串/名称约定”化

### P1（优先处理：一致性/维护性）

3. 配置接口与语义统一（`IManager` / `SetSettings`）
4. 配置默认值与创建逻辑去重
5. 配置校验逻辑集中化（Editor 与运行时共用）
6. 模块 `settings` 空引用防护与初始化一致性
7. 模块配置归属边界统一（全局配置 vs 场景组件配置）

### P2（中期优化：产品化与扩展性）

8. 存档路径迁移到 `Application.persistentDataPath`
9. 提炼“可配置组件基类”减少 `OnValidate` 模板代码
10. `Scene` 模块配置消费路径统一（`SceneComponent` / `SceneManager` 角色明确）

---

## 4. 详细重构项

### P0-1. 线程模型与主线程安全（高优先级）

#### 问题

- `AsyncTimer` 使用 `System.Timers.Timer`，回调线程不是 Unity 主线程。
- `AudioManager` 也使用 `System.Timers.Timer` 做清理逻辑。
- 当前代码中 `UnityAction` 在计时器回调里直接执行，存在跨线程调用 Unity API 的潜在风险。

#### 风险

- 非主线程访问 Unity 对象导致随机异常或隐蔽 bug
- 难复现、难定位，且与运行平台/时序有关

#### 建议方案

- 引入主线程派发机制（Main Thread Dispatcher）
- `AsyncTimer` 只在后台线程发信号，实际 `UnityAction` 放到主线程执行
- `AudioManager` 的清理逻辑如果涉及 Unity/FMOD API，统一回主线程执行

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Timer Module/AsyncTimer.cs`
- `Assets/StarryFramework/Extensions/Runtime/Audio Module/AudioManager.cs`

---

### P0-2. 模块注册/激活机制去脆弱化（高优先级）

#### 问题

- `FrameworkManager` 通过字符串拼接 + `Type.GetType(...)` 查找 Manager 类型
- `MainComponent` 通过子物体名称 `Enum.Parse(...)` 推断模块类型

这两处都依赖“命名严格正确”，对重命名、拆分命名空间、Prefab 调整比较脆弱。

#### 风险

- 场景对象改名后模块启停异常
- 类型名或命名空间变更后运行时找不到 Manager
- 错误在运行时暴露，定位成本高

#### 建议方案

- 建立显式映射表（`ModuleType -> ManagerType`）
- 给模块组件增加显式 `ModuleType` 声明（或在 `BaseComponent` 子类内固定返回）
- 启停逻辑改为读取组件自身的模块类型，而不是读 GameObject 名称

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Base/FrameworkManager.cs`
- `Assets/StarryFramework/Runtime/Framework/Base/MainComponent.cs`

---

### P1-1. 配置接口与语义统一（`SetSettings`）

#### 问题

- `IManager` 强制所有模块实现 `SetSettings(IManagerSettings settings)`
- 但多个模块的 `SetSettings` 实际为空实现（`Event/FSM/ObjectPool/Resource/SceneManager`）
- 容易让人误以为所有模块都支持配置注入

#### 风险

- 接口语义不清晰
- 新模块实现者容易照抄空实现
- 调试时误判“配置已注入”

#### 建议方案

- 拆分接口：
  - `IManager`：只保留生命周期
  - `IConfigurableManager<TSettings>`：仅给可配置模块实现
- 明确 `SetSettings` 与 `ApplySettings` 的职责（存引用 vs 立即生效）

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Base/IManager.cs`
- 各模块 `*Manager.cs`

---

### P1-2. 配置默认值与创建逻辑去重

#### 问题

`FrameworkSettings` 默认模块列表在多个地方重复定义，修改时容易漏改。

#### 风险

- 不同入口创建出的默认配置不一致
- 后续新增模块时容易遗漏

#### 建议方案

- 提取统一默认配置工厂方法，例如：
  - `FrameworkSettings.CreateWithDefaults()`
  - `FrameworkSettings.ApplyDefaultModules()`
- `FrameworkSettings` 菜单和 `SettingsWindow` 共用同一入口

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Base/FrameworkSettings.cs`
- `Assets/StarryFramework/Editor/Window/SettingsWindow.cs`

---

### P1-3. 配置校验逻辑集中化

#### 问题

- 一部分校验在 `FrameworkSettings.SettingCheck()`
- 一部分校验在 `SettingsWindow.DrawValidationMessages()`
- 校验规则分散，逻辑重复，结果展示与运行时行为可能不一致

#### 风险

- Editor 显示通过，但运行时仍报错（或相反）
- 修改规则时容易遗漏某一处

#### 建议方案

- 抽出统一校验器（例如 `FrameworkSettingsValidator`）
- 返回结构化结果（Error/Warning + message + code）
- `SettingsWindow` 负责显示，`MainComponent`/运行时负责执行与日志

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Base/FrameworkSettings.cs`
- `Assets/StarryFramework/Editor/Window/SettingsWindow.cs`

---

### P1-4. 模块 `settings` 空引用防护与初始化一致性

#### 问题

- `Save/Scene/Timer/UI` 的 `settings` 字段没有默认实例
- `Audio` 的 `settings` 则有 `new()` 默认值
- 风格不一致，场景/Prefab 丢失配置时容易出现空引用或行为不明确

#### 风险

- 某些模块在运行时因配置为空报错
- 不同模块的默认行为不一致

#### 建议方案

- 统一约定：
  - 所有模块 `settings` 均提供安全默认实例；或
  - 所有模块在 `Awake` 前统一执行空值填充
- 对关键配置输出清晰日志（一次性）

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Save Module/SaveComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/Scene Module/SceneComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/Timer Module/TimerComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs`
- `Assets/StarryFramework/Extensions/Runtime/Audio Module/AudioComponent.cs`

---

### P1-5. 模块配置归属边界统一（全局 vs 场景）

#### 问题

- `FrameworkSettings` 是全局 `ScriptableObject`
- 模块细项配置大多挂在场景中的模块组件上
- 导致“切换全局设置资产”并不能切换完整模块细项配置

#### 风险

- 多环境配置（开发/测试/正式）切换不完整
- 团队协作时配置来源不清晰

#### 建议方案（两种路线二选一）

1. 全局化路线（更适合框架产品）
- 模块细项配置也纳入 `FrameworkSettings`（直接字段或引用子 `ScriptableObject`）

2. 场景化路线（更适合项目灵活配置）
- 保留模块组件配置，但文档明确“全局配置只控制框架层，不覆盖模块细项”

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Base/FrameworkSettings.cs`
- 各模块 `*Component.cs` / `*Settings.cs`

---

### P2-1. 存档路径迁移（`dataPath` -> `persistentDataPath`）

#### 问题

当前 `SaveManager` 直接使用 `Application.dataPath` 进行读写，不利于发布环境与平台兼容。

#### 风险

- 部分平台无写权限或行为不稳定
- 发布版路径语义不符合 Unity 常规实践

#### 建议方案

- 统一迁移到 `Application.persistentDataPath`
- 增加一次性迁移策略（若旧路径存在数据则搬迁）
- 将存档根路径抽出为可测试方法（便于单元测试）

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs`

---

### P2-2. 提炼“可配置组件基类”减少重复代码

#### 问题

多个模块组件都有相似的 `OnValidate()` 逻辑（运行时把 `settings` 下发给 manager）。

#### 风险

- 复制粘贴导致行为不一致
- 新模块接入时容易漏掉热更新逻辑

#### 建议方案

- 引入通用基类或工具方法，例如：
  - `ConfigurableComponent<TManager, TSettings>`
  - `ApplyManagerSettingsInPlayMode()`

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Save Module/SaveComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/Timer Module/TimerComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs`
- `Assets/StarryFramework/Extensions/Runtime/Audio Module/AudioComponent.cs`

---

### P2-3. `Scene` 模块配置消费路径统一（`SceneComponent` / `SceneManager`）

#### 问题

- `SceneSettings` 是模块配置，但主要由 `SceneComponent` 直接消费（默认动画时间）
- `SceneManager.SetSettings(...)` 为空实现

#### 风险

- 设计语义不一致（“Manager 配置”与“Component 配置”混用）
- 后续扩展 Scene 配置时易混乱

#### 建议方案

- 明确职责并文档化：
  - 路线 A：`SceneSettings` 完全归 `SceneComponent`
  - 路线 B：`SceneSettings` 交由 `SceneManager` 统一持有与消费

建议先选 A（低成本），把空的 `SetSettings` 误导性降到最低。

#### 涉及文件（示例）

- `Assets/StarryFramework/Runtime/Framework/Scene Module/SceneComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/Scene Module/SceneManager.cs`

---

## 5. 推荐实施顺序（建议）

### 阶段 1：稳定性优先（短期）

1. 线程模型与主线程安全（`AsyncTimer` / `AudioManager`）
2. 模块注册与激活机制显式映射化

### 阶段 2：配置系统一致性（中短期）

3. 配置接口拆分（`IManager` / `IConfigurableManager`）
4. 配置默认值创建逻辑去重
5. 配置校验集中化
6. 模块 `settings` 默认值/空引用防护统一

### 阶段 3：产品化与长期维护（中期）

7. 配置归属边界统一（全局 or 场景）
8. 存档路径迁移与数据迁移策略
9. 可配置组件基类抽象
10. `Scene` 模块配置消费路径统一

---

## 6. 重构执行时的通用要求（建议）

- 每次重构只改一类问题，避免同时改架构与功能逻辑
- 优先保持 API 兼容，必要时提供过渡层
- 重构后补充：
  - 最小运行时验证步骤
  - 对应文档更新（`Overview`）
  - Inspector 行为验证（Play 模式热更新 / 非 Play 编辑）

---

## 7. 可作为后续文档的拆分方向（可选）

后续如果要继续细化，可拆成以下专题文档：

- `Configuration_System_Refactor_Plan.md`
- `Module_Registration_Refactor_Plan.md`
- `Save_Module_Storage_Migration_Plan.md`
- `Timer_Audio_Threading_Safety_Plan.md`

---

## 8. 备注

本清单是“路线图”而不是一次性任务列表。建议每完成一个阶段，回写本文件中的“状态/结论”，避免重复讨论。
