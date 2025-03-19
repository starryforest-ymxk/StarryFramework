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
        [Tooltip("Ĭ�ϳ����л������ĵ���ʱ��")]
        [Min(0)]
        [SerializeField]
        internal float defaultAnimationFadeInTime = 0.5f;
        
        [Tooltip("Ĭ�ϳ����л������ĵ���ʱ��")]
        [Min(0)]
        [SerializeField]
        internal float defaultAnimationFadeOutTime = 0.5f;
    }
}
