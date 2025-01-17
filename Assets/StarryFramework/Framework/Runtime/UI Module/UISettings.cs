using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class UISettings :IManagerSettings
    {
        [Tooltip("UI Form �����������")]
        [SerializeField]
        public int cacheCapacity = 10;

        [Tooltip("UI Form ����ʼID����")]
        [Min(10)]
        [SerializeField]
        public int startOfSerialID = 0;
    }
}


