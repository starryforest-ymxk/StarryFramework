using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class TimerSettings :IManagerSettings
    {
        [Tooltip("�����ʹ�õĴ�����ʱ����ʱ����(��)")]
        [SerializeField]
        public float ClearUnusedTriggerTimersInterval = 120f;

        [Tooltip("�����ʹ�õ��첽��ʱ����ʱ����(��)")]
        [Min(10)]
        [SerializeField]
        public float ClearUnusedAsyncTimersInterval = 120f;
    }
}
