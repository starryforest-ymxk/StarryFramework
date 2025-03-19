using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    /// <summary>
    /// 基于继承的Mono单例基类
    /// </summary>
    /// <typeparam Name="T">单例的类</typeparam>
    [DisallowMultipleComponent]
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T instance;

        /// <summary>
        /// 当单例被摧毁时会调用
        /// </summary>
        public static UnityAction OnSingletonDestroy { get; set; }

        private static bool _create = false;

        public static T GetInstance()
        {
            if (instance != null || _create) return instance;
            instance = FindObjectOfType<T>();
            if (instance == null)
            {
                GameObject obj = new()
                {
                    name = typeof(T).Name
                };
                instance = obj.AddComponent<T>();
            }
            _create = true;
            DontDestroyOnLoad(instance);
            return instance;
        }

        protected virtual void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this.gameObject);
            }
            else if (instance == null)
            {
                instance = (T)this;
                _create = true;
                DontDestroyOnLoad(this);
            }
        }

        protected virtual void OnDestroy()
        {
            OnSingletonDestroy?.Invoke();
        }


    }
}

