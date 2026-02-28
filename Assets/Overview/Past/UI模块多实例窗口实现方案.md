# UI模块多实例窗口实现方案（详细版）

整理日期：2026-02-26  
适用范围：`StarryFramework` UI 模块（`UIComponent / UIManager / UIGroup / UIForm / UIComponentInspector`）  
目标：在保持现有项目兼容的前提下，支持同资源名 UI 窗口多实例能力（包括跨 Group、多 Group、单 Group 多开）。

---

## 1. 方案前置决策（已确定）

### 决策 1：旧字符串 API 的多实例语义

结论：`是`，定义为“最上层实例（Topmost Instance）”。

适用 API：
- `GetUIForm(string assetName)`
- `CloseUIForm(string assetName)`
- `RefocusUIForm(string assetName)`

语义定义：
- 当同资源名存在多个实例时，上述 API 操作“该资源名当前最上层的活跃实例”。
- “最上层”在管理器层面的定义为：最近一次 `Open` 或 `Refocus` 的该资源名实例（见本文 `Topmost 判定策略`）。

说明：
- 该定义兼容旧调用方式，不会强迫现有业务立即迁移到 `serialId` API。
- 需要精确控制指定实例时，必须使用新增的 `serialId` 级 API。

### 决策 2：默认打开策略（兼容行为）

结论：`是`，默认保持 `SingleInstanceGlobal`（同资源名全局单实例）。

说明：
- 现有业务代码不需要改动即可维持当前行为。
- 多实例能力通过新增重载/选项显式启用，避免隐式回归。

---

## 2. 目标与非目标

### 目标

1. 在不破坏现有 UI 缓存正确语义（缓存只存已关闭窗体）的前提下支持多实例
2. 支持以下业务场景：
- 全局多实例（同资源名在多个 Group、同一 Group 都可多开）
- 跨 Group 多实例（每个 Group 1 个实例，但不同 Group 可同时存在）
- 单 Group 多实例（在同一个 Group 内连续多开同资源名）
3. 保持现有字符串 API 可用，并给出明确的多实例语义
4. 提供 `serialId` 级精确控制 API，避免歧义
5. 支持异步打开场景下的重复请求去重（仅在单实例策略下）

### 非目标（本期不做）

1. 不改动 UI 业务层具体面板脚本逻辑（如 `TestUIPanel`）
2. 不引入全新的 UI 资源管理系统
3. 不修改 Addressables 加载模式本身
4. 不实现“跨进程/跨场景持久化窗口句柄”

---

## 3. 术语与核心概念

### 资源名（Asset Name）

- 指 `OpenUIForm` 使用的 `uiFormAssetName`
- 同一资源名可生成多个实例（取决于打开策略）

### 实例（UI Form Instance）

- 指一个实际存在的 `UIForm` 对象 + 对应实例化的 UI GameObject
- 每个实例都有唯一的实例 ID（`serialId`）

### 打开策略（Open Policy）

- 决定“同资源名重复打开”时是去重还是创建新实例
- 决定“异步打开中重复请求”是否复用同一个句柄

### Topmost Instance（最上层实例）

- 用于定义旧字符串 API 的行为
- 管理器级语义，不直接等同于实际 Canvas 渲染顺序（除非引入 Group 优先级）

---

## 4. 打开策略设计（支持的模式）

## 4.1 推荐策略枚举（Phase 1）

建议新增枚举：`UIOpenPolicy`

候选值：
- `SingleInstanceGlobal`
- `SingleInstancePerGroup`
- `MultiInstanceGlobal`

### 策略语义

1. `SingleInstanceGlobal`（默认兼容）
- 同资源名全局只允许 1 个活跃实例
- 重复打开同名资源时去重并返回已有实例（或正在打开的句柄）

2. `SingleInstancePerGroup`（支持“跨 Group 多实例”）
- 同资源名在同一 Group 内只允许 1 个活跃实例
- 不同 Group 可各有 1 个同名实例

3. `MultiInstanceGlobal`（支持“全局多实例”和“单 Group 多实例”）
- 同资源名可在任意 Group 重复打开多个实例
- 同一 Group 内也可多开
- 不做实例去重（但仍可对资源加载层做缓存）

## 4.2 用户提出的三类场景与策略映射

1. 全局多实例
- 使用 `MultiInstanceGlobal`

2. 跨 Group 多实例（每 Group 单实例）
- 使用 `SingleInstancePerGroup`

3. 单 Group 多实例（同一 Group 内多开）
- 使用 `MultiInstanceGlobal`，并将 `uiGroupName` 固定为同一组

说明：
- “单 Group 多实例”不是必须单独新增策略；在 `MultiInstanceGlobal` 下天然可实现。
- 如果后续确实需要“仅允许在某一个指定 Group 内多开，跨 Group 不允许同名实例”，可在 Phase 2 增加扩展策略。

## 4.3 策略混用规则（请求策略优先，已确定）

核心规则：
- 每次 `OpenUIForm(...)` 都只按“当前请求携带的 `UIOpenPolicy`”进行判断与处理。
- 打开策略不会追溯修改已经存在的活跃实例（不会自动迁移 Group、不会自动合并、不会自动关闭旧实例）。
- 因此，同一资源名在不同时间使用不同策略是允许的，最终状态由“历史请求序列”决定。

### 混用示例（用户提问场景）

场景：
1. `OpenUIForm(Form1, GroupA, ..., SingleInstanceGlobal)`
2. `OpenUIForm(Form1, GroupB, ..., MultiInstanceGlobal)`

期望行为（本方案定义）：
- 第 1 次请求创建 `Form1#A`（位于 `GroupA`）
- 第 2 次请求按 `MultiInstanceGlobal` 规则执行，不做实例去重
- 创建新实例 `Form1#B`（位于 `GroupB`）
- 最终同时存在 `Form1#A` 与 `Form1#B`

### 单实例策略请求遇到“历史多实例”时的处理约定

由于允许策略混用，可能出现“当前请求是单实例策略，但系统里已存在多个同名实例”的情况（这些实例由历史 `MultiInstanceGlobal` 请求产生）。

约定如下（避免引入隐式清理副作用）：
- `SingleInstanceGlobal` 请求：
  - 返回同资源名的 `Topmost` 活跃实例
  - 输出 `Warning` 日志提示当前存在多个同名实例（历史多实例状态）
  - 不自动合并/关闭其他同名实例
- `SingleInstancePerGroup` 请求：
  - 若目标 Group 内存在多个同名实例（历史多实例状态），返回该 Group 内 `Topmost` 实例
  - 输出 `Warning` 日志
  - 不自动合并/关闭该 Group 内其他同名实例

设计理由：
- 保持“请求策略优先”语义简单、稳定、可预期
- 避免一次打开请求隐式销毁其他窗口，引发业务副作用
- 将“收敛实例数量”的责任交给业务显式调用关闭 API

---

## 5. 对外 API 设计（兼容 + 新增）

## 5.1 保持兼容的旧 API（默认策略不变）

保留：
- `OpenUIForm(string assetName, string groupName, bool pauseCoveredUIForm)`
- `GetUIForm(string assetName)`
- `CloseUIForm(string assetName)`
- `RefocusUIForm(string assetName)`

兼容行为：
- `OpenUIForm(string, string, bool)` 默认等价于 `UIOpenPolicy.SingleInstanceGlobal`
- 字符串 `Get/Close/Refocus` 在多实例模式下作用于“最上层实例”

## 5.1.1 当前实现中的关闭 API（基线现状）

当前 `UIComponent` 对外已有关闭相关 API：
- `void CloseUIForm(string uiFormAssetName)`（当前单实例实现；多实例方案中定义为关闭 Topmost）
- `void CloseUIForm(UIForm uiForm)`（按实例对象关闭）
- `void CloseAndReleaseAllForms()`（关闭并释放全部 UI）

现状问题（在多实例场景下）：
- 缺少“关闭同一资源名所有实例”的 API
- 缺少 `serialId` 级精确关闭 API（多实例下必须）
- 缺少按 `InstanceKey` 批量/精确关闭 API（业务层需要时会反复手写循环）

结论：
- 在多实例方案中，`CloseAllUIForms(string assetName)` 应从“可选”升级为“强烈建议且视为必需”的 API。

## 5.2 新增打开 API（定案：方案 B 选项对象）

本方案定案：
- 采用 `OpenUIForm(OpenUIFormOptions options)` 作为主 API
- 保留旧重载作为兼容入口，内部转发到 `options`

说明：
- 方案 A（简单重载）保留为历史备选，不作为本期主方案

### 方案 A：重载（简单）

```csharp
AsyncOperationHandle<UIForm> OpenUIForm(
    string uiFormAssetName,
    string uiGroupName,
    bool pauseCoveredUIForm,
    UIOpenPolicy openPolicy
)
```

### 方案 B：选项对象（更可扩展，推荐）

```csharp
public sealed class OpenUIFormOptions
{
    public string AssetName;
    public string GroupName;
    public bool PauseCoveredUIForm = true;
    public UIOpenPolicy OpenPolicy = UIOpenPolicy.SingleInstanceGlobal;
    public bool RefocusIfExists = true;     // 单实例策略命中时是否自动聚焦
    public string InstanceKey = null;       // 预留：Phase 2 单实例按业务 key
}

AsyncOperationHandle<UIForm> OpenUIForm(OpenUIFormOptions options)
```

推荐：
- 本方案对外主 API 为 `OpenUIForm(OpenUIFormOptions)`
- 保留旧重载并内部转发到 `options`

## 5.3 新增实例级精确控制 API（必须）

新增：
- `bool HasUIForm(int serialId)`
- `UIForm GetUIForm(int serialId)`
- `void CloseUIForm(int serialId)`
- `void RefocusUIForm(int serialId)`

意义：
- 在多实例模式下避免字符串 API 歧义
- 业务层可保存 `serialId` 做精确控制

## 5.4 新增集合查询 API（建议）

新增：
- `UIForm[] GetUIForms(string assetName)`（全部活跃实例）
- `UIForm[] GetUIForms(string assetName, string groupName)`（按组过滤）
- `UIForm[] GetUIFormsByInstanceKey(string assetName, string instanceKey)`（按业务键过滤）
- `int GetUIFormCount(string assetName)`（便于业务做限流）
- `void CloseAllUIForms(string assetName)`（强烈建议，视为必需）
- `UIForm GetTopUIForm(string assetName)`（明确旧字符串 API 的内部实现）
- `UIForm GetUIForm(string assetName, string instanceKey)`（按业务键取 Topmost）
- `bool HasUIForm(string assetName, string instanceKey)`（按业务键判断存在性）

## 5.5 关闭 API 扩展设计（多实例场景）

### 必须新增（多实例可用性）

- `void CloseUIForm(int serialId)`  
  说明：多实例下的主关闭路径，避免字符串 API 歧义。

- `void CloseAllUIForms(string assetName)`  
  说明：关闭同一资源名的所有活跃实例（跨 Group）。  
  必要性：旧 `CloseUIForm(string)` 在多实例下已定义为关闭 Topmost 实例，无法替代“关闭全部同名实例”。

### 强烈建议新增（常用业务场景）

- `void CloseUIForm(string assetName, string instanceKey)`  
  说明：关闭同一资源名下、指定 `InstanceKey` 的 Topmost 实例。

- `void CloseAllUIFormsInGroup(string assetName, string groupName)`  
  说明：关闭指定 Group 内该资源名的所有活跃实例（跨 Group 多实例场景常用）。
  备注：由于 C# 方法签名冲突（`string, string`），实现中使用显式命名 `InGroup`。

- `void CloseAllUIFormsByInstanceKey(string assetName, string instanceKey)`（可选，但推荐）  
  说明：关闭指定资源名 + `InstanceKey` 的所有活跃实例（用于历史多实例状态收敛）。
  备注：由于 C# 方法签名冲突（`string, string`），实现中使用显式命名 `ByInstanceKey`。

### 行为约定（统一关闭语义）

- 所有 `CloseAll...` 方法只作用于“活跃实例”，不直接操作缓存项。
- 关闭后统一走现有缓存策略：
  - 可缓存则入缓存
  - 否则释放
- `CloseAll...` 内部应使用快照遍历或显式节点迭代，避免 `OnClose` 回调重入导致集合修改异常。

## 5.6 旧 API 过时标记策略（`Obsolete`）

在多实例方案中，为了降低误用风险，建议对一部分旧 API 打上 `Obsolete` 标记，并通过替代 API 明确多实例语义。

当前实现状态（2026-02-26）：
- 第一批与第二批 `Obsolete` 均已按 warning 级（`error=false`）接入。

### 第一批（建议立即标记 `Obsolete`）

这些 API 在多实例时代表达能力不足，或语义容易被误解为“唯一实例操作”。

1. `OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)`
- 原因：无法传入 `UIOpenPolicy`、`InstanceKey`
- 替代：`OpenUIForm(OpenUIFormOptions options)`

2. `GetUIForm(string uiFormAssetName)`
- 原因：多实例下语义变为 `Topmost`，名称容易让调用方误解为“唯一实例”
- 替代：
  - `GetTopUIForm(string assetName)`
  - `GetUIForm(int serialId)`
  - `GetUIForms(string assetName)`
  - `GetUIForm(string assetName, string instanceKey)`（新增后）

3. `CloseUIForm(string uiFormAssetName)`
- 原因：多实例下仅关闭 `Topmost` 实例，容易误以为会关闭全部同名实例
- 替代：
  - `CloseUIForm(int serialId)`
  - `CloseAllUIForms(string assetName)`
  - `CloseUIForm(string assetName, string instanceKey)`

4. `RefocusUIForm(string uiFormAssetName)`
- 原因：多实例下仅聚焦 `Topmost` 实例，语义不够明确
- 替代：
  - `RefocusUIForm(int serialId)`
  - `RefocusUIForm(string assetName, string instanceKey)`（建议新增）

### 第二批（建议在 `serialId`/`InstanceKey` API 稳定后标记）

这些 API 仍可工作，但在缓存复用和会话级实例语义下，长期持有 `UIForm` 对象引用存在误用风险。

5. `CloseUIForm(UIForm uiForm)`
- 风险：业务层长期持有 `UIForm` 引用时，可能在缓存复用后误操作新会话实例
- 替代：`CloseUIForm(int serialId)`

6. `RefocusUIForm(UIForm uiForm)`
- 风险同上
- 替代：`RefocusUIForm(int serialId)`

### 不建议标记 `Obsolete`（继续保留）

以下 API 在多实例语义下仍然清晰，保留成本低且兼容价值高：

- `HasUIGroup(string uiGroupName)`
- `GetUIGroup(string uiGroupName)`
- `GetAllUIGroups()`
- `AddUIGroup(string uiGroupName)`
- `RemoveUIGroup(string uiGroupName)`
- `CloseAndReleaseAllForms()`
- `HasUIForm(string uiFormAssetName)`（语义定义为“是否存在任一活跃实例”）

补充建议：
- 在保留 `HasUIForm(string)` 的同时，新增：
  - `HasUIForm(int serialId)`
  - `HasUIForm(string assetName, string instanceKey)`（建议）

### 标记策略建议（迁移友好）

1. 先使用警告级过时标记（`error = false`）
- 让现有业务在不阻断编译的情况下逐步迁移

2. 在文档和日志中明确替代 API
- 尤其是：
  - `OpenUIForm(OpenUIFormOptions)`
  - `CloseUIForm(int serialId)`
  - `CloseAllUIForms(string assetName)`

3. 待项目完成主要业务迁移后，再考虑升级为错误级过时标记（`error = true`）

### 过时标记文案示例（建议）

```csharp
[Obsolete("Use OpenUIForm(OpenUIFormOptions) to specify UIOpenPolicy and InstanceKey.", false)]
```

```csharp
[Obsolete("Use CloseUIForm(int serialId) or CloseAllUIForms(string assetName) in multi-instance mode.", false)]
```

---

## 6. 数据结构改造方案（UIManager 内部）

现状问题：
- 当前主要按 `assetName` 在各 `UIGroup` 中遍历查找
- 这会导致多实例场景下性能和语义都不清晰

目标：
- `serialId` 成为内部主索引
- `assetName` 只作为二级索引和缓存复用键

## 6.1 建议新增索引

在 `UIManager` 中新增：

1. 实例主索引（活跃实例）
- `Dictionary<int, UIForm> activeFormsBySerial`

2. 按资源名索引（活跃实例集合）
- `Dictionary<string, HashSet<int>> activeSerialsByAsset`

3. 按资源名 + Group 索引（活跃实例集合）
- `Dictionary<string, Dictionary<string, HashSet<int>>> activeSerialsByAssetAndGroup`

说明：
- 也可以用 `struct` 组合键替代嵌套字典（更稳健）
- 组合键方案在后续扩展 `InstanceKey` 时更容易统一

4. 打开中请求索引（仅单实例策略去重）
- `Dictionary<UIOpenRequestKey, AsyncOperationHandle<UIForm>> openingRequests`

其中 `UIOpenRequestKey` 建议包含：
- `AssetName`
- `GroupName`（按策略决定是否参与）
- `OpenPolicy`
- `InstanceKey`（预留）

## 6.2 Topmost 判定索引（用于旧字符串 API）

新增 `long focusSequence`（管理器递增序列）

在 `UIForm` 增加字段：
- `long LastFocusSequence`

更新时机：
- `OnOpen` 后更新
- `OnRefocus` 后更新

用途：
- `GetTopUIForm(string assetName)` 从同名活跃实例中取 `LastFocusSequence` 最大者

说明：
- 这是“管理器语义上的最上层”，不一定等于实际渲染层级。
- 若后续需要“渲染一致的全局最上层”，可在 Phase 2 引入 `UIGroup` 优先级（或 Canvas Sorting 配置）。

---

## 7. `serialId` 语义建议（避免未来技术债）

多实例支持后，建议明确 `serialId` 的语义为：
- `UI 窗口实例会话 ID`（Session ID）

原因：
- 窗体对象可能来自缓存复用，同一个 `UIForm` 对象会经历多次打开/关闭
- 如果 `serialId` 在缓存复用后不变，业务层持有旧 ID 可能误操作重开的新会话

## 7.1 推荐实现方式

### 方案（推荐）

- 每次 `Open`（包括缓存复用）都分配新的 `serialId`
- `UIForm` 增加用于缓存对象内部追踪的私有对象 ID（可选，仅调试）

效果：
- 关闭后保存的旧 `serialId` 不会误命中新会话
- 多实例场景下实例句柄语义更直观、更安全

### 兼容说明

- 这是内部行为增强，不会破坏旧字符串 API
- 会影响 Inspector 中 `serialId` 在缓存复用后的表现（这是预期变化）

如果暂时不想改 `serialId` 语义：
- 也可以保留现状，但建议至少新增 `generation` 概念做安全校验

---

## 8. 打开流程设计（按策略）

## 8.1 打开流程总览

统一入口步骤：

1. 参数校验（asset/group）
2. 解析 `UIOpenPolicy`
2.1 应用“请求策略优先”规则（不追溯修改已有实例）
3. 生成“去重匹配键”（仅单实例策略）
4. 检查已打开实例（active 索引）
5. 检查打开中请求（opening 索引）
6. 尝试从缓存取可复用实例
7. Addressables 加载并实例化新对象
8. 绑定打开上下文（group/pause policy）
9. 分配（或刷新）实例 `serialId`
10. 加入 `UIGroup` 并刷新层级
11. 注册 active 索引 / focusSequence
12. 返回句柄

## 8.2 单实例策略匹配规则

### `SingleInstanceGlobal`

匹配条件：
- 任意 Group 中只要存在同资源名活跃实例即命中

处理：
- 返回已有实例（可选自动 `Refocus`）
- 如果存在打开中请求则返回同一请求句柄
- 如果因历史 `MultiInstanceGlobal` 已存在多个同名实例：返回 `Topmost` 实例并输出 `Warning`，不自动清理其他实例

### `SingleInstancePerGroup`

匹配条件：
- 指定 `uiGroupName` 内存在同资源名活跃实例即命中

处理：
- 返回该 Group 中已有实例（可选自动 `Refocus`）
- 打开中请求按 `(assetName, groupName)` 去重
- 如果因历史 `MultiInstanceGlobal` 导致目标 Group 内已有多个同名实例：返回该 Group `Topmost` 实例并输出 `Warning`

### `MultiInstanceGlobal`

匹配条件：
- 不做实例去重

处理：
- 每次都创建新实例（缓存命中是复用“关闭实例对象”，不是复用“活跃实例”）
- 打开中请求默认不去重
- 即使系统中已有同资源名实例（包括由单实例策略创建的实例）也继续新建

---

## 9. 关闭/聚焦/查询流程设计

## 9.1 `CloseUIForm(int serialId)`（新增主路径）

步骤：

1. 从 `activeFormsBySerial` 取实例
2. 校验实例状态（未释放、已打开、所在组存在）
3. 从 `UIGroup` 移除并关闭
4. 更新所有 active 索引（serial、asset、asset+group）
5. 按缓存策略：
- 缓存容量 > 0 -> 入缓存
- 否则直接释放

## 9.2 `CloseUIForm(string assetName)`（旧 API）

新语义：
- 等价于：
  - `var form = GetTopUIForm(assetName);`
  - `CloseUIForm(form.SerialID);`

如果未命中：
- 保持当前日志/空操作行为

## 9.2.1 `CloseAllUIForms(string assetName)`（新增，建议视为必需）

新语义：
- 关闭该资源名的所有活跃实例（跨 Group）
- 不直接触碰缓存项

建议实现方式：
1. `GetUIForms(assetName)` 获取快照
2. 按快照循环调用 `CloseUIForm(serialId)`

注意：
- 若 `OnClose` 回调里再次触发 UI 操作，遍历必须基于快照，避免重入问题。

## 9.2.2 `CloseUIForm(string assetName, string instanceKey)` / `CloseAllUIFormsByInstanceKey(...)`

建议语义：
- `CloseUIForm(assetName, instanceKey)`：关闭该资源名 + `InstanceKey` 的 Topmost 实例
- `CloseAllUIFormsByInstanceKey(assetName, instanceKey)`：关闭该资源名 + `InstanceKey` 的全部活跃实例

用途：
- 在 `MultiInstanceGlobal` 场景下用业务键精确回收一批窗口
- 在“请求策略优先”产生历史多实例状态后，业务侧可显式做实例收敛

## 9.3 `RefocusUIForm(string/int)` 与 `GetUIForm(string/int)`

原则：
- 内部统一走 `serialId` 路径
- 字符串版本先解析为 `GetTopUIForm(assetName)`

---

## 10. 缓存设计（必须保留现有正确语义）

当前已修复的正确语义必须保持：
- 缓存只存“已关闭、未释放”的窗体
- 活跃窗体绝不进入缓存

多实例下补充要求：

1. 缓存与 active 索引彻底分离
- 缓存项不得出现在 active 索引中

2. 缓存复用时必须刷新上下文
- `UIGroup`
- `pauseCoveredUIForm`
- `serialId`（如果采用会话 ID 方案）
- `LastFocusSequence`

3. 缓存裁剪仍只作用于缓存项
- 继续保留 `IsOpened` 防护

---

## 11. Group 维度行为设计（跨 Group / 单 Group 多实例）

## 11.1 跨 Group 多实例（`SingleInstancePerGroup`）

行为：
- `PanelA` 可在 `HUD` 和 `Popup` 各存在一个实例
- 同一 Group 内重复打开 `PanelA` 会去重并返回该 Group 内现有实例

字符串 API 语义：
- `GetUIForm("PanelA")` 返回最近 `Open`/`Refocus` 的那个实例
- `CloseUIForm("PanelA")` 关闭该“最上层实例”

## 11.2 单 Group 多实例（`MultiInstanceGlobal` + 固定 Group）

行为：
- 在 `Popup` Group 内反复打开 `PanelA`，得到多个实例
- 可通过 `serialId` 精确关闭/聚焦某一个

建议业务层做法：
- 打开后保存 `serialId`
- 在按钮事件里走 `CloseUIForm(serialId)`，避免字符串 API 歧义

## 11.3 全局多实例（`MultiInstanceGlobal`）

行为：
- 同资源名可在多个 Group 中同时存在多个实例
- 字符串 API 仅作为便捷 API，不保证“业务意义上的正确对象”
- 精确控制必须使用 `serialId` API 或 `GetUIForms(...)`

---

## 12. Inspector 与调试支持改造（建议同步）

当前 `UIComponentInspector` 已有缓存/Group 展示能力，建议扩展：

1. 显示同资源名实例聚合信息（按 `assetName` 分组）
- 实例数量
- 各实例 `serialId`
- 所属 Group
- `IsOpened`
- `LastFocusSequence`

2. 显示 active 索引摘要（调试用）
- `activeFormsBySerial.Count`
- `activeSerialsByAsset` key 数量
- `openingRequests.Count`

3. 显示 Topmost 判定结果（调试旧字符串 API）
- 输入资源名 -> 当前 Topmost 实例（可选编辑器按钮）

---

## 13. 实施步骤（建议按阶段落地）

## Phase 1：数据模型与 API 扩展（不改变默认行为）

目标：
- 新增 `UIOpenPolicy`
- 新增 `serialId` 级 API
- 新增查询 API（`GetUIForms` / `GetTopUIForm`）
- 新增 active/opening 索引（先用于内部）

要求：
- 旧 `OpenUIForm(string, string, bool)` 仍保持 `SingleInstanceGlobal`

## Phase 2：打开逻辑策略化

目标：
- `OpenUIForm` 按策略执行去重/多开
- `openingRequests` 使用策略键去重（单实例策略）
- 支持 `SingleInstancePerGroup` 和 `MultiInstanceGlobal`

要求：
- 保持已修复的缓存正确语义
- 保持关闭/关机/移除 Group 清理路径正确

## Phase 3：旧字符串 API 语义落地与文档化

目标：
- `Get/Close/Refocus(string)` 改为 Topmost 语义
- 新注释、文档、示例代码同步更新

## Phase 4：Inspector 与测试完善

目标：
- 多实例状态可视化
- 补充 `TestUI` 多实例测试场景/脚本

---

## 14. 测试方案（必须覆盖）

## 14.1 功能测试

1. 默认策略兼容测试（`SingleInstanceGlobal`）
- 重复打开同名窗体只保留一个实例
- 返回已有实例或打开中句柄

2. 跨 Group 多实例测试（`SingleInstancePerGroup`）
- 同名窗体在 `GroupA`、`GroupB` 各打开一个成功
- 在 `GroupA` 再次打开时去重到 `GroupA` 现有实例

3. 单 Group 多实例测试（`MultiInstanceGlobal`）
- 同 Group 连续打开 3 次得到 3 个不同实例
- `serialId` 都不同（若采用会话 ID 方案）

4. 全局多实例测试（`MultiInstanceGlobal`）
- 同名窗体在多个 Group 多次打开均成功
- `GetUIForms(asset)` 返回全部活跃实例

5. 旧字符串 API Topmost 语义测试
- 多实例并存时 `Get/Close/Refocus(string)` 操作最近 `Open`/`Refocus` 的实例
5.1 同名全部关闭测试
- `CloseAllUIForms(asset)` 能关闭该资源名所有活跃实例（跨 Group）
- 调用后缓存/active 索引状态保持一致
5.2 `InstanceKey` 关闭测试
- `CloseUIForm(asset, instanceKey)` 关闭目标 `InstanceKey` 的 Topmost 实例
- `CloseAllUIFormsByInstanceKey(asset, instanceKey)` 关闭目标 `InstanceKey` 的所有活跃实例
6. 策略混用测试（请求策略优先）
- 先 `SingleInstanceGlobal` 打开到 `GroupA`，再 `MultiInstanceGlobal` 打开到 `GroupB`，应同时存在两个实例
- 在上述状态下再次发起 `SingleInstanceGlobal` 请求，应返回 `Topmost` 实例并输出 `Warning`，不自动合并
- 在 `MultiInstanceGlobal` 制造同组多实例后发起 `SingleInstancePerGroup` 请求，应返回目标 Group 的 `Topmost` 实例并输出 `Warning`

## 14.2 缓存与资源测试

1. 多实例关闭后进入缓存，再次打开复用成功
2. 缓存裁剪不会释放活跃实例
3. 关机/场景切换清理后无残留活跃索引项

## 14.3 稳定性测试

1. 并发异步打开同名窗体
- `SingleInstanceGlobal`：去重
- `SingleInstancePerGroup`：同组去重、跨组不去重
- `MultiInstanceGlobal`：不去重
- 策略混用并发场景：按每个请求自身策略处理，不追溯改写已在飞请求行为

2. 回调重入（`OnClose` 中再次操作 UI）
- 不应触发集合修改异常

### 当前仓库落地状态（2026-02-26）

- 已提供 `TestUI` smoke test 入口：
  - `RunSerialIdReassignSmokeTest`
  - `RunInstanceKeyOrdinalSmokeTest`
  - `RunSingleInstancePerGroupSmokeTest`
  - `RunPolicyMixSmokeTest`
  - `RunAsyncDedupSmokeTest`
- 这些测试覆盖了本章的核心功能路径，但仍建议补充自动化回归（尤其是 `OnClose` 回调重入专项）。

---

## 15. 风险与注意事项

1. 旧业务如果依赖“同名重复打开自动聚焦”的隐式行为
- 在多实例策略启用后会改变
- 需要业务显式选择 `UIOpenPolicy`

2. 字符串 API 在多实例模式下仍然是便捷 API
- 不适合精确业务流程
- 精确控制应迁移到 `serialId` API

3. Topmost 判定与真实渲染层级不一定一致
- 如果项目强依赖“视觉最上层”语义，建议后续引入 Group 优先级/Canvas Sorting 元数据

4. 策略混用会产生“历史多实例状态”
- 后续单实例策略请求不会自动清理这些历史实例（按方案定义这是预期行为）
- 如业务希望恢复单实例状态，应显式调用关闭 API 做收敛

5. 若采用“每次打开重分配 serialId”
- 需要同步更新 Inspector/日志认知（这是预期变化）

---

## 16. 建议的落地顺序（给当前项目）

1. 先做 Phase 1（数据结构 + API 扩展，不改默认行为）
2. 再做 Phase 2（`SingleInstancePerGroup` + `MultiInstanceGlobal`）
3. 再做 Phase 3（字符串 API Topmost 语义）
4. 最后做 Phase 4（Inspector + TestUI 场景）

这样可以保证：
- 每一步都可回归验证
- 默认行为始终兼容
- 多实例能力逐步上线，风险可控

---

## 17. 需要修改的主要文件（实施时参考）

- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIForm.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIGroup.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIFormInfo.cs`（如需额外状态）
- `Assets/StarryFramework/Editor/Inspector/UIComponentInspector.cs`
- `Assets/Test/TestUI/TestUI.cs`
- `Assets/Test/TestUI/TestUIPanel.cs`
- `Assets/Test/TestUI/TestUISetting.cs`

---

## 18. 结论

本方案采用：
- 旧字符串 API = Topmost 实例语义
- 默认策略 = `SingleInstanceGlobal`（兼容）

通过引入 `UIOpenPolicy`、`serialId` 级 API、active/opening 索引与 Topmost 判定机制，可以在不破坏现有缓存修复成果的前提下，稳定支持：
- 全局多实例
- 跨 Group 多实例
- 单 Group 多实例

同时保留了后续扩展空间（如 `InstanceKey`、Group 优先级、严格单 Group 多实例约束）。
