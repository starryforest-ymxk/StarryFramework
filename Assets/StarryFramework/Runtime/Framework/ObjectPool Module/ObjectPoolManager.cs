using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace StarryFramework
{
    public class ObjectPoolManager : IManager
    {

        private Dictionary<string, ObjectPoolBase> poolDic = new Dictionary<string, ObjectPoolBase>();

        private List<ObjectPoolProperty> objectPoolProperties = new List<ObjectPoolProperty>();

        void IManager.Awake()
        {

        }

        void IManager.Init()
        {

        }

        void IManager.ShutDown()
        {
            poolDic.Clear();
            objectPoolProperties.Clear();
        }

        void IManager.Update()
        {
            foreach (var pool in poolDic.Values)
            {
                pool.CheckRelease();
            }
        }
        
        void IManager.SetSettings(IManagerSettings settings) { }

        internal List<ObjectPoolProperty> getObjectPoolProperties()
        {
            return objectPoolProperties;
        }


        #region private 

        private void RegisterPool(ObjectPoolBase pool)
        {
            string name = pool.FullName;
            if (poolDic.ContainsKey(name))
            {
                FrameworkManager.Debugger.LogError("The same object pool has existed.");
            }
            else
            {
                poolDic.Add(name, pool);
                objectPoolProperties.Add(pool.property);
            }
        }

        private void ReleasePool(ObjectPoolBase pool)
        {
            if (poolDic.ContainsKey(pool.FullName))
            {
                poolDic.Remove(pool.FullName);
                objectPoolProperties.Remove(pool.property);
            }
            else
            {
                FrameworkManager.Debugger.LogError("The object pool dosen't existed.");
            }
        }

        private ObjectPoolBase GetPool<T>(string key) where T : IObjectBase
        {
            string fullname = key + typeof(T).ToString();
            if (poolDic.ContainsKey(fullname))
            {
                return poolDic[fullname];
            }
            else
            {
                FrameworkManager.Debugger.LogError("Object pool doesn't existed.");
                return null;
            }
        }

        #endregion

        internal void Register<T>(float autoReleaseInterval, float expireTime, string key = "") where T : ObjectBase, new()
        {
            ObjectPoolBase pool = new ObjectPool<T>();
            pool.Register(key, autoReleaseInterval, expireTime);
            RegisterPool(pool);
        }
        internal void Register<T>(GameObject targetObject, float autoReleaseInterval, float expireTime, GameObject fatherObject = null, string key = "") where T : GameObjectBase
        {
            ObjectPoolBase pool = new GameObjectPool<T>();
            (pool as GameObjectPool<T>).SetPoolObject(fatherObject);
            pool.Register(key, autoReleaseInterval, expireTime);
            (pool as GameObjectPool<T>).SetTarget(targetObject);
            RegisterPool(pool);
        }
        internal void Register<T>(string path, float autoReleaseInterval, float expireTime, GameObject fatherObject = null, string key = "") where T : GameObjectBase
        {
            ObjectPoolBase pool = new GameObjectPool<T>();
            (pool as GameObjectPool<T>).SetPoolObject(fatherObject);
            pool.Register(key, autoReleaseInterval, expireTime);
            GameObject res = Resources.Load<GameObject>(path);
            if (res == null)
            {
                FrameworkManager.Debugger.LogError("Invalid resource path.");
            }
            else
            {
                (pool as GameObjectPool<T>).SetTarget(res);
                RegisterPool(pool);
            }

        }
        internal T Require<T>(string key = "") where T : class, IObjectBase
        {
            ObjectPoolBase pool = GetPool<T>(key);
            return pool.Spawn() as T;
        }
        internal void Recycle<T>(T obj, string key = "") where T : class, IObjectBase
        {
            ObjectPoolBase pool = GetPool<T>(key);
            pool.Unspawn(obj);
        }
        internal void SetLocked<T>(bool locked, string key = "") where T : class, IObjectBase
        {
            ObjectPoolBase pool = GetPool<T>(key);
            pool.SetLocked(locked);
        }
        internal void ReleaseObject<T>(T obj, string key = "") where T : class, IObjectBase
        {
            ObjectPoolBase pool = GetPool<T>(key);
            pool.ReleaseObject(obj);
        }
        internal void ReleaseAllUnused<T>(string key = "") where T : class, IObjectBase
        {
            ObjectPoolBase pool = GetPool<T>(key);
            pool.ReleaseAllUnused();
        }
        internal void ReleaseAllObjects<T>(string key = "") where T : class, IObjectBase
        {
            ObjectPoolBase pool = GetPool<T>(key);
            pool.ReleaseAllObjects();
        }
        internal void ReleasePool<T>(string key = "") where T : class, IObjectBase
        {
            ObjectPoolBase pool = GetPool<T>(key);
            pool.Shutdown();
            ReleasePool(pool);
        }



    }
}

