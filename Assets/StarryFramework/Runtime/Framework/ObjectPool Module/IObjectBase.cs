using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public interface IObjectBase
    {
        float lastUseTime { get; set; }

        bool inUse { get; set; }

        bool releaseFlag { get; set; }

        public abstract void OnSpawn();

        public abstract void OnUnspawn();

        public abstract void OnRelease();
    }
}
