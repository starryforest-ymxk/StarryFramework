using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class SaveSettings :IManagerSettings
    {
        [Tooltip("是否自动存储")]
        [SerializeField]
        internal bool AutoSave = true;

        [Tooltip("自动存档时间间隔(秒)")]
        [Min(10)]
        [SerializeField]
        internal float AutoSaveDataInterval = 600f;

        [Tooltip("存档注释列表，第一个为默认注释")]
        [SerializeField]
        [Multiline]
        internal List<string> SaveInfoList = new List<string>();
    }
}
