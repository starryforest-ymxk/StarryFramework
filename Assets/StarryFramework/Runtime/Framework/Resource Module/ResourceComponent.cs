using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace StarryFramework
{
    public class ResourceComponent : BaseComponent
    {
        private ResourceManager _manager;
        private ResourceManager Manager => _manager ??= FrameworkManager.GetManager<ResourceManager>();
        
        private Type _targetType;
        private string _resourcePath = "";
        private LoadState _state = LoadState.Idle;
        private float _progress;
        private string _lastError = "";
        
        public LoadState State => _state;
        public float Progress => _progress;
        public string ResourcePath => _resourcePath;
        public Type TargetType => _targetType;
        public string LastError => _lastError;

        private ResourceRequest latestRequest;
        private AsyncOperationHandle latestAddressableHandle;

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<ResourceManager>();
        }

        private void Update()
        {
            if(_state == LoadState.Loading)
            {
                if (latestRequest != null)
                {
                    _progress = latestRequest.progress;
                }
                else if (latestAddressableHandle.IsValid())
                {
                    _progress = latestAddressableHandle.PercentComplete;
                }
            }
        }

        /// <summary>
        /// 从Resources文件夹同步加载一个资源
        /// </summary>
        /// <typeparam name="T">资源的类型</typeparam>
        /// <param name="path">资源在Resources文件夹内的相对路径</param>
        /// <param name="gameObjectInstantiate">如果资源是GameObject是否直接实例化</param>
        /// <returns>如果对应资源为GameObject，是否实例化可选；如果不是，则直接返回资源</returns>
        public T LoadRes<T>(string path, bool gameObjectInstantiate = false) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            T t =  Manager.LoadRes<T>(path, gameObjectInstantiate);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
            return t;
        }

        /// <summary>
        /// 从Resources文件夹同步加载路径下所有资源
        /// </summary>
        /// <typeparam name="T">资源的类型</typeparam>
        /// <param name="path">路径</param>
        /// <returns>返回的资源数组</returns>
        public T[] LoadAllRes<T>(string path) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            T[] t = Manager.LoadAllRes<T>(path);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
            return t;
        }

        /// <summary>
        /// 从Resources文件夹异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源在Resources文件夹下的路径，省略扩展名</param>
        /// <param name="callBack">资源加载完成的回调，以加载的资源作为参数</param>
        /// <param name="gameObjectInstantiate">如果资源是GameObject是否直接实例化</param> 
        /// <returns>资源请求对象</returns>
        public ResourceRequest LoadResAsync<T>(string path, UnityAction<T> callBack, bool gameObjectInstantiate = false) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            _lastError = "";
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            _progress = 0f;
            
            void WrappedCallback(T asset)
            {
                if (asset == null)
                {
                    _state = LoadState.Failed;
                    _progress = 0f;
                    _lastError = $"Failed to load asset at Resources/{path}";
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, path);
                }
                else
                {
                    _state = LoadState.Completed;
                    _progress = 1f;
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
                }
                callBack?.Invoke(asset);
            }
            
            ResourceRequest r = Manager.LoadResAsync(path, (UnityAction<T>)WrappedCallback, gameObjectInstantiate);
            latestRequest = r;
            return r;
        }

        /// <summary>
        /// 卸载非GameObject类型的资源，GameObject需要用Destroy销毁
        /// </summary>
        /// <param name="object">要卸载的资源对象</param>
        public void UnloadRes(Object @object)
        {
            Manager.UnloadRes(@object);
        }

        /// <summary>
        /// 释放所有未使用的Resources资源
        /// </summary>
        public void UnloadUnusedRes()
        {
            Manager.UnloadUnusedRes();
        }

        /// <summary>
        /// 从Addressables同步加载资源（使用WaitForCompletion，会阻塞主线程）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="address">Addressable资源地址</param>
        /// <param name="gameObjectInstantiate">如果资源是GameObject是否直接实例化</param>
        /// <returns>加载的资源对象</returns>
        public T LoadAddressable<T>(string address, bool gameObjectInstantiate = false) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = address;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            T t = Manager.LoadAddressable<T>(address, gameObjectInstantiate);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
            return t;
        }

        /// <summary>
        /// 从Addressables异步加载资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="address">Addressable资源地址</param>
        /// <param name="callBack">资源加载完成的回调函数</param>
        /// <param name="gameObjectInstantiate">如果资源是GameObject是否直接实例化</param>
        /// <returns>异步操作句柄</returns>
        public AsyncOperationHandle<T> LoadAddressableAsync<T>(string address, UnityAction<T> callBack, bool gameObjectInstantiate = false) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = address;
            _lastError = "";
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            _progress = 0f;

            void WrappedCallback(T asset)
            {
                if (asset == null)
                {
                    _state = LoadState.Failed;
                    _progress = 0f;
                    _lastError = $"Failed to load Addressable asset: {address}";
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, address);
                }
                else
                {
                    _state = LoadState.Completed;
                    _progress = 1f;
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
                }
                callBack?.Invoke(asset);
            }

            AsyncOperationHandle<T> handle = Manager.LoadAddressableAsync(address, (UnityAction<T>)WrappedCallback, gameObjectInstantiate);
            latestAddressableHandle = handle;
            return handle;
        }

        /// <summary>
        /// 使用Addressables实例化GameObject（推荐使用此方法而非LoadAddressable+Instantiate）
        /// </summary>
        /// <param name="address">Addressable资源地址</param>
        /// <param name="parent">实例化后的父节点（可选）</param>
        /// <returns>异步操作句柄</returns>
        public AsyncOperationHandle<GameObject> InstantiateAddressable(string address, Transform parent = null)
        {
            _targetType = typeof(GameObject);
            _resourcePath = address;
            _lastError = "";
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            _progress = 0f;

            AsyncOperationHandle<GameObject> handle = Manager.InstantiateAddressable(address, parent);
            
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    _state = LoadState.Completed;
                    _progress = 1f;
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
                }
                else
                {
                    _state = LoadState.Failed;
                    _progress = 0f;
                    _lastError = $"Failed to instantiate Addressable asset: {address}";
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadAssetFailed, address);
                }
            };

            latestAddressableHandle = handle;
            return handle;
        }

        /// <summary>
        /// 释放Addressable异步操作句柄
        /// </summary>
        /// <param name="handle">要释放的句柄</param>
        public void ReleaseAddressableHandle(AsyncOperationHandle handle)
        {
            Manager.ReleaseAddressableHandle(handle);
        }

        /// <summary>
        /// 释放Addressable资源对象
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="asset">要释放的资源对象</param>
        public void ReleaseAddressableAsset<T>(T asset) where T : Object
        {
            Manager.ReleaseAddressableAsset(asset);
        }

        /// <summary>
        /// 释放通过InstantiateAddressable创建的GameObject实例
        /// </summary>
        /// <param name="instance">要释放的GameObject实例</param>
        public void ReleaseAddressableInstance(GameObject instance)
        {
            Manager.ReleaseAddressableInstance(instance);
        }

        /// <summary>
        /// 释放所有Addressable资源句柄（框架关闭时会自动调用）
        /// </summary>
        public void ReleaseAllAddressableHandles()
        {
            Manager.ReleaseAllAddressableHandles();
        }

        public int GetLoadedAssetCount()
        {
            return Manager.GetLoadedAssetCount();
        }

        public Dictionary<string, ResourceRefInfo> GetAllLoadedAssets()
        {
            return Manager.GetAllLoadedAssets();
        }

        public int GetActiveOperationCount()
        {
            return Manager.GetActiveOperationCount();
        }

        public Dictionary<string, AsyncLoadOperation> GetAllActiveOperations()
        {
            return Manager.GetAllActiveOperations();
        }

        public void ReleaseResource(string address)
        {
            Manager.ReleaseResource(address);
        }

        public long GetTotalMemorySize()
        {
            return Manager.GetTotalMemorySize();
        }

        public Dictionary<string, ResourceRefInfo> GetResourcesByType(ResourceSourceType sourceType)
        {
            return Manager.GetResourcesByType(sourceType);
        }

        /// <summary>
        /// 按标签批量加载Addressables资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="label">Addressable资源标签</param>
        /// <param name="onEachLoaded">每个资源加载完成时的回调</param>
        /// <param name="onCompleted">所有资源加载完成时的回调</param>
        /// <returns>异步操作句柄</returns>
        public AsyncOperationHandle<IList<T>> LoadAddressablesByLabel<T>(
            string label,
            UnityAction<T> onEachLoaded = null,
            UnityAction<BatchLoadResult<T>> onCompleted = null) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = $"Label:{label}";
            _lastError = "";
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            _progress = 0f;

            void WrappedOnCompleted(BatchLoadResult<T> result)
            {
                if (result.FailedCount > 0)
                {
                    _state = LoadState.Failed;
                    _lastError = $"Failed to load {result.FailedCount} assets from label: {label}";
                }
                else
                {
                    _state = LoadState.Completed;
                    _progress = 1f;
                }
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
                onCompleted?.Invoke(result);
            }

            AsyncOperationHandle<IList<T>> handle = Manager.LoadAddressablesByLabelAsync(label, onEachLoaded, WrappedOnCompleted);
            latestAddressableHandle = handle;
            return handle;
        }

        /// <summary>
        /// 按多个标签或地址批量加载Addressables资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="keys">标签或地址列表</param>
        /// <param name="mergeMode">多个键的合并模式（Union/Intersection/UseFirst）</param>
        /// <param name="onEachLoaded">每个资源加载完成时的回调</param>
        /// <param name="onCompleted">所有资源加载完成时的回调</param>
        /// <returns>异步操作句柄</returns>
        public AsyncOperationHandle<IList<T>> LoadAddressablesBatch<T>(
            IEnumerable keys,
            Addressables.MergeMode mergeMode = Addressables.MergeMode.Union,
            UnityAction<T> onEachLoaded = null,
            UnityAction<BatchLoadResult<T>> onCompleted = null) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = $"Batch:Multiple keys";
            _lastError = "";
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            _progress = 0f;

            void WrappedOnCompleted(BatchLoadResult<T> result)
            {
                if (result.FailedCount > 0)
                {
                    _state = LoadState.Failed;
                    _lastError = $"Failed to load {result.FailedCount} assets from batch";
                }
                else
                {
                    _state = LoadState.Completed;
                    _progress = 1f;
                }
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
                onCompleted?.Invoke(result);
            }

            AsyncOperationHandle<IList<T>> handle = Manager.LoadAddressablesBatchAsync(keys, mergeMode, onEachLoaded, WrappedOnCompleted);
            latestAddressableHandle = handle;
            return handle;
        }

        /// <summary>
        /// 按地址列表批量加载Addressables资源（可关联地址和资源）
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="addresses">地址列表</param>
        /// <param name="onEachLoaded">每个资源加载完成时的回调（包含地址和资源）</param>
        /// <param name="onCompleted">所有资源加载完成时的回调</param>
        public void LoadAddressablesByAddresses<T>(
            IList<string> addresses,
            UnityAction<string, T> onEachLoaded = null,
            UnityAction<BatchLoadResult<T>> onCompleted = null) where T : Object
        {
            _targetType = typeof(T);
            _resourcePath = $"Addresses:{addresses.Count}";
            _lastError = "";
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            _progress = 0f;

            void WrappedOnCompleted(BatchLoadResult<T> result)
            {
                if (result.FailedCount > 0)
                {
                    _state = LoadState.Failed;
                    _lastError = $"Failed to load {result.FailedCount}/{addresses.Count} assets";
                }
                else
                {
                    _state = LoadState.Completed;
                    _progress = 1f;
                }
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
                onCompleted?.Invoke(result);
            }

            Manager.LoadAddressablesByAddressesAsync(addresses, onEachLoaded, WrappedOnCompleted);
        }



    }
}

