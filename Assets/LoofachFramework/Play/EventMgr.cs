using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 通用事件接口，保证任意数量泛型的事件都能被保存在一个字典中
/// </summary>
public interface IEventInfo { }
#region 0-3个参数的事件类型(可随意添加到更多参数)
public class EventInfo : IEventInfo
{
    public UnityAction Action;
}
public class EventInfo<T> : IEventInfo
{
    public UnityAction<T> Action;
}
public class EventInfo<T1, T2> : IEventInfo
{
    public UnityAction<T1, T2> Action;
}
public class EventInfo<T1, T2, T3> : IEventInfo
{
    public UnityAction<T1, T2, T3> Action;
}
#endregion

/// <summary>
/// 基于委托实现的事件系统
/// 默认只支持0-3个参数的事件，但若有需要可以随时按样子进行拓展
/// 对某物体做出操作的事件，需要在物体被禁用时进行删除，防止空引用报错
/// </summary>
public class EventMgr : Singleton<EventMgr>
{
    /// <summary>
    /// 事件字典，储存任意参数个数的事件
    /// </summary>
    private Dictionary<string, IEventInfo> eventDic = new Dictionary<string, IEventInfo>();

    #region 添加观察者
    /// <summary>
    /// 添加无泛型事件观察者
    /// </summary>
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">触发的函数</param>
    public void AddEventListener(string eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).Action += action;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo() { Action = action });
        }
    }
    /// <summary>
    /// 添加单泛型事件观察者
    /// </summary>
    /// <typeparam Name="T">第一个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">触发的函数</param>
    public void AddEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).Action += action;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T>() { Action = action });
        }
    }
    /// <summary>
    /// 添加双泛型事件观察者
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <typeparam Name="T2">第二个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">触发的函数</param>
    public void AddEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T1, T2>).Action += action;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T1, T2>() { Action = action });
        }
    }
    /// <summary>
    /// 添加三个泛型事件观察者
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <typeparam Name="T2">第二个参数的类型</typeparam>
    /// <typeparam Name="T3">第三个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">触发的函数</param>
    public void AddEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T1, T2, T3>).Action += action;
        }
        else
        {
            eventDic.Add(eventName, new EventInfo<T1, T2, T3>() { Action = action });
        }
    }
    #endregion
    #region 删除观察者
    /// <summary>
    /// 删除无泛型事件观察者
    /// </summary>
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">删除函数</param>
    public void DeleteEventListener(string eventName, UnityAction action)
    {
        if (eventDic.ContainsKey(eventName) && eventDic[eventName] != null)
        {
            (eventDic[eventName] as EventInfo).Action -= action;
        }
        else
        {
            Debug.Log("尝试删除不存在的事件");
        }
    }
    /// <summary>
    /// 删除单泛型事件观察者
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">删除函数</param>
    public void DeleteEventListener<T>(string eventName, UnityAction<T> action)
    {
        if (eventDic.ContainsKey(eventName) && eventDic[eventName] != null)
        {
            (eventDic[eventName] as EventInfo<T>).Action -= action;
        }
        else
        {
            Debug.Log("尝试删除不存在的事件");
        }
    }
    /// <summary>
    /// 删除双泛型事件观察者
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <typeparam Name="T2">第二个参数的类型</typeparam>    
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">删除函数</param>
    public void DeleteEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
    {
        if (eventDic.ContainsKey(eventName) && eventDic[eventName] != null)
        {
            (eventDic[eventName] as EventInfo<T1, T2>).Action -= action;
        }
        else
        {
            Debug.Log("尝试删除不存在的事件");
        }
    }
    /// <summary>
    /// 删除三泛型事件观察者
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <typeparam Name="T2">第二个参数的类型</typeparam>
    /// <typeparam Name="T3">第三个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    /// <param Name="action">删除函数</param>
    public void DeleteEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
    {
        if (eventDic.ContainsKey(eventName) && eventDic[eventName] != null)
        {
            (eventDic[eventName] as EventInfo<T1, T2, T3>).Action -= action;
        }
        else
        {
            Debug.Log("尝试删除不存在的事件");
        }
    }
    #endregion
    #region 触发事件
    /// <summary>
    /// 触发无泛型事件
    /// </summary>
    /// <param Name="eventName">事件名称</param>
    public void InvokeEvent(string eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo).Action?.Invoke();
        }
        else
        {
            Debug.LogWarning("尝试触发不存在的事件");
        }
    }
    /// <summary>
    /// 触发单泛型事件
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    public void InvokeEvent<T>(string eventName, T t)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T>).Action?.Invoke(t);
        }
        else
        {
            Debug.LogWarning("尝试触发不存在的事件");
        }
    }
    /// <summary>
    /// 触发双泛型事件
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <typeparam Name="T2">第二个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    public void InvokeEvent<T1, T2>(string eventName, T1 t1, T2 t2)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T1, T2>).Action?.Invoke(t1, t2);
        }
        else
        {
            Debug.LogWarning("尝试触发不存在的事件");
        }
    }
    /// <summary>
    /// 触发三泛型事件
    /// </summary>
    /// <typeparam Name="T1">第一个参数的类型</typeparam>
    /// <typeparam Name="T2">第二个参数的类型</typeparam>
    /// <typeparam Name="T3">第三个参数的类型</typeparam>
    /// <param Name="eventName">事件名称</param>
    public void InvokeEvent<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
    {
        if (eventDic.ContainsKey(eventName))
        {
            (eventDic[eventName] as EventInfo<T1, T2, T3>).Action?.Invoke(t1, t2, t3);
        }
        else
        {
            Debug.LogWarning("尝试触发不存在的事件");
        }
    }
    #endregion
    /// <summary>
    /// 清空事件观察者
    /// </summary>
    /// <param Name="eventName">事件名称</param>
    public void ClearEventLinstener(string eventName)
    {
        if (eventDic.ContainsKey(eventName))
        {
            eventDic.Remove(eventName);
        }
        else
        {
            Debug.LogWarning("尝试清空不存在的事件");
        }
    }
}
