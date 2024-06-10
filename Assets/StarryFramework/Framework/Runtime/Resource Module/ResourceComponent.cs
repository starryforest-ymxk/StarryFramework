using UnityEngine;
using UnityEngine.Events;
using System;

namespace StarryFramework
{
    public class ResourceComponent : BaseComponent
    {

        private ResourceManager _manager = null;

        private ResourceManager manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = FrameworkManager.GetManager<ResourceManager>();
                }
                return _manager;
            }
        }
        private Type _targetType = null;

        private string _resourcePath = "";

        private LoadState _state = LoadState.Idle;

        private float _progress = 0;


        public LoadState State => _state;

        public float Progress => _progress;

        public string ResourcePath => _resourcePath;

        public Type TargetType => _targetType;

        private ResourceRequest latestRequest = null;

        protected override void Awake()
        {
            base.Awake();
            if (_manager == null)
            {
                _manager = FrameworkManager.GetManager<ResourceManager>();
            }
        }

        private void Update()
        {
            if(_state == LoadState.Loading)
            {
                _progress = latestRequest.progress;
            }
        }

        internal override void DisableProcess()
        {
            base.DisableProcess();
        }

        /// <summary>
        /// 同步加载一个资源
        /// </summary>
        /// <typeparam path="T">资源的类型</typeparam>
        /// <param path="path">资源在Resources文件夹下完整路径名</param>
        /// <param path="GameObjectInstantiate">如果资源是GameObject是否直接生成</param>
        /// <returns>如果相应资源为Gameobjet,则生成并返回物体；如果不是，则直接返回物体</returns>
        public T LoadRes<T>(string path, bool GameObjectInstantiate = false) where T : UnityEngine.Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            T t =  manager.LoadRes<T>(path, GameObjectInstantiate);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
            return t;
        }

        /// <summary>
        /// 同步加载路径下所有资源
        /// </summary>
        /// <typeparam path="T">资源的类型</typeparam>
        /// <param path="path">路径</param>
        /// <returns>加载的资源数组</returns>
        public T[] LoadAllRes<T>(string path) where T : UnityEngine.Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            T[] t = manager.LoadAllRes<T>(path);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
            return t;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam path="T">资源类型</typeparam>
        /// <param path="path">资源在Resources文件夹下的路径，省略扩展名</param>
        /// <param path="callBack">资源加载完的回调，以加载的资源物体为参数</param>
        /// <param path="GameObjectInstantiate">如果资源是GameObject是否直接生成</param> 
        /// <returns>资源加载请求</returns>
        public ResourceRequest LoadAsync<T>(string path, UnityAction<T> callBack, bool GameObjectInstantiate = false) where T : UnityEngine.Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            callBack += (a) => {
                _state = LoadState.Idle; 
                _progress = 1f; 
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset); 
            };
            ResourceRequest r = manager.LoadAsync<T>(path, callBack, GameObjectInstantiate);
            latestRequest = r;
            return r;
        }

        /// <summary>
        /// 只能卸载非GameObject对象, GameObject对象用Destroy即可
        /// </summary>
        /// <param path="_object"></param>
        public void Unload(UnityEngine.Object _object)
        {
            manager.Unload(_object);
        }

        /// <summary>
        /// 释放所有没在使用的资源
        /// </summary>
        public void UnloadUnused()
        {
            manager.UnloadUnused();
        }



    }
}

