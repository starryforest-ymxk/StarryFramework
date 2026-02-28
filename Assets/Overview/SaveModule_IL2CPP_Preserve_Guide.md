# Save 模块 IL2CPP 保留策略（Provider 自动发现）

## 适用场景

当项目使用 IL2CPP 构建且启用代码裁剪时，`[SaveDataProvider]` 自动发现依赖反射，未被直接引用的类型可能被裁剪。

---

## 推荐方案

优先使用 `link.xml` 显式保留 Provider 与数据模型类型；必要时配合 `[Preserve]`。

### 方案 A：`link.xml`（推荐）

在项目中添加 `Assets/link.xml`（或合并到现有 `link.xml`）：

```xml
<linker>
  <assembly fullname="Assembly-CSharp">
    <type fullname="StarryFramework.Test.DemoSaveDataProvider" preserve="all" />
    <type fullname="StarryFramework.Test.DemoPlayerData" preserve="all" />
    <type fullname="StarryFramework.Test.DemoGameSettings" preserve="all" />
  </assembly>
</linker>
```

说明：
1. `assembly fullname` 替换为你的实际程序集名（常见是 `Assembly-CSharp`）。
2. `type fullname` 使用完整命名空间 + 类型名。
3. 每个自定义 Provider、PlayerData、GameSettings 都应列出。

### 方案 B：`[Preserve]`（补充）

```csharp
using UnityEngine.Scripting;

[Preserve]
[SaveDataProvider(priority: 100)]
public class MySaveDataProvider : ISaveDataProvider
{
    // ...
}
```

说明：
1. 对 Provider 与关键数据类型都可加 `[Preserve]`。
2. 建议和 `link.xml` 同时使用，降低裁剪风险。

---

## 最小验收清单

1. IL2CPP 构建后可正常加载存档并拿到自定义类型。
2. 启动日志能看到当前生效 Provider 类型。
3. `GetPlayerData<T>()` / `GetGameSettings<T>()` 在真机构建中类型匹配。
