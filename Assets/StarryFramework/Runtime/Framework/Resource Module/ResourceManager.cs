using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

namespace StarryFramework
{
    public class ResourceRefInfo
    {
        public Object Asset;
        public object Handle;
        public int RefCount;
        public string Address;
        public DateTime LoadTime;
        public ResourceSourceType SourceType;
        public Type AssetType;
        
        public long GetMemorySize()
        {
            if (Asset == null) return 0;
            
            if (Asset is Texture texture)
                return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(texture);
            else if (Asset is AudioClip audio)
                return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(audio);
            else if (Asset is Mesh mesh)
                return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
            else
                return UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(Asset);
        }
    }

    public enum ResourceSourceType
    {
        Resources,
        Addressables
    }

    public class AsyncLoadOperation
    {
        public string Address { get; set; }
        public Type AssetType { get; set; }
        public LoadState State { get; set; }
        public float Progress { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime StartTime { get; set; }
        public object Handle { get; set; }
        public int TotalCount { get; set; }
        public int LoadedCount { get; set; }
        public bool IsBatchOperation { get; set; }
        
        public float ElapsedTime => (float)(DateTime.Now - StartTime).TotalSeconds;
    }

    public class BatchLoadResult<T> where T : Object
    {
        public List<T> Assets { get; set; }
        public List<string> Addresses { get; set; }
        public Dictionary<string, T> AssetDictionary { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> FailedAddresses { get; set; }
        
        public BatchLoadResult()
        {
            Assets = new List<T>();
            Addresses = new List<string>();
            AssetDictionary = new Dictionary<string, T>();
            FailedAddresses = new List<string>();
        }
    }

    internal class ResourceManager : IManager
    {
        private readonly List<AsyncOperationHandle> activeHandles = new();
        private readonly Dictionary<string, ResourceRefInfo> resourceCache = new();
        private readonly Dictionary<string, AsyncLoadOperation> activeOperations = new();
        private readonly Dictionary<int, string> handleToAddress = new();

        void IManager.Awake() { }
        void IManager.Init() { }
        void IManager.ShutDown() 
        { 
            ReleaseAllAddressableHandles();
            resourceCache.Clear();
            activeOperations.Clear();
            handleToAddress.Clear();
        }
        void IManager.Update() 
        {
            foreach (var kvp in activeOperations)
            {
                var operation = kvp.Value;
                if (operation.State == LoadState.Loading && operation.Handle != null)
                {
                    float newProgress = 0f;
                    
                    if (operation.Handle is AsyncOperationHandle handle && handle.IsValid())
                    {
                        newProgress = handle.PercentComplete;
                    }
                    else if (operation.Handle is ResourceRequest request)
                    {
                        newProgress = request.progress;
                    }
                    
                    if (Math.Abs(newProgress - operation.Progress) > 0.01f)
                    {
                        operation.Progress = newProgress;
                        FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetProgress, kvp.Key, newProgress);
                    }
                }
            }
        }
        #region Resources Load

        /// <summary>
        /// 从Resources文件夹同步加载资源
        /// </summary>
        internal T LoadRes<T>(string path, bool gameObjectInstantiate) where T : Object
        {
            string cacheKey = $"Resources/{path}";
            
            if (resourceCache.TryGetValue(cacheKey, out var cachedInfo))
            {
                cachedInfo.RefCount++;
                if (cachedInfo.Asset is GameObject && gameObjectInstantiate)
                {
                    return Object.Instantiate(cachedInfo.Asset) as T;
                }
                return cachedInfo.Asset as T;
            }
            
            T res = Resources.Load<T>(path);
            
            if (res != null)
            {
                var refInfo = new ResourceRefInfo
                {
                    Asset = res,
                    Handle = null,
                    RefCount = 1,
                    Address = cacheKey,
                    LoadTime = DateTime.Now,
                    SourceType = ResourceSourceType.Resources,
                    AssetType = typeof(T)
                };
                resourceCache[cacheKey] = refInfo;
            }
            
            if (res is GameObject && gameObjectInstantiate)
            {
                return Object.Instantiate(res);
            }

            return res;
        }

        /// <summary>
        /// 从Resources文件夹加载路径下所有资源
        /// </summary>
        internal T[] LoadAllRes<T>(string path) where T : Object
        {
            T[] res = Resources.LoadAll<T>(path);
            return res;
        }

        /// <summary>
        /// 从Resources文件夹异步加载资源
        /// </summary>
        internal ResourceRequest LoadResAsync<T>(string name, UnityAction<T> callBack, bool gameObjectInstantiate) where T : Object
        {
            string cacheKey = $"Resources/{name}";
            
            if (resourceCache.TryGetValue(cacheKey, out var cachedInfo))
            {
                cachedInfo.RefCount++;
                T cachedAsset = cachedInfo.Asset as T;
                
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetStart, name);
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, name);
                
                if (cachedAsset is GameObject && gameObjectInstantiate)
                {
                    callBack?.Invoke(Object.Instantiate(cachedAsset) as T);
                }
                else
                {
                    callBack?.Invoke(cachedAsset);
                }
                
                return null;
            }
            
            ResourceRequest r = Resources.LoadAsync<T>(name);
            if(r == null)
            {
                FrameworkManager.Debugger.LogError($"Can not find asset at Resources/{name}");
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, name);
                callBack?.Invoke(null);
                return null;
            }
            
            var operation = new AsyncLoadOperation
            {
                Address = cacheKey,
                AssetType = typeof(T),
                State = LoadState.Loading,
                Progress = 0f,
                StartTime = DateTime.Now,
                Handle = r
            };
            activeOperations[cacheKey] = operation;
            
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetStart, name);
            
            r.completed += asyncOperation =>
            {
                if (r.asset == null)
                {
                    operation.State = LoadState.Failed;
                    operation.ErrorMessage = $"Failed to load asset at Resources/{name}";
                    FrameworkManager.Debugger.LogError(operation.ErrorMessage);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, name);
                    activeOperations.Remove(cacheKey);
                    callBack?.Invoke(null);
                    return;
                }
                
                operation.State = LoadState.Completed;
                operation.Progress = 1f;
                
                if (!resourceCache.ContainsKey(cacheKey))
                {
                    var refInfo = new ResourceRefInfo
                    {
                        Asset = r.asset as T,
                        Handle = r,
                        RefCount = 1,
                        Address = cacheKey,
                        LoadTime = DateTime.Now,
                        SourceType = ResourceSourceType.Resources,
                        AssetType = typeof(T)
                    };
                    resourceCache[cacheKey] = refInfo;
                }
                
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, name);
                activeOperations.Remove(cacheKey);
                
                if (r.asset is GameObject && gameObjectInstantiate)
                {
                    callBack(Object.Instantiate(r.asset) as T);
                }
                else
                {
                    callBack(r.asset as T);
                }
            };
            
            return r;
        }

        /// <summary>
        /// 卸载Resources资源
        /// </summary>
        internal void UnloadRes(Object @object)
        {
            string address = FindAddressByAsset(@object);
            if (!string.IsNullOrEmpty(address) && resourceCache.TryGetValue(address, out var refInfo))
            {
                if (refInfo.SourceType == ResourceSourceType.Resources)
                {
                    refInfo.RefCount--;
                    if (refInfo.RefCount <= 0)
                    {
                        Resources.UnloadAsset(@object);
                        resourceCache.Remove(address);
                        FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnReleaseAsset, address);
                    }
                }
                else
                {
                    FrameworkManager.Debugger.LogWarning($"Asset {address} is from Addressables, use ReleaseAddressableAsset instead.");
                }
            }
            else
            {
                Resources.UnloadAsset(@object);
            }
        }

        internal void ReleaseResource(string address)
        {
            if (resourceCache.TryGetValue(address, out var refInfo))
            {
                refInfo.RefCount--;
                if (refInfo.RefCount <= 0)
                {
                    if (refInfo.SourceType == ResourceSourceType.Resources)
                    {
                        if (refInfo.Asset != null)
                        {
                            Resources.UnloadAsset(refInfo.Asset);
                        }
                    }
                    else
                    {
                        if (refInfo.Asset != null)
                        {
                            Addressables.Release(refInfo.Asset);
                        }
                        if (refInfo.Handle is AsyncOperationHandle handle && handle.IsValid())
                        {
                            activeHandles.Remove(handle);
                        }
                    }
                    
                    resourceCache.Remove(address);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnReleaseAsset, address);
                }
            }
        }

        /// <summary>
        /// 释放所有未使用的Resources资源
        /// </summary>
        internal void UnloadUnusedRes()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

        #endregion

        #region Addressables Load
        
        private int GetHandleId(AsyncOperationHandle handle)
        {
            return handle.GetHashCode();
        }
        
        private string FindAddressByHandle(AsyncOperationHandle handle)
        {
            int handleId = GetHandleId(handle);
            if (handleToAddress.TryGetValue(handleId, out string address))
            {
                return address;
            }
            return null;
        }
        
        private string FindAddressByAsset(Object asset)
        {
            foreach (var kvp in resourceCache)
            {
                if (kvp.Value.Asset == asset)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        /// <summary>
        /// 从Addressables同步加载资源（使用WaitForCompletion会阻塞主线程）
        /// </summary>
        internal T LoadAddressable<T>(string address, bool gameObjectInstantiate) where T : Object
        {
            FrameworkManager.Debugger.LogWarning($"LoadAddressable synchronous method uses WaitForCompletion which may cause performance issues. Consider using LoadAddressableAsync instead.");
            
            if (resourceCache.TryGetValue(address, out var cachedInfo))
            {
                cachedInfo.RefCount++;
                if (cachedInfo.Asset is GameObject && gameObjectInstantiate)
                {
                    return Object.Instantiate(cachedInfo.Asset) as T;
                }
                return cachedInfo.Asset as T;
            }

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            T res = handle.WaitForCompletion();
            
            if (res == null)
            {
                FrameworkManager.Debugger.LogError($"Can not find asset at Addressable address: {address}");
                Addressables.Release(handle);
                return null;
            }

            var refInfo = new ResourceRefInfo
            {
                Asset = res,
                Handle = handle,
                RefCount = 1,
                Address = address,
                LoadTime = DateTime.Now,
                SourceType = ResourceSourceType.Addressables,
                AssetType = typeof(T)
            };
            resourceCache[address] = refInfo;
            activeHandles.Add(handle);
            handleToAddress[GetHandleId(handle)] = address;

            if (res is GameObject && gameObjectInstantiate)
            {
                return Object.Instantiate(res) as T;
            }

            return res;
        }

        /// <summary>
        /// 从Addressables异步加载资源
        /// </summary>
        internal AsyncOperationHandle<T> LoadAddressableAsync<T>(string address, UnityAction<T> callBack, bool gameObjectInstantiate) where T : Object
        {
            if (resourceCache.TryGetValue(address, out var cachedInfo))
            {
                cachedInfo.RefCount++;
                T cachedAsset = cachedInfo.Asset as T;
                
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetStart, address);
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, address);
                
                if (cachedAsset is GameObject && gameObjectInstantiate)
                {
                    callBack?.Invoke(Object.Instantiate(cachedAsset));
                }
                else
                {
                    callBack?.Invoke(cachedAsset);
                }
                
                return (AsyncOperationHandle<T>)cachedInfo.Handle;
            }

            var operation = new AsyncLoadOperation
            {
                Address = address,
                AssetType = typeof(T),
                State = LoadState.Loading,
                Progress = 0f,
                StartTime = DateTime.Now
            };
            
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetStart, address);

            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            operation.Handle = handle;
            activeOperations[address] = operation;
            
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    T res = op.Result;
                    
                    operation.State = LoadState.Completed;
                    operation.Progress = 1f;
                    
                    if (!resourceCache.ContainsKey(address))
                    {
                        var refInfo = new ResourceRefInfo
                        {
                            Asset = res,
                            Handle = handle,
                            RefCount = 1,
                            Address = address,
                            LoadTime = DateTime.Now,
                            SourceType = ResourceSourceType.Addressables,
                            AssetType = typeof(T)
                        };
                        resourceCache[address] = refInfo;
                        handleToAddress[GetHandleId(handle)] = address;
                    }
                    
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, address);
                    activeOperations.Remove(address);
                    
                    if (res is GameObject && gameObjectInstantiate)
                    {
                        callBack?.Invoke(Object.Instantiate(res) as T);
                    }
                    else
                    {
                        callBack?.Invoke(res);
                    }
                }
                else
                {
                    operation.State = LoadState.Failed;
                    operation.Progress = 0f;
                    operation.ErrorMessage = $"Failed to load Addressable asset: {address}";
                    
                    FrameworkManager.Debugger.LogError(operation.ErrorMessage);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, address);
                    activeOperations.Remove(address);
                    handleToAddress.Remove(GetHandleId(handle));
                    callBack?.Invoke(null);
                }
            };

            activeHandles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 使用Addressables实例化GameObject
        /// </summary>
        internal AsyncOperationHandle<GameObject> InstantiateAddressable(string address, Transform parent = null)
        {
            var operation = new AsyncLoadOperation
            {
                Address = address,
                AssetType = typeof(GameObject),
                State = LoadState.Loading,
                Progress = 0f,
                StartTime = DateTime.Now
            };
            
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetStart, address);
            
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent);
            operation.Handle = handle;
            activeOperations[address + "_instantiate"] = operation;
            handleToAddress[GetHandleId(handle)] = address;
            
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    operation.State = LoadState.Completed;
                    operation.Progress = 1f;
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, address);
                    activeOperations.Remove(address + "_instantiate");
                }
                else
                {
                    operation.State = LoadState.Failed;
                    operation.Progress = 0f;
                    operation.ErrorMessage = $"Failed to instantiate Addressable asset: {address}";
                    FrameworkManager.Debugger.LogError(operation.ErrorMessage);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, address);
                    activeOperations.Remove(address + "_instantiate");
                    handleToAddress.Remove(GetHandleId(handle));
                }
            };

            activeHandles.Add(handle);
            return handle;
        }

        /// <summary>
        /// 释放单个Addressable资源句柄
        /// </summary>
        internal void ReleaseAddressableHandle(AsyncOperationHandle handle)
        {
            if (handle.IsValid())
            {
                string address = FindAddressByHandle(handle);
                if (!string.IsNullOrEmpty(address) && resourceCache.TryGetValue(address, out var refInfo))
                {
                    refInfo.RefCount--;
                    if (refInfo.RefCount <= 0)
                    {
                        Addressables.Release(handle);
                        resourceCache.Remove(address);
                        activeHandles.Remove(handle);
                        handleToAddress.Remove(GetHandleId(handle));
                        FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnReleaseAsset, address);
                    }
                }
                else
                {
                    Addressables.Release(handle);
                    activeHandles.Remove(handle);
                    handleToAddress.Remove(GetHandleId(handle));
                }
            }
        }

        /// <summary>
        /// 释放Addressable资源对象
        /// </summary>
        internal void ReleaseAddressableAsset<T>(T asset) where T : Object
        {
            string address = FindAddressByAsset(asset);
            if (!string.IsNullOrEmpty(address) && resourceCache.TryGetValue(address, out var refInfo))
            {
                refInfo.RefCount--;
                if (refInfo.RefCount <= 0)
                {
                    Addressables.Release(asset);
                    if (refInfo.Handle is AsyncOperationHandle handle && handle.IsValid())
                    {
                        activeHandles.Remove(handle);
                        handleToAddress.Remove(GetHandleId(handle));
                    }
                    resourceCache.Remove(address);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnReleaseAsset, address);
                }
            }
            else
            {
                Addressables.Release(asset);
            }
        }

        /// <summary>
        /// 释放通过InstantiateAddressable创建的GameObject实例
        /// </summary>
        internal void ReleaseAddressableInstance(GameObject instance)
        {
            Addressables.ReleaseInstance(instance);
        }

        /// <summary>
        /// 批量加载Addressables资源（按标签）
        /// </summary>
        internal AsyncOperationHandle<IList<T>> LoadAddressablesByLabelAsync<T>(
            string label, 
            UnityAction<T> onEachLoaded = null, 
            UnityAction<BatchLoadResult<T>> onCompleted = null) where T : Object
        {
            return LoadAddressablesBatchAsync(
                new List<object> { label }, 
                Addressables.MergeMode.Union, 
                onEachLoaded, 
                onCompleted);
        }

        /// <summary>
        /// 批量加载Addressables资源（按多个标签或地址）
        /// </summary>
        internal AsyncOperationHandle<IList<T>> LoadAddressablesBatchAsync<T>(
            IEnumerable keys, 
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Union,
            UnityAction<T> onEachLoaded = null,
            UnityAction<BatchLoadResult<T>> onCompleted = null) where T : Object
        {
            string batchId = $"Batch_{typeof(T).Name}_{DateTime.Now.Ticks}";
            
            var operation = new AsyncLoadOperation
            {
                Address = batchId,
                AssetType = typeof(T),
                State = LoadState.Loading,
                Progress = 0f,
                StartTime = DateTime.Now,
                TotalCount = 0,
                LoadedCount = 0,
                IsBatchOperation = true
            };
            
            activeOperations[batchId] = operation;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetStart, batchId);

            var result = new BatchLoadResult<T>();
            
            AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(
                keys,
                loadedAsset =>
                {
                    operation.LoadedCount++;
                    result.Assets.Add(loadedAsset);
                    result.SuccessCount++;
                    
                    onEachLoaded?.Invoke(loadedAsset);
                    
                    if (operation.TotalCount > 0)
                    {
                        operation.Progress = (float)operation.LoadedCount / operation.TotalCount;
                        FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetProgress, batchId, operation.Progress);
                    }
                },
                mergeMode
            );

            operation.Handle = handle;
            activeHandles.Add(handle);
            handleToAddress[GetHandleId(handle)] = batchId;
            
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    operation.State = LoadState.Completed;
                    operation.Progress = 1f;
                    operation.TotalCount = op.Result.Count;
                    operation.LoadedCount = op.Result.Count;
                    
                    foreach (var asset in op.Result)
                    {
                        if (asset != null)
                        {
                            string address = FindAddressByAsset(asset);
                            if (string.IsNullOrEmpty(address))
                            {
                                address = $"{batchId}_{asset.name}";
                            }
                            
                            result.Addresses.Add(address);
                            result.AssetDictionary[address] = asset;
                            
                            if (!resourceCache.ContainsKey(address))
                            {
                                var refInfo = new ResourceRefInfo
                                {
                                    Asset = asset,
                                    Handle = handle,
                                    RefCount = 1,
                                    Address = address,
                                    LoadTime = DateTime.Now,
                                    SourceType = ResourceSourceType.Addressables,
                                    AssetType = typeof(T)
                                };
                                resourceCache[address] = refInfo;
                            }
                            else
                            {
                                resourceCache[address].RefCount++;
                            }
                        }
                        else
                        {
                            result.FailedCount++;
                        }
                    }
                    
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, batchId);
                    onCompleted?.Invoke(result);
                }
                else
                {
                    operation.State = LoadState.Failed;
                    operation.ErrorMessage = $"Failed to batch load assets with keys: {string.Join(", ", keys)}";
                    result.FailedCount = operation.TotalCount;
                    
                    FrameworkManager.Debugger.LogError(operation.ErrorMessage);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, batchId);
                    onCompleted?.Invoke(result);
                }
                
                activeOperations.Remove(batchId);
            };

            return handle;
        }

        /// <summary>
        /// 按地址列表批量加载Addressables资源（可关联地址和资源）
        /// </summary>
        internal void LoadAddressablesByAddressesAsync<T>(
            IList<string> addresses,
            UnityAction<string, T> onEachLoaded = null,
            UnityAction<BatchLoadResult<T>> onCompleted = null) where T : Object
        {
            string batchId = $"BatchAddresses_{typeof(T).Name}_{DateTime.Now.Ticks}";
            
            var operation = new AsyncLoadOperation
            {
                Address = batchId,
                AssetType = typeof(T),
                State = LoadState.Loading,
                Progress = 0f,
                StartTime = DateTime.Now,
                TotalCount = addresses.Count,
                LoadedCount = 0,
                IsBatchOperation = true
            };
            
            activeOperations[batchId] = operation;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetStart, batchId);

            var result = new BatchLoadResult<T>();
            var loadHandles = new List<AsyncOperationHandle<T>>();
            var loadedAssets = new Dictionary<string, T>();

            AsyncOperationHandle<IList<IResourceLocation>> locationsHandle = 
                Addressables.LoadResourceLocationsAsync(addresses, Addressables.MergeMode.Union, typeof(T));

            locationsHandle.Completed += locOp =>
            {
                if (locOp.Status == AsyncOperationStatus.Succeeded)
                {
                    foreach (var location in locOp.Result)
                    {
                        string address = location.PrimaryKey;
                        
                        if (resourceCache.TryGetValue(address, out var cachedInfo))
                        {
                            cachedInfo.RefCount++;
                            T cachedAsset = cachedInfo.Asset as T;
                            
                            operation.LoadedCount++;
                            result.Assets.Add(cachedAsset);
                            result.Addresses.Add(address);
                            result.AssetDictionary[address] = cachedAsset;
                            result.SuccessCount++;
                            
                            onEachLoaded?.Invoke(address, cachedAsset);
                            
                            operation.Progress = (float)operation.LoadedCount / operation.TotalCount;
                            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetProgress, batchId, operation.Progress);
                        }
                        else
                        {
                            AsyncOperationHandle<T> assetHandle = Addressables.LoadAssetAsync<T>(location);
                            loadHandles.Add(assetHandle);
                            activeHandles.Add(assetHandle);
                            handleToAddress[GetHandleId(assetHandle)] = address;
                            
                            assetHandle.Completed += assetOp =>
                            {
                                operation.LoadedCount++;
                                
                                if (assetOp.Status == AsyncOperationStatus.Succeeded && assetOp.Result != null)
                                {
                                    T asset = assetOp.Result;
                                    loadedAssets[address] = asset;
                                    
                                    result.Assets.Add(asset);
                                    result.Addresses.Add(address);
                                    result.AssetDictionary[address] = asset;
                                    result.SuccessCount++;
                                    
                                    var refInfo = new ResourceRefInfo
                                    {
                                        Asset = asset,
                                        Handle = assetHandle,
                                        RefCount = 1,
                                        Address = address,
                                        LoadTime = DateTime.Now,
                                        SourceType = ResourceSourceType.Addressables,
                                        AssetType = typeof(T)
                                    };
                                    resourceCache[address] = refInfo;
                                    
                                    onEachLoaded?.Invoke(address, asset);
                                }
                                else
                                {
                                    result.FailedCount++;
                                    result.FailedAddresses.Add(address);
                                    FrameworkManager.Debugger.LogError($"Failed to load asset at address: {address}");
                                }
                                
                                operation.Progress = (float)operation.LoadedCount / operation.TotalCount;
                                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetProgress, batchId, operation.Progress);
                                
                                if (operation.LoadedCount >= operation.TotalCount)
                                {
                                    operation.State = result.FailedCount > 0 ? LoadState.Failed : LoadState.Completed;
                                    operation.Progress = 1f;
                                    
                                    if (result.FailedCount > 0)
                                    {
                                        operation.ErrorMessage = $"Failed to load {result.FailedCount}/{operation.TotalCount} assets";
                                        FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, batchId);
                                    }
                                    else
                                    {
                                        FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, batchId);
                                    }
                                    
                                    activeOperations.Remove(batchId);
                                    onCompleted?.Invoke(result);
                                }
                            };
                        }
                    }
                    
                    if (operation.LoadedCount >= operation.TotalCount)
                    {
                        operation.State = LoadState.Completed;
                        operation.Progress = 1f;
                        FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetSucceeded, batchId);
                        activeOperations.Remove(batchId);
                        onCompleted?.Invoke(result);
                    }
                }
                else
                {
                    operation.State = LoadState.Failed;
                    operation.ErrorMessage = "Failed to load resource locations";
                    FrameworkManager.Debugger.LogError(operation.ErrorMessage);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, batchId);
                    activeOperations.Remove(batchId);
                    onCompleted?.Invoke(result);
                }
                
                Addressables.Release(locationsHandle);
            };
        }

        /// <summary>
        /// 释放所有Addressable资源句柄
        /// </summary>
        internal void ReleaseAllAddressableHandles()
        {
            foreach (var handle in activeHandles)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            activeHandles.Clear();
            resourceCache.Clear();
            handleToAddress.Clear();
        }

        internal int GetLoadedAssetCount()
        {
            return resourceCache.Count;
        }

        internal Dictionary<string, ResourceRefInfo> GetAllLoadedAssets()
        {
            return new Dictionary<string, ResourceRefInfo>(resourceCache);
        }

        internal int GetActiveOperationCount()
        {
            return activeOperations.Count;
        }

        internal Dictionary<string, AsyncLoadOperation> GetAllActiveOperations()
        {
            return new Dictionary<string, AsyncLoadOperation>(activeOperations);
        }

        internal long GetTotalMemorySize()
        {
            long totalSize = 0;
            foreach (var kvp in resourceCache)
            {
                totalSize += kvp.Value.GetMemorySize();
            }
            return totalSize;
        }

        internal Dictionary<string, ResourceRefInfo> GetResourcesByType(ResourceSourceType sourceType)
        {
            var filtered = new Dictionary<string, ResourceRefInfo>();
            foreach (var kvp in resourceCache)
            {
                if (kvp.Value.SourceType == sourceType)
                {
                    filtered[kvp.Key] = kvp.Value;
                }
            }
            return filtered;
        }

        #endregion

    }
}

