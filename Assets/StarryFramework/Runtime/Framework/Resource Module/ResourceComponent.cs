using System;
using UnityEngine;
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
        
        public LoadState State => _state;
        public float Progress => _progress;
        public string ResourcePath => _resourcePath;
        public Type TargetType => _targetType;

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
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            callBack += _ => {
                _state = LoadState.Idle; 
                _progress = 1f; 
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset); 
            };
            ResourceRequest r = Manager.LoadResAsync(path, callBack, gameObjectInstantiate);
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
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;

            void WrappedCallback(T a)
            {
                _state = LoadState.Idle;
                _progress = 1f;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
                callBack?.Invoke(a);
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
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;

            AsyncOperationHandle<GameObject> handle = Manager.InstantiateAddressable(address, parent);
            
            handle.Completed += _ =>
            {
                _state = LoadState.Idle;
                _progress = 1f;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
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



    }
}

