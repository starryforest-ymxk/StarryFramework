# StarryFramework 下一步重构清单（Post-P2）

> 更新时间：2026-02-26  
> 背景：`P0 / P1 / P2` 已完成（见 `Assets/Overview/Framework_Refactor_Roadmap.md`），本文件用于记录下一阶段值得推进的重构项。

---

## 1. 当前结论（已完成基础）

- 已完成线程安全主线程派发（`AsyncTimer` / `AudioManager` 回调）
- 已完成模块注册显式映射（内置模块 + 扩展 Audio）
- 已完成配置接口拆分与配置校验集中化
- 已完成 `Save/Timer/UI/Audio` 真热更新
- 已完成 `SaveManager` 存档路径迁移到 `persistentDataPath`
- 已完成 `ConfigurableComponent` helper 基类（减少 `OnValidate/Awake` 模板代码）
- 已明确 `SceneSettings` 由 `SceneComponent` 本地消费

---

## 1.1 Status Update (2026-02-26, A1/A2)

- `A1` Completed: clarified `AudioSettings.clearUnusedAudioInterval` as seconds and converted to milliseconds inside `AudioManager` before assigning `System.Timers.Timer.Interval`.
- `A2` Completed (strict-mode path): `FrameworkManager` now defaults to strict module registration (`StrictModuleRegistration = true`), so manager type / component type legacy fallback is disabled unless explicitly turned off via `FrameworkManager.SetStrictModuleRegistration(false)`.

---

## 1.2 Status Update (2026-02-26, B3)

- `B3` Completed: extracted the default animated scene transition flow in `SceneComponent` into a shared coroutine template to reduce duplicated logic across `LoadSceneDefault(...)` and `ChangeSceneDefault(...)` overloads.

---

## 1.3 Status Update (2026-02-26, C1)

- `C1` Completed (inspector copy / encoding cleanup path): cleaned user-facing strings in `SettingsWindow` and module inspectors, standardized visible copy to English (ASCII), and removed remaining garbled/non-ASCII comments in the touched inspector files.

---

## 2. 下一步优先级（建议）

### A. 高优先（建议先做）

### A1. 修正 `AudioManager` 清理定时器配置单位语义（秒 / 毫秒）

#### 现状
- `AudioSettings.clearUnusedAudioInterval` 从 Inspector 看更像“秒”
- `AudioManager` 直接赋给 `System.Timers.Timer.Interval`
- `Timer.Interval` 实际单位是毫秒，存在语义错配风险

#### 涉及文件
- `Assets/StarryFramework/Extensions/Runtime/Audio Module/AudioSettings.cs`
- `Assets/StarryFramework/Extensions/Runtime/Audio Module/AudioManager.cs`

#### 建议方案
- 方案 1（推荐）：配置字段保持“秒”，在 `AudioManager` 内部统一换算为毫秒（`seconds * 1000`）
- 方案 2：字段重命名为毫秒（`...IntervalMs`），但需要处理 Inspector 兼容/迁移

#### 验收标准
- Inspector 输入 `120` 时，实际清理间隔为 120 秒
- 文案与代码单位语义一致

---

### A2. 去掉 `FrameworkManager` 字符串反射兼容回退（或加严格模式）

#### 现状
- 已有显式模块注册映射
- 但仍保留 `Type.GetType("StarryFramework." + managerType + "Manager")` 的兼容回退

#### 风险
- 自定义模块漏注册时，错误会延迟到运行时暴露
- 继续保留反射回退会弱化“显式注册”的约束价值

#### 涉及文件
- `Assets/StarryFramework/Runtime/Framework/Base/FrameworkManager.cs`

#### 建议方案
- 阶段 1：增加“严格模式”开关（默认开启），未注册直接报错
- 阶段 2：移除反射回退逻辑（保留清晰错误信息）

#### 验收标准
- 未注册模块在初始化阶段直接输出明确错误（包含模块类型）
- 内置模块与 Audio 扩展不受影响

---

### A3. `SaveManager` 存档迁移的可观测性与回归保障增强

#### 现状
- 已支持 `Application.dataPath/SaveData -> Application.persistentDataPath/SaveData` 的一次性迁移
- 已处理冲突文件重命名与跨卷移动

#### 仍可增强的点
- 迁移结果统计（迁移文件数、冲突数、失败数）
- 更清晰的日志（首次迁移 / 无需迁移 / 迁移失败）
- 最小回归测试方案（人工或自动）

#### 涉及文件
- `Assets/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs`

#### 建议方案
- 增加迁移结果结构（summary）
- 将迁移日志统一为一条摘要 + 必要 warning
- 补一份测试步骤文档（旧目录构造 -> 启动 -> 校验新目录）

#### 验收标准
- 能从日志快速判断迁移是否执行、迁移了多少文件
- 迁移失败时保留清晰错误上下文

---

## 3. 中优先（维护性提升明显）

### B1. `FrameworkSettingsValidator` 的结果展示增强（显示 Code / 修复建议）

#### 现状
- 校验器已具备 `Severity / Code / Message`
- Editor 窗口当前主要展示 `Message`

#### 涉及文件
- `Assets/StarryFramework/Runtime/Framework/Base/FrameworkSettingsValidator.cs`
- `Assets/StarryFramework/Editor/Window/SettingsWindow.cs`

#### 建议方案
- 展示格式改为 `[CODE] Message`
- 常见问题增加修复建议（例如模块缺失、模块重复）
- 可选：增加“自动修复”按钮（仅处理安全规则）

---

### B2. 配置“热更新能力”显式声明（哪些配置立即生效，哪些仅初始化生效）

#### 现状
- `Save/Timer/UI/Audio` 已支持真热更新
- `SceneSettings` 为组件本地配置
- 其他模块无独立 settings

#### 问题
- Inspector 修改配置时，开发者不容易判断是否立即生效

#### 建议方案
- 在各模块 Inspector 增加说明（Play 模式下是否即时生效）
- 或在 settings 类/文档中维护“热更新能力矩阵”

#### 建议文档输出
- `Assets/Overview/Module_Settings_HotReload_Matrix.md`（可选）

---

### B3. 提炼 `SceneComponent` 默认动画切场流程（减少四处重复）

#### 现状
- `LoadSceneDefault/ChangeSceneDefault` 的默认动画流程存在重复逻辑
- 已统一时间读取入口，但流程仍重复

#### 涉及文件
- `Assets/StarryFramework/Runtime/Framework/Scene Module/SceneComponent.cs`

#### 建议方案
- 抽取私有协程模板（例如：`RunDefaultSceneTransition(...)`）
- 统一处理：
  - `DefaultAnimationCanvas` 加载
  - Fade In/Out
  - `StartSceneLoadAnim/EndSceneLoadAnim` 事件
  - 回调串接

#### 验收标准
- 默认动画功能行为不变
- 重复代码明显减少，修改动画流程只改一处

---

## 4. 低优先（工程体验/产品化）

### C1. Inspector 文案与编码清理（中英混合/历史编码异常）

#### 现状
- 部分编辑器脚本字符串存在历史编码显示异常
- 影响维护体验，但不影响运行

#### 建议方案
- 统一编码（UTF-8）
- 清理乱码文案并统一术语（中英对照策略）

#### 涉及范围
- `Assets/StarryFramework/Editor/`
- `Assets/StarryFramework/Extensions/Editor/`

---

### C2. 为存档路径迁移补一份独立说明文档（面向团队协作）

#### 目的
- 说明新旧路径、迁移时机、冲突策略、排障步骤

#### 建议文档
- `Assets/Overview/Save_Storage_Migration_Notes.md`

---

## 5. 推荐实施顺序（Post-P2）

1. `A1` 音频清理间隔单位语义修正（低成本高收益）
2. `A2` 模块注册严格模式 / 去掉反射回退
3. `A3` 存档迁移可观测性增强
4. `B3` `SceneComponent` 默认动画流程提炼
5. `B1` 校验结果展示增强
6. `B2` 热更新能力矩阵与 Inspector 提示补齐
7. `C1 / C2` 工程体验与文档补全

---

## 6. 备注

- 本文件是“下一步 backlog”，不是一次性必须完成的任务列表。
- 建议每完成一项后，在本文件中追加 `Status Update`，避免后续重复评估。
