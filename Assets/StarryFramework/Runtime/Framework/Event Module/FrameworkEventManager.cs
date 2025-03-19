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
    internal class FrameworkEventManager 
    {

        private Dictionary<string, IFrameworkEvemtInfo> eventDic = new Dictionary<string, IFrameworkEvemtInfo>();


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
