using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{

    internal abstract class ObjectPoolBase
    {

        internal ObjectPoolProperty property = new ObjectPoolProperty();
        internal string FullName => property.FullName;
        internal abstract Type Type();
        internal abstract void Register(string name, float autoReleaseInterval, float expireTime);
        internal abstract void SetLocked(bool locked);
        internal abstract IObjectBase Spawn();
        internal abstract void Unspawn(IObjectBase _object);
        internal abstract void ReleaseObject(IObjectBase _object);
        internal abstract void CheckRelease();
        internal abstract void ReleaseAllUnused();
        internal abstract void ReleaseAllObjects();
        internal abstract void Shutdown();
    }
}
