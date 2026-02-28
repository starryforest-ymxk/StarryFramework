# Resource Module 改进计划

## 文档信息

**创建时间**: 2024年  
**框架版本**: StarryFramework  
**模块名称**: Resource Module（资源管理模块）  
**评估结果**: 基础功能完整，但需要改进资源生命周期管理和高级特性

---

## 当前状态概览

### 已实现功能

- ✅ Resources 同步/异步加载
- ✅ Addressables 同步/异步加载
- ✅ GameObject 实例化支持
- ✅ 基础句柄管理
- ✅ Inspector 运行时调试面板
- ✅ 框架事件系统集成（BeforeLoadAsset/AfterLoadAsset）
- ✅ 加载进度追踪

### 架构设计

- **Component-Manager 模式**: 符合框架设计理念
- **统一API接口**: 通过 `ResourceComponent` 提供一致的使用体验
- **双资源系统**: 同时支持 Resources 和 Addressables

---

## 问题清单

### 🔴 高优先级问题（必须修复）

#### 1. GameObject实例化的资源泄漏风险

**问题描述**:
当 `gameObjectInstantiate=true` 时，实例化后的 GameObject 与原始资源的生命周期管理不清晰。

**代码示例**:
```csharp
// ResourceManager.cs - LoadAddressable 方法
if (res is GameObject && gameObjectInstantiate)
{
    return Object.Instantiate(res);  // 返回实例化对象
}
activeHandles.Add(handle);  // 但handle追踪的是原始Prefab
```

**问题影响**:
- Addressables 的 handle 追踪原始 Prefab，但返回的是实例化对象
- 用户调用 `ReleaseAddressableHandle` 释放的是原始资源，而非实例
- 可能导致内存泄漏和引用计数错误
- 与 `InstantiateAddressable` 方法的行为不一致

**改进方案**:
- 移除 `gameObjectInstantiate` 参数，统一使用 `InstantiateAddressable`
- 或者为实例化对象建立 handle 映射关系
- 明确文档说明资源释放规则

---

#### 2. 缺少资源引用计数机制

**问题描述**:
同一资源可能被多次加载，但没有引用计数管理。

**问题场景**:
```csharp
// 场景1：不同组件加载同一资源
var obj1 = Framework.ResourceComponent.LoadAddressable<GameObject>("Player");
var obj2 = Framework.ResourceComponent.LoadAddressable<GameObject>("Player");
// activeHandles 中会有两个相同资源的 handle

// 场景2：过早释放
ReleaseAddressableHandle(handle1);  // 释放后，obj2 也失效了
```

**问题影响**:
- 重复加载相同资源浪费内存和性能
- 一个使用者释放资源后，其他使用者的引用失效
- 无法实现资源共享和缓存优化
- 难以追踪资源的真实使用情况

**改进方案**:
- 实现资源引用计数系统
- 使用字典缓存已加载的资源
- 只有引用计数为0时才真正释放资源
- 提供 `Retain/Release` 接口管理引用

---

#### 3. 异步加载错误处理不完善

**问题描述**:
Resources 异步加载失败时，状态管理有问题。

**代码示例**:
```csharp
// ResourceManager.cs - LoadResAsync 方法
ResourceRequest r = Resources.LoadAsync<T>(name);
if(r == null)
{
    FrameworkManager.Debugger.LogError($"Can not find asset at Resources/{name}");
    return null;  // 返回null，但State已改为Loading
}
```

**问题影响**:
- `ResourceComponent.State` 会停留在 `Loading` 状态
- `Progress` 不会更新到 1.0
- 回调函数可能不会被调用
- 缺少失败回调通知机制

**改进方案**:
- 添加 `LoadState.Failed` 状态
- 失败时调用回调并传递 null
- 统一异步操作的错误处理流程
- 添加失败事件通知

---

### 🟡 中优先级问题（推荐改进）

#### 4. 异步操作状态追踪不完整

**问题描述**:
只能追踪最新的一次异步加载操作。

**代码示例**:
```csharp
// ResourceComponent.cs
private ResourceRequest latestRequest;
private AsyncOperationHandle latestAddressableHandle;
```

**问题影响**:
- 多个异步加载操作同时进行时，只能追踪最新的一个
- Inspector 无法显示所有进行中的加载操作
- 难以调试复杂的资源加载场景
- 无法获取整体加载进度

**改进方案**:
- 使用列表管理所有进行中的异步操作
- 提供获取所有活跃操作的接口
- Inspector 显示所有加载任务列表
- 计算整体加载进度

---

#### 5. 缺少加载策略和优先级管理

**问题描述**:
没有资源加载优先级设置和并发控制。

**缺失功能**:
- 加载优先级设置（高/中/低）
- 并发加载数量限制
- 资源预加载机制
- 资源卸载策略
- 加载队列管理

**问题影响**:
- 大量异步加载可能造成性能峰值
- 无法控制资源加载顺序
- 难以实现关卡资源预加载
- 无法优化加载性能

**改进方案**:
- 添加 `LoadPriority` 枚举（High/Normal/Low）
- 实现加载队列和并发限制
- 提供预加载和批量加载接口
- 实现资源加载调度器

---

#### 6. 缺少完整的资源生命周期事件

**问题描述**:
虽然触发了 `BeforeLoadAsset` 和 `AfterLoadAsset` 事件，但缺少其他关键事件。

**缺失事件**:
- 资源卸载事件（`BeforeUnloadAsset`/`AfterUnloadAsset`）
- 加载失败事件（`OnLoadAssetFailed`）
- 加载进度事件（`OnLoadProgress`）
- 资源引用变化事件（`OnAssetReferenceChanged`）

**问题影响**:
- 难以实现全局的资源加载监控
- 无法统计资源使用情况
- 与框架的事件系统集成不够深入
- 难以实现加载界面和进度条

**改进方案**:
- 在 `FrameworkEvent` 中添加新事件定义
- 在关键节点触发相应事件
- 提供事件参数传递详细信息
- 编写事件使用示例文档

---

### 🟢 低优先级问题（可选优化）

#### 7. Addressables 同步加载性能警告

**问题描述**:
使用 `WaitForCompletion()` 同步加载会阻塞主线程。

**代码示例**:
```csharp
// ResourceManager.cs - LoadAddressable 方法
T res = handle.WaitForCompletion();  // 阻塞主线程
```

**问题影响**:
- 可能导致游戏卡顿，特别是加载网络资源时
- Unity 官方不推荐频繁使用 `WaitForCompletion`
- 与 Addressables 异步设计理念冲突

**改进方案**:
- 添加警告日志提示性能风险
- 推荐使用异步加载方法
- 文档中说明使用场景限制
- 考虑使用协程替代同步加载

---

#### 8. 缺少资源缓存机制

**问题描述**:
没有已加载资源的缓存，每次都重新加载。

**缺失功能**:
- 资源缓存池
- 资源预热（Preload）
- 资源常驻内存配置
- 缓存清理策略
- 内存占用统计

**问题影响**:
- 性能浪费，重复加载相同资源
- 无法复用已加载资源
- 没有利用 Addressables 的缓存特性
- 难以控制内存占用

**改进方案**:
- 实现资源缓存字典
- 提供预加载接口
- 添加资源常驻配置
- 实现 LRU 缓存淘汰策略
- 提供内存占用查询接口

---

#### 9. 与 ObjectPool 模块集成不足

**问题描述**:
Resource Module 和 ObjectPool Module 没有协作。

**潜在集成点**:
- GameObject 实例化后自动进入对象池
- 对象池回收时保持资源引用
- 统一的资源和对象生命周期管理
- 预加载资源并填充对象池

**改进方案**:
- 添加 `LoadAndPoolAddressable` 方法
- 对象池支持 Addressables 资源
- 统一资源释放接口
- 提供集成使用示例

---

#### 10. Inspector 调试功能有限

**问题描述**:
当前 Inspector 只显示单个操作的基本信息。

**缺失功能**:
- 所有活跃句柄列表显示
- 资源内存占用统计
- 一键释放所有资源按钮
- 资源引用计数显示
- 加载历史记录
- 资源依赖关系可视化

**改进方案**:
- 增强 `ResourceComponentInspector`
- 添加资源列表面板
- 提供内存统计图表
- 添加调试工具按钮
- 创建独立的资源管理器窗口

---

## 改进优先级路线图

### Phase 1: 核心问题修复（必须完成）

**目标**: 确保资源管理的正确性和稳定性

1. ✅ 修复 GameObject 实例化的资源管理问题
2. ✅ 实现资源引用计数机制
3. ✅ 完善异步加载错误处理
4. ✅ 添加 `LoadState.Failed` 状态
5. ✅ 统一资源释放逻辑

---

### Phase 2: 功能增强（推荐完成）

**目标**: 提升资源管理的易用性和性能

1. ✅ 实现多异步操作追踪
2. ✅ 添加资源加载优先级系统
3. ✅ 实现加载队列和并发控制
4. ✅ 完善资源生命周期事件
5. ✅ 添加资源预加载接口

---

### Phase 3: 高级优化（可选完成）

**目标**: 提供企业级的资源管理能力

1. ✅ 实现资源缓存和预热机制
2. ✅ 与 ObjectPool 模块深度集成
3. ✅ 增强 Inspector 调试功能
4. ✅ 创建资源管理器编辑器窗口
5. ✅ 添加资源使用统计和分析
6. ✅ 实现资源热更新支持

---

## 技术实现建议

### 1. 资源引用计数系统设计

```csharp
// 新增类：资源引用信息
internal class ResourceRefInfo
{
    public Object Asset;                    // 资源对象
    public AsyncOperationHandle Handle;     // Addressables句柄
    public int RefCount;                    // 引用计数
    public string Address;                  // 资源地址
    public DateTime LoadTime;               // 加载时间
    public bool IsResident;                 // 是否常驻内存
}

// ResourceManager 中添加
private Dictionary<string, ResourceRefInfo> resourceCache = new();

internal T LoadWithRefCount<T>(string address) where T : Object
{
    if (resourceCache.TryGetValue(address, out var info))
    {
        info.RefCount++;
        return info.Asset as T;
    }
    // 首次加载逻辑...
}

internal void ReleaseWithRefCount(string address)
{
    if (resourceCache.TryGetValue(address, out var info))
    {
        info.RefCount--;
        if (info.RefCount <= 0 && !info.IsResident)
        {
            // 真正释放资源
            Addressables.Release(info.Handle);
            resourceCache.Remove(address);
        }
    }
}
```

---

### 2. 加载优先级系统设计

```csharp
// 新增枚举
public enum LoadPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

// 新增类：加载请求
internal class LoadRequest
{
    public string Address;
    public Type ResourceType;
    public LoadPriority Priority;
    public UnityAction<Object> Callback;
    public float QueueTime;
}

// ResourceManager 中添加
private Queue<LoadRequest>[] loadQueues = new Queue<LoadRequest>[4];
private int maxConcurrentLoads = 5;
private int currentLoadingCount = 0;

internal void QueueLoad(LoadRequest request)
{
    int index = (int)request.Priority;
    loadQueues[index].Enqueue(request);
    ProcessLoadQueue();
}
```

---

### 3. 完整的加载状态机

```csharp
public enum LoadState
{
    Idle,           // 空闲
    Queued,         // 已加入队列
    Loading,        // 加载中
    Completed,      // 完成
    Failed,         // 失败
    Cancelled       // 取消
}

// ResourceComponent 中添加错误信息
public string LastError { get; private set; }

// 失败处理示例
callBack += asset => {
    if (asset == null)
    {
        _state = LoadState.Failed;
        LastError = $"Failed to load asset: {address}";
        FrameworkManager.EventManager.InvokeEvent(
            FrameworkEvent.OnLoadAssetFailed, 
            address
        );
    }
    else
    {
        _state = LoadState.Completed;
        LastError = string.Empty;
    }
};
```

---

### 4. 增强的事件系统

```csharp
// FrameworkEvent 中添加新事件
public static class FrameworkEvent
{
    // 现有事件
    public const string BeforeLoadAsset = "BeforeLoadAsset";
    public const string AfterLoadAsset = "AfterLoadAsset";
    
    // 新增事件
    public const string OnLoadAssetFailed = "OnLoadAssetFailed";
    public const string OnLoadProgress = "OnLoadProgress";
    public const string BeforeUnloadAsset = "BeforeUnloadAsset";
    public const string AfterUnloadAsset = "AfterUnloadAsset";
    public const string OnAssetCached = "OnAssetCached";
    public const string OnAssetEvicted = "OnAssetEvicted";
}

// 使用示例
Framework.EventComponent.AddEventListener<string>(
    FrameworkEvent.OnLoadAssetFailed, 
    address => Debug.LogError($"Failed: {address}")
);

Framework.EventComponent.AddEventListener<float>(
    FrameworkEvent.OnLoadProgress,
    progress => UpdateLoadingBar(progress)
);
```

---

## API 设计示例

### 新增推荐 API

```csharp
// ResourceComponent 新增方法

// 引用计数加载
public T LoadWithCache<T>(string address) where T : Object;
public void ReleaseCache(string address);

// 优先级加载
public void LoadAsync<T>(string address, UnityAction<T> callback, 
    LoadPriority priority = LoadPriority.Normal) where T : Object;

// 批量加载
public void LoadMultiple<T>(string[] addresses, 
    UnityAction<T[]> callback) where T : Object;

// 预加载
public void PreloadAssets(string[] addresses, 
    UnityAction onComplete = null);

// 场景资源管理
public void LoadSceneAssets(string sceneName, 
    UnityAction onComplete = null);
public void UnloadSceneAssets(string sceneName);

// 常驻资源
public void SetResident(string address, bool resident);

// 统计查询
public int GetLoadedAssetCount();
public long GetTotalMemoryUsage();
public ResourceRefInfo[] GetAllLoadedAssets();

// 清理
public void ClearCache(bool force = false);
public void UnloadUnusedAssets();
```

---

## 测试计划

### 单元测试清单

- [ ] 资源引用计数正确性测试
- [ ] 重复加载同一资源的内存测试
- [ ] 异步加载失败的状态测试
- [ ] 多个异步操作并发测试
- [ ] GameObject 实例化和释放测试
- [ ] 优先级队列加载顺序测试
- [ ] 资源缓存命中率测试
- [ ] 内存泄漏压力测试

### 性能测试清单

- [ ] 1000个资源加载性能基准
- [ ] 引用计数 vs 重复加载性能对比
- [ ] 缓存命中率统计
- [ ] 内存占用峰值测试
- [ ] 加载队列响应时间测试

---

## 文档更新清单

- [ ] 更新 `PROJECT_OVERVIEW.md` 中的 Resource Module 部分
- [ ] 更新 `API_QUICK_REFERENCE.md` 添加新API
- [ ] 创建 `Resource Module 最佳实践.md`
- [ ] 添加资源管理使用示例到 `/Test` 目录
- [ ] 编写资源生命周期管理指南
- [ ] 更新 Inspector 使用文档

---

## 兼容性说明

### 向后兼容

- 所有现有 API 保持不变
- 新增 API 为可选功能
- 默认行为与现有版本一致
- 提供迁移指南

### 破坏性变更（考虑）

以下变更需要在主版本更新时考虑：

1. 移除 `gameObjectInstantiate` 参数（改用 `InstantiateAddressable`）
2. 将 `LoadAddressable` 同步方法标记为过时
3. 修改默认的资源释放策略

---

## 总结

Resource Module 当前的**基础功能完整且稳定**，但在**资源生命周期管理**和**性能优化**方面还有较大改进空间。

### 关键改进点

1. **资源引用计数**：避免重复加载和过早释放
2. **错误处理**：完善异步操作的失败处理
3. **性能优化**：加载队列、优先级、缓存机制
4. **调试工具**：增强 Inspector 和编辑器窗口

### 预期收益

- ✅ 提升资源管理的健壮性
- ✅ 减少内存占用和加载时间
- ✅ 改善开发和调试体验
- ✅ 为大型项目提供企业级资源管理能力

---

**下一步行动**: 根据优先级路线图，从 Phase 1 开始逐步实施改进。
