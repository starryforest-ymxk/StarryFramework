using System;
using UnityEngine;




namespace StarryFramework
{
    [Serializable]
    public class EventSettings
    {
        [Tooltip("框架内部事件被触发时，是否会同时触发外部同名事件")]
        [SerializeField]
        internal bool InternalEventTrigger = true;
    }
}

