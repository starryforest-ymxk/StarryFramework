using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class TimerSettings :IManagerSettings
    {
        [Tooltip("清除非使用的触发计时器的时间间隔(秒)")]
        [SerializeField]
        public float ClearUnusedTriggerTimersInterval = 120f;

        [Tooltip("清除非使用的异步计时器的时间间隔(秒)")]
        [Min(10)]
        [SerializeField]
        public float ClearUnusedAsyncTimersInterval = 120f;
    }
}
