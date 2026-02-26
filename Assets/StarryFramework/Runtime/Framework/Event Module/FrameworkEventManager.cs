using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    interface IFrameworkEvemtInfo { }
    public class FrameworkEventInfo : IFrameworkEvemtInfo
    {
        public UnityEvent Event = new UnityEvent();
    }
    public class FrameworkEventInfo<T> : IFrameworkEvemtInfo
    {
        public UnityEvent<T> Event = new UnityEvent<T>();
    }
    public class FrameworkEventInfo<T1, T2> : IFrameworkEvemtInfo
    {
        public UnityEvent<T1, T2> Event = new UnityEvent<T1, T2>();
    }
    
    internal class FrameworkEventManager 
    {

        private Dictionary<string, IFrameworkEvemtInfo> eventDic = new();
        
        internal void ShutDown()
        {
            eventDic.Clear();
        }

        internal void AddEventListener(string eventName, UnityAction action)
        {
            if (eventDic.ContainsKey(eventName))
            {
                (eventDic[eventName] as FrameworkEventInfo).Event.AddListener(action);
            }
            else
            {
                eventDic.Add(eventName, new FrameworkEventInfo());
                (eventDic[eventName] as FrameworkEventInfo).Event.AddListener(action);
            }
        }

        internal void AddEventListener<T>(string eventName, UnityAction<T> action)
        {
            if (eventDic.ContainsKey(eventName))
            {
                (eventDic[eventName] as FrameworkEventInfo<T>).Event.AddListener(action);
            }
            else
            {
                eventDic.Add(eventName, new FrameworkEventInfo<T>());
                (eventDic[eventName] as FrameworkEventInfo<T>).Event.AddListener(action);
            }
        }
        
        internal void AddEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
        {
            if (eventDic.ContainsKey(eventName))
            {
                (eventDic[eventName] as FrameworkEventInfo<T1, T2>).Event.AddListener(action);
            }
            else
            {
                eventDic.Add(eventName, new FrameworkEventInfo<T1, T2>());
                (eventDic[eventName] as FrameworkEventInfo<T1, T2>).Event.AddListener(action);
            }
        }

        internal void RemoveEventListener(string eventName, UnityAction action)
        {
            if (eventDic.ContainsKey(eventName) && eventDic[eventName] != null)
            {
                (eventDic[eventName] as FrameworkEventInfo).Event.RemoveListener(action);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"Framework Event Manager : 尝试删除不存在的事件[{eventName}]");
            }
        }

        internal void RemoveEventListener<T>(string eventName, UnityAction<T> action)
        {
            if (eventDic.ContainsKey(eventName) && eventDic[eventName] != null)
            {
                (eventDic[eventName] as FrameworkEventInfo<T>).Event.RemoveListener(action);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"Framework Event Manager : 尝试删除不存在的事件[{eventName}]");
            }
        }

        internal void InvokeEvent(string eventName)
        {
            if (eventDic.ContainsKey(eventName))
            {
                (eventDic[eventName] as FrameworkEventInfo).Event?.Invoke();
            }
            else
            {
                FrameworkManager.Debugger.Log($"Framework Event Manager : 尝试触发不存在的事件[{eventName}]");
            }

            if(FrameworkManager.Setting.ModuleInUse(ModuleType.Event) && FrameworkManager.Setting.InternalEventTrigger)
            {
                FrameworkComponent.GetComponent<EventComponent>().InvokeEvent(eventName);
            }

        }

        internal void InvokeEvent<T>(string eventName ,T t)
        {
            if (eventDic.ContainsKey(eventName))
            {
                (eventDic[eventName] as FrameworkEventInfo<T>).Event?.Invoke(t);
            }
            else
            {
                FrameworkManager.Debugger.Log($"Framework Event Manager : 尝试触发不存在的事件[{eventName}]");
            }

            if (FrameworkManager.Setting.ModuleInUse(ModuleType.Event) && FrameworkManager.Setting.InternalEventTrigger)
            {
                FrameworkComponent.GetComponent<EventComponent>().InvokeEvent<T>(eventName,t);
            }

        }
        
        internal void InvokeEvent<T1, T2>(string eventName ,T1 t1, T2 t2)
        {
            if (eventDic.ContainsKey(eventName))
            {
                (eventDic[eventName] as FrameworkEventInfo<T1, T2>).Event?.Invoke(t1, t2);
            }
            else
            {
                FrameworkManager.Debugger.Log($"Framework Event Manager : 尝试触发不存在的事件[{eventName}]");
            }

            if (FrameworkManager.Setting.ModuleInUse(ModuleType.Event) && FrameworkManager.Setting.InternalEventTrigger)
            {
                FrameworkComponent.GetComponent<EventComponent>().InvokeEvent<T1, T2>(eventName,t1,t2);
            }

        }

        internal void ClearEventLinstener(string eventName)
        {
            if (eventDic.ContainsKey(eventName))
            {
                eventDic.Remove(eventName);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"Framework Event Manager : 尝试清空不存在的事件[{eventName}]");
            }
        }

    }
}
