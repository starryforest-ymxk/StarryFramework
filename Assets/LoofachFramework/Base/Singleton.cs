using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基于继承的非Mono单例基类
/// </summary>
/// <typeparam Name="T">新单例的类</typeparam>
public class Singleton<T> where T : Singleton<T>, new()
{
    private static T instance;
    public static T GetInstance()
    {
        if (instance == null)
        {
            instance = new T();
        }
        return instance;
    }
}
