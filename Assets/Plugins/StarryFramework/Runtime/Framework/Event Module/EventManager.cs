using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    #region EventBase

    internal abstract class IEvent {}
    internal class EventInfo : IEvent
    {
        public UnityEvent Event = new UnityEvent();
    }
    internal class EventInfo<T> : IEvent
    {
        public UnityEvent<T> Event = new UnityEvent<T>();
    }
    internal class EventInfo<T1, T2> : IEvent
    {
        public UnityEvent<T1, T2> Event = new UnityEvent<T1, T2>();
    }
    internal class EventInfo<T1, T2, T3> : IEvent
    {
        public UnityEvent<T1, T2, T3> Event = new UnityEvent<T1, T2, T3>();
    }
    internal class EventInfo<T1, T2, T3, T4> : IEvent
    {
        public UnityEvent<T1, T2, T3, T4> Event = new UnityEvent<T1, T2, T3, T4>();
    }
    #endregion

    internal class EventManager : IManager
    {
        private Dictionary<string, IEvent> eventDic = new Dictionary<string, IEvent>();
        private Dictionary<string, Dictionary<string, int>> eventInfoDic = new Dictionary<string, Dictionary<string, int>>();

        private const string paramFlag_0 = "_0parameter";
        private const string paramFlag_1 = "_1parameter_";
        private const string paramFlag_2 = "_2parameters_";
        private const string paramFlag_3 = "_3parameters_";
        private const string paramFlag_4 = "_4parameters_";

        void IManager.Awake()
        {

        }

        void IManager.Init()
        {

        }

        void IManager.Update()
        {

        }

        void IManager.ShutDown()
        {
            eventDic.Clear();
            eventInfoDic.Clear();
        }
        
        #region GetFullName

        private string GetFullName(string eventName)
        {
            return eventName + paramFlag_0;
        }

        private string GetFullName<T>(string eventName)
        {
            return eventName + paramFlag_1 + typeof(T).ToString();
        }

        private string GetFullName<T1, T2>(string eventName)
        {
            return eventName + paramFlag_2 + typeof(T1).ToString() + "_" + typeof(T2).ToString();
        }

        private string GetFullName<T1, T2, T3>(string eventName)
        {
            return eventName + paramFlag_3 + typeof(T1).ToString() + "_" + typeof(T2).ToString() + "_" + typeof(T3).ToString(); 
        }
        private string GetFullName<T1, T2, T3, T4>(string eventName)
        {
            return eventName + paramFlag_4 + typeof(T1).ToString() + "_" + typeof(T2).ToString() + "_" + typeof(T3).ToString() + "_" + typeof(T4).ToString();
        }

        #endregion

        internal Dictionary<string, Dictionary<string, int>> GetAllEventsInfo()
        {
            return new Dictionary<string, Dictionary<string, int>>(eventInfoDic);
        }

        #region 添加事件监听

        internal void AddEventListener(string eventName, UnityAction action)
        {
            string eventFullName = GetFullName(eventName);

            if(!eventInfoDic.ContainsKey(eventName))
            {
                eventInfoDic.Add(eventName, new Dictionary<string, int>());
            }

            if (!eventDic.ContainsKey(eventFullName))
            {
                eventInfoDic[eventName].Add(eventFullName,0);
                eventDic.Add(eventFullName, new EventInfo());
            }

            eventInfoDic[eventName][eventFullName]++;

            (eventDic[eventFullName] as EventInfo).Event.AddListener(action);
        }

        internal void AddEventListener<T>(string eventName, UnityAction<T> action)
        {
            string eventFullName = GetFullName<T>(eventName);

            if (!eventInfoDic.ContainsKey(eventName))
            {
                eventInfoDic.Add(eventName, new Dictionary<string, int>());
            }

            if (!eventDic.ContainsKey(eventFullName))
            {
                eventInfoDic[eventName].Add(eventFullName,0);
                eventDic.Add(eventFullName, new EventInfo<T>());
            }

            eventInfoDic[eventName][eventFullName]++;

            (eventDic[eventFullName] as EventInfo<T>).Event.AddListener(action);
        }

        internal void AddEventListener<T1,T2>(string eventName, UnityAction<T1, T2> action)
        {
            string eventFullName = GetFullName<T1, T2>(eventName);

            if (!eventInfoDic.ContainsKey(eventName))
            {
                eventInfoDic.Add(eventName, new Dictionary<string, int>());
            }

            if (!eventDic.ContainsKey(eventFullName))
            {
                eventInfoDic[eventName].Add(eventFullName,0);
                eventDic.Add(eventFullName, new EventInfo<T1, T2>());
            }

            eventInfoDic[eventName][eventFullName]++;

            (eventDic[eventFullName] as EventInfo<T1, T2>).Event.AddListener(action);
        }

        internal void AddEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            string eventFullName = GetFullName<T1, T2, T3>(eventName);

            if (!eventInfoDic.ContainsKey(eventName))
            {
                eventInfoDic.Add(eventName, new Dictionary<string, int>());
            }

            if (!eventDic.ContainsKey(eventFullName))
            {
                eventInfoDic[eventName].Add(eventFullName,0);
                eventDic.Add(eventFullName, new EventInfo<T1, T2, T3>());
            }

            eventInfoDic[eventName][eventFullName]++;

            (eventDic[eventFullName] as EventInfo<T1, T2, T3>).Event.AddListener(action);
        }

        internal void AddEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            string eventFullName = GetFullName<T1,T2, T3, T4>(eventName);

            if (!eventInfoDic.ContainsKey(eventName))
            {
                eventInfoDic.Add(eventName, new Dictionary<string, int>());
            }

            if (!eventDic.ContainsKey(eventFullName))
            {
                eventInfoDic[eventName].Add(eventFullName,0);
                eventDic.Add(eventFullName, new EventInfo<T1, T2, T3, T4>());
            }

            eventInfoDic[eventName][eventFullName]++;

            (eventDic[eventFullName] as EventInfo<T1, T2, T3, T4>).Event.AddListener(action);
        }

        #endregion

        #region 删除事件监听

        internal void RemoveEventListener(string eventName, UnityAction action)
        {
            string eventFullName = GetFullName(eventName);

            if (!eventDic.ContainsKey(eventFullName))
            {
                FrameworkManager.Debugger.Log("尝试删除不存在的事件");
                return;
            }

            (eventDic[eventFullName] as EventInfo).Event.RemoveListener(action);

            eventInfoDic[eventName][eventFullName]--;

            if(eventInfoDic[eventName][eventFullName]==0)
            {
                eventInfoDic[eventName].Remove(eventFullName);
                eventDic.Remove(eventFullName);
            }

            if(eventInfoDic[eventName].Count == 0)
            {
                eventInfoDic.Remove(eventName);
            }
        }

        internal void RemoveEventListener<T>(string eventName, UnityAction<T> action)
        {
            string eventFullName = GetFullName<T>(eventName);

            if (!eventDic.ContainsKey(eventFullName))
            {
                FrameworkManager.Debugger.LogError("尝试删除不存在的事件");
                return;
            }

            (eventDic[eventFullName] as EventInfo<T>).Event.RemoveListener(action);

            eventInfoDic[eventName][eventFullName]--;

            if (eventInfoDic[eventName][eventFullName] == 0)
            {
                eventInfoDic[eventName].Remove(eventFullName);
                eventDic.Remove(eventFullName);
            }

            if (eventInfoDic[eventName].Count == 0)
            {
                eventInfoDic.Remove(eventName);
            }
        }

        internal void RemoveEventListener<T1,T2>(string eventName, UnityAction<T1, T2> action)
        {
            string eventFullName = GetFullName<T1, T2>(eventName);

            if (!eventDic.ContainsKey(eventFullName))
            {
                FrameworkManager.Debugger.LogError("尝试删除不存在的事件");
                return;
            }

            (eventDic[eventFullName] as EventInfo<T1,T2>).Event.RemoveListener(action);

            eventInfoDic[eventName][eventFullName]--;

            if (eventInfoDic[eventName][eventFullName] == 0)
            {
                eventInfoDic[eventName].Remove(eventFullName);
                eventDic.Remove(eventFullName);
            }

            if (eventInfoDic[eventName].Count == 0)
            {
                eventInfoDic.Remove(eventName);
            }
        }

        internal void RemoveEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            string eventFullName = GetFullName<T1, T2, T3>(eventName);

            if (!eventDic.ContainsKey(eventFullName))
            {
                FrameworkManager.Debugger.LogError("尝试删除不存在的事件");
                return;
            }

            (eventDic[eventFullName] as EventInfo<T1, T2, T3>).Event.RemoveListener(action);

            eventInfoDic[eventName][eventFullName]--;

            if (eventInfoDic[eventName][eventFullName] == 0)
            {
                eventInfoDic[eventName].Remove(eventFullName);
                eventDic.Remove(eventFullName);
            }

            if (eventInfoDic[eventName].Count == 0)
            {
                eventInfoDic.Remove(eventName);
            }
        }

        internal void RemoveEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            string eventFullName = GetFullName<T1, T2, T3, T4>(eventName);

            if (!eventDic.ContainsKey(eventFullName))
            {
                FrameworkManager.Debugger.LogError("尝试删除不存在的事件");
                return;
            }

            (eventDic[eventFullName] as EventInfo<T1, T2, T3, T4>).Event.RemoveListener(action);

            eventInfoDic[eventName][eventFullName]--;

            if (eventInfoDic[eventName][eventFullName] == 0)
            {
                eventInfoDic[eventName].Remove(eventFullName);
                eventDic.Remove(eventFullName);
            }

            if (eventInfoDic[eventName].Count == 0)
            {
                eventInfoDic.Remove(eventName);
            }
        }


        #endregion

        #region 触发事件
        internal void InvokeEvent(string eventName)
        {
            string eventFullName = GetFullName(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo).Event?.Invoke();
            }
            else
            {
                FrameworkManager.Debugger.Log($"尝试触发不存在的事件[{eventFullName}]");
            }
        }

        internal void InvokeEvent<T>(string eventName, T t)
        {
            string eventFullName = GetFullName<T>(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T>).Event?.Invoke(t);
            }
            else
            {
                FrameworkManager.Debugger.Log($"尝试触发不存在的事件[{eventFullName}]");
            }
        }

        internal void InvokeEvent<T1, T2>(string eventName, T1 t1, T2 t2)
        {
            string eventFullName = GetFullName<T1, T2>(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T1, T2>).Event?.Invoke(t1, t2);
            }
            else
            {
                FrameworkManager.Debugger.Log($"尝试触发不存在的事件[{eventFullName}]");
            }
        }

        internal void InvokeEvent<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
        {
            string eventFullName = GetFullName<T1, T2, T3>(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T1, T2, T3>).Event?.Invoke(t1, t2, t3);
            }
            else
            {
                FrameworkManager.Debugger.Log($"尝试触发不存在的事件[{eventFullName}]");
            }
        }

        internal void InvokeEvent<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            string eventFullName = GetFullName < T1, T2, T3, T4> (eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T1, T2, T3, T4>).Event?.Invoke(t1, t2, t3, t4);
            }
            else
            {
                FrameworkManager.Debugger.Log($"尝试触发不存在的事件[{eventFullName}]");
            }
        }

        #endregion

        #region 清除事件

        internal void ClearAllEventLinsteners(string eventName)
        {
            if (eventInfoDic.ContainsKey(eventName))
            {
                foreach (var a in eventInfoDic[eventName])
                {
                    eventDic.Remove(a.Key);
                    
                }
                eventInfoDic.Remove(eventName);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"尝试清空不存在的事件[{eventName}]");
            }
        }

        internal void ClearEventListeners(string eventName)
        {
            string eventFullName = GetFullName(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo).Event.RemoveAllListeners();

                eventDic.Remove(eventFullName);

                eventInfoDic[eventName].Remove(eventFullName);

                if (eventInfoDic[eventName].Count == 0)
                {
                    eventInfoDic.Remove(eventName);
                }
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"尝试清空不存在的事件[{eventFullName}]");
            }
        }

        internal void ClearEventListeners<T>(string eventName)
        {
            string eventFullName = GetFullName<T>(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T>).Event.RemoveAllListeners();

                eventDic.Remove(eventFullName);

                eventInfoDic[eventName].Remove(eventFullName);

                if (eventInfoDic[eventName].Count == 0)
                {
                    eventInfoDic.Remove(eventName);
                }
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"尝试清空不存在的事件[{eventFullName}]");
            }
        }

        internal void ClearEventListeners<T1, T2>(string eventName)
        {
            string eventFullName = GetFullName<T1, T2>(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T1, T2>).Event.RemoveAllListeners();

                eventDic.Remove(eventFullName);

                eventInfoDic[eventName].Remove(eventFullName);

                if (eventInfoDic[eventName].Count == 0)
                {
                    eventInfoDic.Remove(eventName);
                }
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"尝试清空不存在的事件[{eventFullName}]");
            }
        }

        internal void ClearEventListeners<T1, T2, T3>(string eventName)
        {
            string eventFullName = GetFullName<T1, T2, T3>(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T1, T2, T3>).Event.RemoveAllListeners();

                eventDic.Remove(eventFullName);

                eventInfoDic[eventName].Remove(eventFullName);

                if (eventInfoDic[eventName].Count == 0)
                {
                    eventInfoDic.Remove(eventName);
                }
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"尝试清空不存在的事件[{eventFullName}]");
            }
        }

        internal void ClearEventListeners<T1, T2, T3, T4>(string eventName)
        {
            string eventFullName = GetFullName<T1, T2, T3, T4>(eventName);

            if (eventDic.ContainsKey(eventFullName))
            {
                (eventDic[eventFullName] as EventInfo<T1, T2, T3, T4>).Event.RemoveAllListeners();

                eventDic.Remove(eventFullName);

                eventInfoDic[eventName].Remove(eventFullName);

                if (eventInfoDic[eventName].Count == 0)
                {
                    eventInfoDic.Remove(eventName);
                }
            }
            else
            {
                FrameworkManager.Debugger.LogWarning($"尝试清空不存在的事件[{eventFullName}]");
            }
        }


        #endregion


    }
}
