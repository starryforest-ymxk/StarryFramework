using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StarryFramework
{
    public abstract class GameObjectBase : MonoBehaviour, IObjectBase
    {
        [FoldOutGroup("对象池属性")]
        [SerializeField]
        private bool _inUse;
        [FoldOutGroup("对象池属性")]
        [SerializeField]
        private float _lastUseTime;
        [FoldOutGroup("对象池属性")]
        [SerializeField]
        private bool _releaseFlag;

        public float lastUseTime { get => _lastUseTime; set => _lastUseTime = value; }
        public bool inUse { get => _inUse; set => _inUse = value; }
        public bool releaseFlag { get => _releaseFlag; set => _releaseFlag = value; }

        public virtual void OnSpawn()
        {

        }

        public virtual void OnUnspawn()
        {

        }

        public virtual void OnRelease()
        {

        }


    }
}

