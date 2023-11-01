using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class SceneSettings
    {
        [Tooltip("初始场景,设置为GameFramework则不加载初始场景")]
        [SerializeField]
        [SceneIndex]
        internal int StartScene = 0;

        [Tooltip("初始场景加载时是否启用默认动画")]
        [SerializeField]
        internal bool StartSceneAnimation = false;

        [Tooltip("默认场景切换动画的淡入时间")]
        [Min(0)]
        [SerializeField]
        internal float fadeInTime = 0.5f;

        [Tooltip("默认场景切换动画的淡出时间")]
        [Min(0)]
        [SerializeField]
        internal float fadeOutTime = 0.5f;
    }
}
