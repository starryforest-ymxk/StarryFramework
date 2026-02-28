# Save 模块 Provider 自动发现重构计划（无手动配置）

## 背景

当前 Save 模块通过 `SaveDataProviderAsset` + Inspector 拖拽的方式绑定自定义 `PlayerData` / `GameSettings`。该流程可用，但接入步骤偏多，并且容易产生场景配置遗漏。

本次重构目标是将 Provider 接入改为“特性自动发现”单路径，彻底移除手动配置链路，避免遗留技术债。

---

## 重构目标

1. 用户仅需实现 `ISaveDataProvider` 并添加特性即可生效。
2. 移除 `SaveDataProviderAsset` 与 `SaveSettings.SaveDataProvider` 手动配置方式。
3. Provider 选择规则确定、可复现、可诊断。
4. 修复 Provider 初始化时序，确保首次启动即按自定义类型加载。
5. 文档、示例、测试与实现同步完成，不保留兼容壳。

---

## 目标架构

1. 新增特性：`SaveDataProviderAttribute`
2. 新增解析器：`SaveDataProviderResolver`
3. Provider 来源唯一：实现 `ISaveDataProvider` 的普通类（非 ScriptableObject）
4. 选择优先级：代码注册（如后续需要）> 特性自动发现 > 内置 Provider 回退
5. 自动发现冲突处理：按 `Priority` 高到低选择；同优先级按 `Type.FullName` 字典序选择并记录警告

---

## 分阶段实施计划

### 阶段 0：基线冻结

工作项：
1. 记录当前 Save 核心流程行为基线（新建、读档、保存、自动存档、卸载、删除）。
2. 记录当前已知问题清单，作为回归比对。

验收标准：
1. 基线流程可重复执行。
2. 后续每阶段可与基线对比。

### 阶段 1：新增特性与解析器

工作项：
1. 新增 `SaveDataProviderAttribute`（至少包含 `Priority`）。
2. 新增 `SaveDataProviderResolver`，负责扫描、筛选、实例化、排序、选择。
3. 解析器增加合法性校验：
   - 必须实现 `ISaveDataProvider`
   - 非抽象、非泛型定义
   - 可实例化（无参构造）
   - `PlayerDataType` / `GameSettingsType` 不可为空

验收标准：
1. 无 Provider / 单 Provider / 多 Provider 场景均可稳定决议。
2. 决议结果具有确定性，不依赖程序集加载顺序。

### 阶段 2：接管 SaveManager 初始化

工作项：
1. `SaveManager` 在加载设置数据前先解析并锁定 Provider。
2. `LoadSetting()`、`LoadData()`、`SaveData()`统一从锁定 Provider 读取类型信息。
3. 启动时输出当前生效 Provider 类型日志（Debug 级）。

验收标准：
1. 首次启动即按自定义 `GameSettings` 类型读取。
2. 不再出现“首轮用内置类型，后续才切换”的时序问题。

### 阶段 3：彻底移除手动配置路径

工作项：
1. 删除 `SaveDataProviderAsset` 体系。
2. 从 `SaveSettings` 删除 `SaveDataProvider` 字段。
3. 从 `SaveComponentInspector` 删除 Provider 手动配置 UI。
4. 移除全部旧路径调用与分支逻辑，不保留过渡兼容层。

验收标准：
1. 全项目无 `SaveDataProviderAsset` 引用。
2. Save 模块不再依赖 Inspector 拖拽配置。

### 阶段 4：示例与测试迁移

工作项：
1. 将 `Assets/Test/SaveModule/CustomSaveDataProviderExample.cs` 改为“普通类 + 特性”示例。
2. 新增/更新自动化测试：
   - 单 Provider 自动发现
   - 多 Provider 优先级决议
   - 同优先级冲突决议
   - 无 Provider 回退
   - 自定义类型首次启动加载正确
   - Event 模块布尔字段联动仍可用

验收标准：
1. 测试全部通过。
2. Unity Console 不新增 Error/Warning（正常路径）。

### 阶段 5：清理与收口

工作项：
1. 清理旧 Provider 资产及失效引用（若存在）。
2. 清理旧文档描述、截图、教程文本。
3. 统一更新项目文档索引与 API 示例。

验收标准：
1. 无遗留旧入口说明。
2. 文档与代码实现一致。

---

## 风险与控制

1. 反射扫描性能风险  
控制：仅初始化扫描一次并缓存结果。

2. 多 Provider 冲突导致行为不确定  
控制：固定决议规则 + 明确冲突日志。

3. IL2CPP 裁剪导致 Provider 丢失  
控制：在文档中加入保留策略（`[Preserve]` / `link.xml`）并提供模板。

4. 破坏式重构引发旧项目升级成本  
控制：提供迁移指南与检查清单，一次性迁移，不长期维持双轨。

---

## 完成定义（DoD）

1. `SaveDataProviderAsset` 与相关字段、调用全部移除。
2. Provider 发现与选择逻辑单一且确定。
3. Save 初始化时序问题修复并验证通过。
4. 示例、测试、文档全部切换为自动发现方案。
5. 无 TODO/FIXME/临时兼容代码残留。

---

## 迁移检查清单（执行时逐项勾选）

1. 自定义 Provider 类已实现 `ISaveDataProvider` 并添加特性。
2. 自定义数据类型包含无参构造并可序列化。
3. 冷启动后 `GetPlayerData<T>()` / `GetGameSettings<T>()` 类型匹配。
4. 新建、保存、读档、删除、卸载流程均可用。
5. `OnLoadData` 触发后的事件布尔字段联动正常。

---

## 当前执行进度（2026-02-28）

1. 阶段 0：已完成（基线确认）
2. 阶段 1：已完成（特性与解析器已落地）
3. 阶段 2：已完成（SaveManager 启动时序已切换）
4. 阶段 3：已完成（手动配置入口已移除）
5. 阶段 4：已完成实现（示例、自动化测试、文档迁移已落地；待在 Unity Test Runner 执行通过）
6. 阶段 5：已完成（历史文档收口与 IL2CPP 保留指南已补齐）
