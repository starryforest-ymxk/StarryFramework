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
    public class SceneSingleton<T> : MonoBehaviour where T : SceneSingleton<T>
    {
        private static T instance;

        /// <summary>
        /// 当单例被摧毁时会调用
        /// </summary>
        public static UnityAction OnSingletonDestroy { get; set; }
        
        public static T GetInstance()
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<T>();
            if (instance != null) return instance;
            GameObject obj = new()
            {
                name = typeof(T).Name
            };
            instance = obj.AddComponent<T>();
            return instance;
        }

        protected virtual void Awake()
        {
            if (instance != null) return;
            instance = FindObjectOfType<T>();
            if (instance != null) return;
            GameObject obj = new()
            {
                name = typeof(T).Name
            };
            instance = obj.AddComponent<T>();
        }

        protected virtual void OnDestroy()
        {
            OnSingletonDestroy?.Invoke();
        }


    }
}

