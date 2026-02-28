using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class UISettings :IManagerSettings
    {
        [Tooltip("UI Form 缓存最大容量")]
        [SerializeField]
        public int cacheCapacity = 10;

        [Tooltip("UI Form 的起始ID计数")]
        [Min(10)]
        [SerializeField]
        public int startOfSerialID = 0;
    }
}


