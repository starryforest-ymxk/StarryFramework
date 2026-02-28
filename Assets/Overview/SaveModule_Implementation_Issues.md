# Save 模块实现问题清单（现状审查）

> 历史审查文档（2026-02-28）：用于记录重构前后问题排查过程。
> 当前实施状态与最终方案请以 `SaveModule_AutoDiscovery_Refactor_Plan.md` 为准。

## 文档信息

- 审查日期：2026-02-28
- 审查范围：
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module`
  - `Assets/Plugins/StarryFramework/Editor/Inspector/SaveComponentInspector.cs`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Event Module/EventComponent.cs`
- 审查方式：静态代码审查（未进入 Unity PlayMode）

> 状态更新（2026-02-28）：问题 1（Provider 初始化时序）已在自动发现重构中修复。

---

## 问题总览

当前识别到 5 个问题：

1. 高风险：自定义 Provider 的 `GameSettings` 在启动首次加载时可能不生效
2. 高风险：`DeleteData` 在 `info` 文件缺失时可能抛异常中断
3. 高风险：多个保存写盘路径缺少异常保护
4. 中风险：损坏数据恢复后可能出现“槽位占用”无法复用
5. 低风险：对外暴露可变内部集合，存在越权修改风险

---

## 详细问题

### 1) 自定义 Provider 的 `GameSettings` 首次加载可能不生效（高）

- 位置：
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveComponent.cs:107`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Base/ConfigurableComponent.cs:28`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Base/FrameworkManager.cs:313`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:150`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:184`
- 现象：
  - `SaveManager` 在 `Awake()` 内先执行 `LoadSetting()`；
  - `SaveSettings`（含 `SaveDataProvider`）由 `SaveComponent` 稍后注入；
  - 导致首轮加载 `GameSettings` 时可能仍按内置类型处理。
- 影响：
  - 自定义 `GameSettings` 模型在首次运行/首次加载时行为不一致；
  - 可能出现类型不匹配、默认值回退异常感知。

### 2) `DeleteData` 在 `info` 文件缺失时可能抛异常（高）

- 位置：
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:703`
- 现象：
  - 在数据文件存在的分支中，直接执行 `File.Delete(infoPath)`，未先判断文件存在。
- 影响：
  - 当 `SaveDataXXX.save` 存在但 `SaveDataInfoXXX.save` 缺失时，删除流程可能中断。

### 3) 多处写盘流程缺少异常保护（高）

- 位置：
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:490`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:515`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:542`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:745`
- 现象：
  - `CreateNewData`、`SaveData`、`SaveData(int)`、`SaveSetting` 直接序列化并写盘，缺少 `try/catch` 兜底。
- 影响：
  - 在 IO 异常、权限异常、序列化异常下，可能抛出未处理异常并中断流程。

### 4) 损坏恢复后可能产生“僵尸槽位”（中）

- 位置：
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:304`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:420`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:574`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveManager.cs:631`
- 现象：
  - 槽位分配依赖 `infoDic`；
  - 数据文件损坏时仅迁移数据文件，`info` 可能仍保留并占位。
- 影响：
  - 某些槽位可能长期不可复用，造成用户感知“有空位却无法新建”。

### 5) 对外暴露可变集合（低）

- 位置：
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveComponent.cs:25`
  - `Assets/Plugins/StarryFramework/Runtime/Framework/Save Module/SaveComponent.cs:26`
- 现象：
  - `SaveInfoList` 与 `DataInfoDic` 直接返回内部引用。
- 影响：
  - 外部代码可直接修改内部状态，容易引入不可控副作用。

---

## 建议修复优先级

1. 优先修复问题 1、2、3（会影响稳定性与兼容性）
2. 然后处理问题 4（数据一致性）
3. 最后处理问题 5（API 封装性与可维护性）

---

## 验收建议（修复后）

1. 自定义 Provider 首次启动即按自定义类型正确加载 `GameSettings`
2. `DeleteData` 在缺失 `info` 文件时不抛异常，仅记录日志
3. 所有写盘入口在异常下不崩溃，符合“记录日志、不阻断”目标
4. 损坏恢复后槽位可按预期复用
5. 外部无法直接篡改内部集合（或有明确只读契约）
