using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    internal class ResourceManager : IManager
    {


        void IManager.Awake() { }
        void IManager.Init() { }
        void IManager.ShutDown() { }
        void IManager.Update() { }
        void IManager.SetSettings(IManagerSettings settings) { }


        internal T LoadRes<T>(string path, bool GameObjectInstantiate) where T : UnityEngine.Object
        {
            T res = Resources.Load<T>(path);
            if (res is GameObject && GameObjectInstantiate)
            {
                return GameObject.Instantiate(res);
            }
            else
            {
                return res;
            }
        }

        internal T[] LoadAllRes<T>(string path) where T : UnityEngine.Object
        {
            T[] res = Resources.LoadAll<T>(path);
            return res;
        }

        internal ResourceRequest LoadAsync<T>(string name, UnityAction<T> callBack, bool GameObjectInstantiate) where T : UnityEngine.Object
        {
            ResourceRequest r = Resources.LoadAsync<T>(name);
            if(r == null)
            {
                FrameworkManager.Debugger.LogError($"Can not find asset at Resources/{name}");
                return null;
            }
            if (r.asset is GameObject && GameObjectInstantiate)
            {
                r.completed += (a) => { callBack(GameObject.Instantiate(r.asset) as T); };
            }
            else
            {
                r.completed += (a) => { callBack(r.asset as T); };
            }
            return r;
        }

        internal void Unload(UnityEngine.Object _object)
        {
            Resources.UnloadAsset(_object);
        }

        internal void UnloadUnused()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }

    }
}

