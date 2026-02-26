# UI模块已知问题与修复思路

整理日期：2026-02-26  
范围：`StarryFramework` UI 模块（`UIComponent / UIManager / UIGroup / UIForm / UguiForm`）  
说明：以下结论基于当前源码静态阅读，未在 Unity 编辑器内逐项复现。

---

## 1. 问题总览（按优先级）

### P0（高优先级，建议先修）

1. 缓存中包含“已打开窗体”，可能导致同一 `UIForm` 实例被重复打开
2. 缓存命中重开时不会更新 `UIGroup` / `pauseCoveredUIForm`，会产生过期上下文
3. 缓存裁剪会释放正在显示中的 UI（尤其运行时热改 `UISettings` 时）
4. `UIGroup.RemoveAndCloseAllUIForms()` 未重置 `formCount`，会污染后续深度计算

### P1（中优先级）

5. 删除 UI 组时未关闭/释放该组窗体，可能造成状态泄漏
6. `UIComponent.cs` 为 Runtime 脚本但直接引用 `UnityEditor`，存在打包编译风险
7. `RemoveAndCloseAllUIForms()` 的 `foreach` 调用用户回调，存在重入修改集合风险

### P2（可优化）

8. `UIComponent` 暴露内部可变集合，外部代码可破坏管理器状态
9. `UguiForm` 依赖硬编码 `CanvasStatic/CanvasDynamic` 且缺少空值校验
10. 同名窗体多开语义不清（API 以资源名为主，关闭/获取行为可能歧义）

---

## 2. 已确认问题与修复思路

### 问题 1：缓存中包含“已打开窗体”，可能重复打开同一实例（P0）

**现象/原因**
- 新建窗体时在真正打开前就加入缓存：`UIManager.OpenUIForm()` 中 `AddUIFormInCache(newForm)`。
- `OpenUIForm()` 再次打开同名资源时会先查缓存并直接复用。
- 当前缓存并不区分“已关闭可复用”和“正在显示”状态。

**影响**
- 同一 `UIForm` 实例可能被重复加入 `UIGroup`。
- 生命周期回调顺序混乱（重复 `OnOpen` / 关闭异常 / 层级异常）。
- 后续缓存裁剪会误释放活跃 UI（与问题 3 叠加）。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:173`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:194`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:269`

**修复思路**
- 明确缓存语义：缓存只存“已关闭但未释放”的窗体。
- 新窗体创建时不要立即加入缓存；应在 `CloseUIForm` 后按策略决定“入缓存 or 释放”。
- 缓存命中复用时，先从缓存移除，再加入目标 `UIGroup`。
- 增加断言/保护：若 `uiForm.IsOpened == true`，禁止进入缓存。

**建议改法（结构）**
- `OpenUIForm(new)`：创建 -> `OnInit` -> 加入组 -> `OnOpen`（不入缓存）
- `CloseUIForm`：从组移除并 `OnClose(false)` -> 若允许缓存则 `AddUIFormInCache`，否则 `OnRelease`
- `CloseAndReleaseAllForms`：先关闭所有活跃窗体，再释放缓存窗体

---

### 问题 2：缓存命中时上下文未更新（组与遮挡暂停策略过期）（P0）

**现象/原因**
- `UIForm` 的 `uiGroup` 和 `pauseCoveredUiForm` 仅在 `UIForm.OnInit(...)` 中设置一次。
- 缓存命中路径不会重新 `OnInit`，也没有“重绑定打开上下文”的逻辑。

**影响**
- 复用窗体打开到新组时，`uiForm.UIGroup` 仍指向旧组。
- `CloseUIForm(UIForm)` / `RefocusUIForm(UIForm)` 可能操作错误的组。
- 本次打开传入的 `pauseCoveredUIForm` 参数不生效。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIForm.cs:76`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:173`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:215`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:239`

**修复思路**
- 将“首次资源初始化”和“每次打开的上下文绑定”拆开。
- 为 `UIForm` 增加方法（例如）：
  - `BindOpenContext(UIGroup group, bool pauseCoveredUIForm)`
  - 或 `PrepareForOpen(UIGroup group, bool pauseCoveredUIForm)`
- 缓存命中时先更新上下文，再加入 UI 组并刷新。

**补充建议**
- 若设计上不允许同一资源跨组复用，可在 `OpenUIForm` 明确限制并报错。

---

### 问题 3：缓存裁剪会释放正在显示中的 UI（P0）

**现象/原因**
- `TrimCacheToCapacity()` / `AddUIFormInCache()` 超容量时直接对缓存尾部执行 `OnRelease()`。
- 由于当前缓存包含活跃窗体（见问题 1），裁剪可能释放正在显示中的 UI。
- 运行时修改 `UISettings.cacheCapacity` 会触发 `ApplySettings(false)`，进一步触发裁剪。

**影响**
- 显示中的窗体被销毁，出现 UI 消失、空引用、后续关闭时报错等问题。
- 运行时热调配置可能直接破坏当前 UI 状态。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:38`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:60`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:64`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:282`

**修复思路**
- 先修复问题 1（缓存仅保存已关闭窗体）。
- 裁剪前增加安全条件：只释放 `!IsOpened` 且 `!ReleaseTag` 的窗体。
- 若缓存中存在异常活跃项，打印错误日志并跳过（保守处理）。
- 可增加运行时不变量检查方法（Debug 模式）：扫描缓存中 `IsOpened` 项并报警。

---

### 问题 4：`RemoveAndCloseAllUIForms()` 未重置 `formCount`（P0）

**现象/原因**
- 方法关闭所有窗体并清空 `formInfosList`，但未将 `formCount` 设回 `0`。
- 后续 `DepthRefresh()` 使用旧 `formCount` 计算深度。

**影响**
- 新打开窗体的 `DepthInUIGroup` 可能错误。
- `UIFormLogic.OnDepthChanged(...)` 收到不正确深度值，导致 sibling index、排序异常。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIGroup.cs:255`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIGroup.cs:261`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIGroup.cs:267`

**修复思路**
- 在 `RemoveAndCloseAllUIForms(bool isShutdown)` 末尾增加：
  - `formCount = 0;`
  - 可选：`pause = false;`（视语义决定，通常在组清空时重置更安全）

---

### 问题 5：删除 UI 组时未关闭/释放该组窗体（P1）

**现象/原因**
- `UIManager.RemoveUIGroup(string)` 仅从字典删除组对象。
- 未调用该组的关闭流程。

**影响**
- 场景中 UI 物体仍存在，但管理器无法再访问。
- 状态、引用、回调可能泄漏，造成“孤儿 UI”。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:105`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs:112`

**修复思路**
- 删除前先执行组级清理：
  - `group.RemoveAndCloseAllUIForms(false)`（或补一个 `ReleaseAll` 流程）
- 再从字典移除。
- 若未来支持“组销毁后保留缓存”，需明确缓存与组的解耦规则；否则建议同步释放相关缓存项。

---

### 问题 6：Runtime 脚本直接 `using UnityEditor`（P1）

**现象/原因**
- `UIComponent.cs` 位于 Runtime 目录，但文件头直接 `using UnityEditor;`。
- 尽管 `OnValidate()` 已经用 `#if UNITY_EDITOR` 包裹，`using` 本身仍可能在 Player 编译中报错。

**影响**
- 无 `.asmdef` 隔离时，打包/Player 编译存在失败风险。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs:3`

**修复思路**
- 将 `using UnityEditor;` 包裹为：
  - `#if UNITY_EDITOR`
  - `using UnityEditor;`
  - `#endif`
- 或移除该 `using`（若当前文件内未直接使用 `Editor` 类型，可完全不需要）。

**备注**
- 这类问题在框架其他 Runtime 文件中也存在，建议做一次全局排查。

---

### 问题 7：批量关闭时 `foreach` 调用户回调存在重入风险（P1）

**现象/原因**
- `UIGroup.RemoveAndCloseAllUIForms()` 使用 `foreach` 遍历 `formInfosList`。
- `uiForm.OnClose(...)` 会进入用户实现，用户代码可能再次调用 UI API 修改同一组集合。

**影响**
- 可能抛出 “Collection was modified” 异常。
- 批量关闭流程被中断，部分 UI 未关闭。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIGroup.cs:255`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIForm.cs:131`

**修复思路**
- 改为显式 `LinkedListNode` 迭代（先缓存 `next` 再回调），与 `UIGroup.Update()` 的做法保持一致。
- 或先拷贝快照数组后遍历关闭。

---

### 问题 8：`UIComponent` 暴露内部可变集合（P2）

**现象/原因**
- `UIGroupsDic` 和 `UIFormsCacheList` 直接返回管理器内部集合引用。

**影响**
- 外部业务代码可直接 `Add/Remove/Clear`，破坏 UI 管理器不变量。
- 调试用字段与运行时 API 边界不清晰。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs:17`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs:18`

**修复思路**
- 运行时 API 改为只读视图或快照：
  - `IReadOnlyDictionary<string, UIGroup>`
  - `IReadOnlyCollection<UIForm>` / `UIForm[]`
- Inspector 若需要内部结构，可使用 `internal` + Editor 访问，或通过专门调试接口获取。

---

### 问题 9：`UguiForm` 对场景 Canvas 命名有硬依赖（P2）

**现象/原因**
- `Awake()` 中直接 `GameObject.Find("CanvasStatic")` / `GameObject.Find("CanvasDynamic")`。
- 未找到时无空值保护。

**影响**
- 场景命名不一致时直接空引用。
- UI 预制体可移植性较差，不利于多场景复用。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/Examples/UguiForm.cs:48`
- `Assets/StarryFramework/Runtime/Framework/UI Module/Examples/UguiForm.cs:49`

**修复思路**
- 将父节点来源改为可配置（`UIRoot`、`CanvasProvider`、序列化引用、标签等）。
- 增加空值校验与可读错误日志。
- 保留 `Find` 作为 fallback，而非主路径。

---

### 问题 10：同名窗体多开语义不清（P2）

**现象/原因**
- 对外 API 主要按资源名操作：`GetUIForm(string)` / `CloseUIForm(string)` / `RefocusUIForm(string)`。
- 框架内部虽然支持按 `serialId` 查询（在 `UIGroup`），但未在 `UIComponent/UIManager` 对外暴露。
- 如果允许同名窗体多开（例如多个相同弹窗实例），按资源名操作会歧义。

**影响**
- 可能关闭到“错误实例”。
- 缓存命中与多开语义冲突，进一步放大问题 1/2。

**代码位置**
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs:93`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs:124`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs:142`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIGroup.cs:91`

**修复思路（两种方向二选一）**
- 方向 A（简单）：明确“同资源名窗体全局唯一”，`OpenUIForm` 若已打开则拒绝/复用并返回已有实例。
- 方向 B（灵活）：正式支持多开，对外暴露 `serialId` 级 API（关闭/聚焦/查询），并让 `OpenUIForm` 返回后由调用方保存句柄/ID。

---

## 3. 推荐修复顺序（建议）

### 第一轮（保证正确性）

1. 修复缓存语义（问题 1）
2. 增加缓存复用的上下文重绑定（问题 2）
3. 修复缓存裁剪安全性（问题 3）
4. 修复 `formCount` 未重置（问题 4）

### 第二轮（稳健性）

5. 删除 UI 组前关闭组内窗体（问题 5）
6. 修复 Runtime `UnityEditor` 引用（问题 6）
7. 修复批量关闭重入迭代风险（问题 7）

### 第三轮（接口与可维护性）

8. 收紧 `UIComponent` 的集合暴露（问题 8）
9. 优化 `UguiForm` Canvas 获取方式（问题 9）
10. 明确同名窗体多开策略并调整 API（问题 10）

---

## 4. 修复后建议验证点

1. 同一窗体打开 -> 关闭 -> 再打开（同组）是否正常复用
2. 同一窗体关闭后在不同组打开时，关闭/聚焦是否操作正确组
3. 运行时降低 `cacheCapacity` 时，显示中的 UI 不应被释放
4. 场景切换前 `UIRoot.ClearCache()` 后再次打开 UI，深度值是否从 0/1 正常递增
5. 删除 UI 组后场景中是否仍残留该组 UI 物体
6. Player 编译是否通过（验证 `UnityEditor` 引用问题）
7. 批量关闭过程中 UI 回调内再次调用 UI API 时是否稳定

---

## 5. 附：涉及的主要文件

- `Assets/StarryFramework/Runtime/Framework/UI Module/UIComponent.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIManager.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIGroup.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIForm.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/UIFormLogic.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/Examples/UguiForm.cs`
- `Assets/StarryFramework/Runtime/Framework/UI Module/Examples/UIRoot.cs`

