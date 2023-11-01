using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Mono控制器，Mono，需要挂载在GameManager
/// MonoMgr利用的工具，所有通过MonoMgr注册的事件都在这里执行
/// </summary>
public class MonoController : MonoSingleton<MonoController>
{
    private UnityEvent updateEvent = null;
    void Update()
    {
        updateEvent?.Invoke();
    }
    /// <summary>
    /// 注册一个Update事件(只被MonoMgr调用)
    /// </summary>
    /// <param Name="func">注册事件</param>
    public void AddUpdateListener(UnityAction func)
    {
        updateEvent.AddListener(func);
    }
    /// <summary>
    /// 删除一个Update事件(只被MonoMgr调用)
    /// </summary>
    /// <param Name="func">删除事件</param>
    public void RemoveUpdateListener(UnityAction func)
    {
        updateEvent.RemoveListener(func);
    }
    /// <summary>
    /// 删除所有Update事件(只被MonoMgr调用)
    /// </summary>
    public void RemoveAllUpdateListeners()
    {
        updateEvent.RemoveAllListeners();
    }
}
