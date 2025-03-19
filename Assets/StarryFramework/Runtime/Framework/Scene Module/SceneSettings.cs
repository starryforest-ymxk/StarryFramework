using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarryFramework
{
    [Serializable]
    public class SceneSettings :IManagerSettings
    {
        [Tooltip("默认场景切换动画的淡入时间")]
        [Min(0)]
        [SerializeField]
        internal float defaultAnimationFadeInTime = 0.5f;
        
        [Tooltip("默认场景切换动画的淡出时间")]
        [Min(0)]
        [SerializeField]
        internal float defaultAnimationFadeOutTime = 0.5f;
    }
}
