using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace StarryFramework
{
    internal class GameObjectPool<T> : ObjectPoolBase where T : GameObjectBase
    {
        private List<T> poolList = new List<T>();
        private GameObject target;
        private GameObject objectPool;


        private bool CheckObj(IObjectBase _object)
        {
            if(_object == null)
            {
                FrameworkManager.Debugger.LogError("Object is null");
                return false;
            }
            return true;
        }

        internal override Type Type()
        {
            return typeof(T);
        }

        internal override void Register(string name, float autoReleaseInterval, float expireTime)
        {
            property.Name = name;
            property.FullName = name + typeof(T).ToString();
            property.AutoReleaseInterval = autoReleaseInterval;
            property.ExpireTime = expireTime;
            property.LastReleaseTime = Time.unscaledTime;
        }

        internal void SetTarget(GameObject gameObject)
        {
            target = gameObject;
        }

        internal void SetPoolObject(GameObject gameObject)
        {
            objectPool = gameObject;
        }

        internal override void SetLocked(bool locked) { property.Locked = locked; }

        internal override IObjectBase Spawn()
        {
            foreach (var a in poolList)
            {
                if (a.inUse == false)
                {
                    a.inUse = true;
                    a.gameObject.SetActive(true);
                    a.OnSpawn();
                    return a;
                }
            }

            if(target == null)
            {
                FrameworkManager.Debugger.LogError("GameObject to Instantiate is null");
                return null;
            }
            else
            {
                GameObject go = null;
                if (objectPool == null)
                {
                    go = GameObject.Instantiate(target);
                }
                else
                {
                    go = GameObject.Instantiate(target, objectPool.transform);
                }
                T t = go.GetComponent<T>();
                t.inUse = true;
                poolList.Add(t);
                property.Count++;
                t.OnSpawn();
                return t;
            }


        }


        internal override void Unspawn(IObjectBase _object)
        {
            if (!CheckObj(_object)) return;
            if (!poolList.Contains(_object as T))
            {
                poolList.Add(_object as T);
                property.Count++;
            }
            _object.OnUnspawn();
            _object.inUse = false;
            _object.lastUseTime = Time.unscaledTime;
            if (objectPool != null)
            {
                (_object as GameObjectBase).gameObject.transform.SetParent(objectPool.transform, false);
            }
            
            (_object as GameObjectBase).gameObject.SetActive(false);

            if (_object.releaseFlag)
            {
                _object.OnRelease();
                if (poolList.Contains(_object as T))
                {
                    poolList.Remove(_object as T);
                    property.Count--;
                }
                GameObject.Destroy((_object as GameObjectBase).gameObject);
            }
        }

        internal override void CheckRelease()
        {
            if(!property.Locked && Time.unscaledTime > property.LastReleaseTime + property.AutoReleaseInterval)
            {
                for (int i = 0; i < poolList.Count; i++)
                {
                    var a = poolList[i];
                    if (a.inUse == false && Time.unscaledTime - a.lastUseTime > property.ExpireTime)
                    {
                        a.OnRelease();
                        poolList.Remove(a);
                        i--;
                        property.Count--;
                        GameObject.Destroy((a).gameObject);
                    }
                }
            }
        }

        internal override void ReleaseAllUnused()
        {
            for(int i = 0; i< poolList.Count; i++)
            {
                var a = poolList[i];
                if (a.inUse == false)
                {
                    a.OnRelease();
                    poolList.Remove(a);
                    i--;
                    property.Count--;
                    GameObject.Destroy((a as GameObjectBase).gameObject);
                }
            }
        }
        internal override void ReleaseAllObjects()
        {
            for (int i = 0; i < poolList.Count; i++)
            {
                var a = poolList[i];
                poolList.Remove(a);
                i--;
                property.Count--;
                GameObject.Destroy((a as GameObjectBase).gameObject);
            }
        }

        internal override void ReleaseObject(IObjectBase _object)
        {
            if (!CheckObj(_object)) return;
            if (poolList.Contains(_object as T))
            {
                if (_object.inUse == false)
                {
                    _object.OnRelease();
                    poolList.Remove(_object as T);
                    property.Count--;
                    GameObject.Destroy((_object as GameObjectBase).gameObject);
                }
                else
                {
                    _object.releaseFlag = true;
                }
            }
        }

        internal override void Shutdown()
        {
            foreach (var a in poolList)
            {
                a.OnRelease();
                property.Count--;
                GameObject.Destroy((a as GameObjectBase).gameObject);
            }
            poolList.Clear();
        }


    }
}

