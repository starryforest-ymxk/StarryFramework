using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StarryFramework
{
    internal class ObjectPool<T> : ObjectPoolBase where T : ObjectBase, new()
    {
        private List<T> poolList = new List<T>();

        private bool CheckObj(IObjectBase _object)
        {
            if (_object == null)
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

        internal override void SetLocked(bool locked) { property.Locked = locked; }

        internal override IObjectBase Spawn()
        {
            foreach(var a in poolList)
            {
                if(a.inUse == false)
                {
                    a.inUse = true;
                    a.OnSpawn();
                    return a;
                }
            }

            T t = new T();
            poolList.Add(t);
            property.Count++;
            t.inUse = true;
            t.OnSpawn();
            return t;
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

            if(_object.releaseFlag)
            {
                _object.OnRelease();
                if (poolList.Contains(_object as T))
                {
                    poolList.Remove(_object as T);
                    property.Count--;
                }

            }
        }



        internal override void CheckRelease()
        {
            if (!property.Locked && Time.unscaledTime > property.LastReleaseTime + property.AutoReleaseInterval)
            {
                for(int i = 0; i < poolList.Count; i++)
                {
                    var a = poolList[i];
                    if (a.inUse == false && Time.unscaledTime - a.lastUseTime > property.ExpireTime)
                    {
                        a.OnRelease();
                        poolList.Remove(a);
                        i--;
                        property.Count--;
                    }
                }
            }
        }

        internal override void ReleaseAllUnused()
        {
            for(int i = 0; i < poolList.Count; i++)
            {
                var a = poolList[i];
                if (a.inUse == false)
                {
                    a.OnRelease();
                    poolList.Remove(a);
                    i--;
                    property.Count--;
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

            }
        }

        internal override void ReleaseObject(IObjectBase _object)
        {
            if (!CheckObj(_object)) return;
            if (poolList.Contains(_object as T))
            {
                if(_object.inUse == false)
                {
                    _object.OnRelease();
                    poolList.Remove(_object as T);
                    property.Count--;
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
            }
            poolList.Clear();
        }

    }
}

