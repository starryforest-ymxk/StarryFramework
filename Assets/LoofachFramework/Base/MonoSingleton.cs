using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于继承的Mono单例基类
/// </summary>
/// <typeparam Name="T">单例的类</typeparam>
public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    protected static T instance;

    public static T GetInstance() => instance;

    protected virtual void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = (T)this;
            DontDestroyOnLoad(this);
        }
    }

}
