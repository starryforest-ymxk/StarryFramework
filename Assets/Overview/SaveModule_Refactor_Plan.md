# Save 模块数据模型解耦重构计划

## 背景与目标

当前存档系统将 `PlayerData` 与 `GameSettings` 内置在框架目录中。用户直接修改这两个类时，后续框架升级容易覆盖用户自定义字段。

本计划目标：

1. 框架负责存档流程，不再强绑定业务数据类。
2. 用户可在框架外定义自己的数据模型并稳定升级框架。
3. 迁移过程保持高兼容，避免一次性破坏现有项目。
4. 所有异常情况统一使用框架 Debug 日志，不抛异常阻断程序。

---

## 总体策略

采用**双轨兼容迁移**：

- 新增抽象数据入口（推荐新用法）
- 保留旧强类型入口（兼容旧项目）
- 分阶段迁移 Editor 和 Event 模块消费方式
- 最后在大版本再考虑移除旧 API

---

## 分阶段计划

### 阶段 1：建立抽象层（无破坏）

**目标**：新增能力，不影响旧代码。

**工作项**：

1. 在 Save 模块新增数据提供器抽象（如 `ISaveDataProvider`）
2. 提供默认实现（仍使用现有 `PlayerData` / `GameSettings`）
3. `SaveSettings` 增加 Provider 配置入口（为空时回退默认实现）
4. `SaveManager` 增加对象级存取入口（`object` 层）

**验收标准**：

- 旧项目不改代码可正常编译运行
- 存档读写行为与当前一致

---

### 阶段 2：运行时 API 双轨并存

**目标**：允许新项目不依赖内置类型。

**工作项**：

1. 在 `SaveComponent` 新增：
   - `GetPlayerData<T>()`
   - `GetGameSettings<T>()`
   - `GetPlayerDataObject()`
   - `GetGameSettingsObject()`
2. 旧属性 `PlayerData` / `GameSettings` 保留，作为兼容入口
3. `SaveManager` 内部序列化/反序列化改为走 provider 的类型信息

**验收标准**：

- 旧 API 可用
- 新 API 可用
- 存档文件结构保持兼容

---

### 阶段 3：Event 与 Inspector 解耦

**目标**：去除对具体类型的强依赖。

**工作项**：

1. `EventComponent` 从对象入口反射布尔字段，不再写死 `PlayerData` 类型
2. `SaveComponentInspector` 从对象入口反射字段并绘制，不再强依赖具体类
3. 反射失败、类型不匹配等异常分支仅记录框架日志

**验收标准**：

- 自动事件联动行为保持一致
- Inspector 运行时显示与编辑能力可用
- 无新增编译错误/控制台异常

---

### 阶段 4：文档与示例迁移

**目标**：统一对外推荐新用法。

**工作项**：

1. 更新文档中的存档示例为新 API
2. 保留旧 API 用法说明（标记兼容）
3. 更新测试示例，新增新 API 参考

**验收标准**：

- 新接入用户可不修改框架内置类即可扩展存档
- 文档与实现一致

---

### 阶段 5：弃用与清理（大版本）

**目标**：最终完成彻底解耦。

**工作项**：

1. 旧入口先标记 `[Obsolete]`（warning）
2. 提前一个版本公告移除计划
3. 大版本移除旧强类型入口

**验收标准**：

- 迁移说明完整
- 生态可平滑过渡

---

## 影响面（重点文件）

- `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs`
- `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveComponent.cs`
- `Assets/Plugins/StarryFramework/Runtime/Framework/Event Module/EventComponent.cs`
- `Assets/Plugins/StarryFramework/Editor/Inspector/SaveComponentInspector.cs`
- `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveSettings.cs`

---

## 风险与控制

1. **风险：旧项目编译中断**
   - 控制：阶段 1-3 不移除旧 API

2. **风险：历史存档读取失败**
   - 控制：保持文件命名与读取流程兼容，默认 provider 保底

3. **风险：Inspector 显示异常**
   - 控制：双轨期间保留旧入口，逐步切换反射对象来源

4. **风险：异常导致流程中断**
   - 控制：统一用框架 Debug 日志处理异常分支，不抛出阻断异常

---

## 执行与验收清单

每个阶段完成后都执行：

1. 清空 Unity Console
2. 刷新与编译
3. 检查 Error/Warning 为 0
4. 走一遍核心流程：
   - 创建存档
   - 保存存档
   - 读取存档
   - 删除存档
   - 自动存档
   - 卸载存档

---

## 当前建议

建议先执行**阶段 1 + 阶段 2**，先把扩展能力做出来并保持兼容；阶段 3 视需求推进，阶段 4/5 结合版本节奏完成。

---

## 当前执行进度（2026-02-28）

1. 阶段 1：已完成（抽象层与默认 provider 已落地）
2. 阶段 2：已完成（新旧 API 双轨并存）
3. 阶段 3：已完成首轮（Event/Inspector 已改为对象入口，异常分支走框架日志）
4. 阶段 4：已完成（文档示例、测试示例与外置 Provider 接入说明已同步）
5. 阶段 5：已启动预热（旧强类型入口已添加 `[Obsolete]` warning，暂不破坏兼容）
