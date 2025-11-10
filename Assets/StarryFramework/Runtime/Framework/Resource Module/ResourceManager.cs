using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace StarryFramework
{
    internal class ResourceManager : IManager
    {
        private readonly List<AsyncOperationHandle> activeHandles = new();

        void IManager.Awake() { }
        void IManager.Init() { }
        void IManager.ShutDown() 
        { 
            ReleaseAllAddressableHandles();
        }
        void IManager.Update() { }
        void IManager.SetSettings(IManagerSettings settings) { }

        #region Resources Load

        /// <summary>
        /// 从Resources文件夹同步加载资源
        /// </summary>
        internal T LoadRes<T>(string path, bool gameObjectInstantiate) where T : Object
        {
            T res = Resources.Load<T>(path);
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
            ResourceRequest r = Resources.LoadAsync<T>(name);
            if(r == null)
            {
                FrameworkManager.Debugger.LogError($"Can not find asset at Resources/{name}");
                return null;
            }
            if (r.asset is GameObject && gameObjectInstantiate)
            {
                r.completed += _ => { callBack(Object.Instantiate(r.asset) as T); };
            }
            else
            {
                r.completed += _ => { callBack(r.asset as T); };
            }
            return r;
        }

        /// <summary>
        /// 卸载Resources资源
        /// </summary>
        internal void UnloadRes(Object @object)
        {
            Resources.UnloadAsset(@object);
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

        /// <summary>
        /// 从Addressables同步加载资源（使用WaitForCompletion会阻塞主线程）
        /// </summary>
        internal T LoadAddressable<T>(string address, bool gameObjectInstantiate) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            T res = handle.WaitForCompletion();
            
            if (res == null)
            {
                FrameworkManager.Debugger.LogError($"Can not find asset at Addressable address: {address}");
                Addressables.Release(handle);
                return null;
            }

            activeHandles.Add(handle);

            if (res is GameObject && gameObjectInstantiate)
            {
                return Object.Instantiate(res);
            }

            return res;
        }

        /// <summary>
        /// 从Addressables异步加载资源
        /// </summary>
        internal AsyncOperationHandle<T> LoadAddressableAsync<T>(string address, UnityAction<T> callBack, bool gameObjectInstantiate) where T : Object
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(address);
            
            handle.Completed += operation =>
            {
                if (operation.Status == AsyncOperationStatus.Succeeded)
                {
                    T res = operation.Result;
                    if (res is GameObject && gameObjectInstantiate)
                    {
                        callBack?.Invoke(Object.Instantiate(res));
                    }
                    else
                    {
                        callBack?.Invoke(res);
                    }
                }
                else
                {
                    FrameworkManager.Debugger.LogError($"Failed to load Addressable asset: {address}");
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
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(address, parent);
            
            handle.Completed += operation =>
            {
                if (operation.Status != AsyncOperationStatus.Succeeded)
                {
                    FrameworkManager.Debugger.LogError($"Failed to instantiate Addressable asset: {address}");
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
                Addressables.Release(handle);
                activeHandles.Remove(handle);
            }
        }

        /// <summary>
        /// 释放Addressable资源对象
        /// </summary>
        internal void ReleaseAddressableAsset<T>(T asset) where T : Object
        {
            Addressables.Release(asset);
        }

        /// <summary>
        /// 释放通过InstantiateAddressable创建的GameObject实例
        /// </summary>
        internal void ReleaseAddressableInstance(GameObject instance)
        {
            Addressables.ReleaseInstance(instance);
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
        }

        #endregion

    }
}

