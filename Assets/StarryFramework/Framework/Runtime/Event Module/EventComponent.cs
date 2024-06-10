using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace StarryFramework
{
    public class EventComponent : BaseComponent
    {

        private EventManager _manager = null;

        private EventManager manager
        {
            get 
            { 
                if (_manager == null)
                {
                    _manager = FrameworkManager.GetManager<EventManager>();
                }    
                return _manager; 
            }
        }

        private bool hasBoundTriggers = false;

        private string lastEventName = "Null";
        private string lastEventParam = "Null";

        public string LastEventName => lastEventName;
        public string LastEventParam => lastEventParam;



        private Dictionary<string,UnityAction> triggerActions = new Dictionary<string, UnityAction>();
        

        protected override void Awake()
        {
            base.Awake();
            if (_manager == null)
            {
                _manager = FrameworkManager.GetManager<EventManager>();
            }
        }
        private void Start()
        {
            FrameworkManager.EventManager.AddEventListener(FrameworkEvent.OnLoadData, LoadPlayerData);
            FrameworkManager.EventManager.AddEventListener(FrameworkEvent.OnUnloadData, UnloadPlayerData);
        }
        internal override void Shutdown()
        {
            triggerActions.Clear();
            FrameworkManager.EventManager.RemoveEventListener(FrameworkEvent.OnLoadData, LoadPlayerData);
            FrameworkManager.EventManager.RemoveEventListener(FrameworkEvent.OnUnloadData, UnloadPlayerData);
        }

        /// <summary>
        /// 只有启用Save模块才会被调用，用于将PlayerData数据与Save模块同步
        /// </summary>
        private void LoadPlayerData()
        {
            if (hasBoundTriggers)
            {
                UnloadPlayerData();
            }
            PlayerData data = FrameworkManager.GetManager<SaveManager>().PlayerData;
            InitActionsDic(data);
            BindTriggerActions();
        }
        private void BindTriggerActions()
        {
            foreach (var a in triggerActions)
            {
                AddEventListener(a.Key, a.Value);
            }

            hasBoundTriggers = true;
        }
        private void InitActionsDic(PlayerData data)
        {
            foreach (System.Reflection.FieldInfo info in data.GetType().GetFields())
            {
                if (info.FieldType == typeof(bool))
                {
                    if (!triggerActions.ContainsKey(info.Name))
                        triggerActions.Add(info.Name, new UnityAction(() => info.SetValue(data, true)));
                }
            }
        }
        private void UnbindTriggerActions()
        {
            foreach (var a in triggerActions)
            {
                RemoveEventListener(a.Key, a.Value);
            }

            hasBoundTriggers = false;
        }
        private void UnloadPlayerData()
        {
            UnbindTriggerActions();
            triggerActions.Clear();
        }

        public Dictionary<string, Dictionary<string, int>> GetAllEventsInfo()
        {
            return manager.GetAllEventsInfo();
        }


        #region 添加事件监听

        /// <summary>
        /// 添加事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener(string eventName, UnityAction action)
        {
            manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加一个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T>(string eventName, UnityAction<T> action)
        {
            manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加两个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
        {
            manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加三个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加四个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            manager.AddEventListener(eventName, action);
        }

        #endregion

        #region 删除事件监听

        /// <summary>
        /// 删除事件监听
        /// </summary>
        public void RemoveEventListener(string eventName, UnityAction action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除一个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除两个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除三个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除四个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            manager.RemoveEventListener(eventName, action);
        }


        #endregion
         
        #region 触发事件

        /// <summary>
        /// 触发事件
        /// </summary>
        public void InvokeEvent(string eventName)
        {
            lastEventName = eventName;
            lastEventParam = "None";
            manager.InvokeEvent(eventName);
        }

        /// <summary>
        /// 触发一个参数的事件
        /// </summary>
        public void InvokeEvent<T>(string eventName, T t)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T).ToString();
            manager.InvokeEvent(eventName,t);
        }

        /// <summary>
        /// 触发两个参数的事件
        /// </summary>
        public void InvokeEvent<T1, T2>(string eventName, T1 t1, T2 t2)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1).ToString()+" , "+ typeof(T2).ToString();
            manager.InvokeEvent(eventName, t1, t2);
        }

        /// <summary>
        /// 触发三个参数的事件
        /// </summary>
        public void InvokeEvent<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1).ToString() + " , " + typeof(T2).ToString() + " , " + typeof(T3).ToString();
            manager.InvokeEvent(eventName, t1, t2, t3);
        }

        /// <summary>
        /// 触发四个参数的事件
        /// </summary>
        public void InvokeEvent<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1).ToString() + " , " + typeof(T2).ToString() + " , " + typeof(T3).ToString()+ " , " + typeof(T4).ToString();
            manager.InvokeEvent(eventName, t1, t2, t3, t4);
        }

        #endregion

        #region 触发延时事件

        /// <summary>
        /// 延时触发事件
        /// </summary>
        /// <param Name="delayTime">延时时间，秒为单位</param>
        /// <param Name="realtime">是否采用真实时间（unscaledTime）</param>
        public void InvokeDelayedEvent(string eventName, float delayTime, bool realtime = false)
        {
            lastEventName = eventName;
            lastEventParam = "null";
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if(realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else 
                    yield return new WaitForSeconds(delayTime);

                manager.InvokeEvent(eventName);
            }

        }

        /// <summary>
        /// 延时触发一个参数的事件
        /// </summary>
        /// <param Name="delayTime">延时时间，秒为单位</param>
        /// <param Name="realtime">是否采用真实时间（unscaledTime）</param>
        public void InvokeDelayedEvent<T>(string eventName, T t, float delayTime, bool realtime = false)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T).ToString();
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);

                manager.InvokeEvent(eventName, t);
            }
        }

        /// <summary>
        /// 延时触发两个参数事件
        /// </summary>
        /// <param Name="delayTime">延时时间，秒为单位</param>
        /// <param Name="realtime">是否采用真实时间（unscaledTime）</param>
        public void InvokeDelayedEvent<T1, T2>(string eventName, T1 t1, T2 t2, float delayTime, bool realtime = false)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1).ToString() + " , " + typeof(T2).ToString();
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);

                manager.InvokeEvent(eventName, t1, t2);
            }

        }

        /// <summary>
        /// 延时触发三个参数的事件
        /// </summary>
        /// <param Name="delayTime">延时时间，秒为单位</param>
        /// <param Name="realtime">是否采用真实时间（unscaledTime）</param>
        public void InvokeDelayedEvent<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3, float delayTime, bool realtime = false)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1).ToString() + " , " + typeof(T2).ToString() + " , " + typeof(T3).ToString();
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);

                manager.InvokeEvent(eventName, t1, t2, t3);
            }
        }

        /// <summary>
        /// 延时触发四个参数的事件
        /// </summary>
        /// <param Name="delayTime">延时时间，秒为单位</param>
        /// <param Name="realtime">是否采用真实时间（unscaledTime）</param>
        public void InvokeDelayedEvent<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4, float delayTime, bool realtime = false)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1).ToString() + " , " + typeof(T2).ToString() + " , " + typeof(T3).ToString() + " , " + typeof(T4).ToString();
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);

                manager.InvokeEvent(eventName, t1, t2, t3, t4);
            }
        }

        #endregion

        #region 清除事件监听

        /// <summary>
        /// 清除某个事件的所有监听
        /// </summary>
        public void ClearAllEventLinsteners(string eventName)
        {
            manager.ClearAllEventLinsteners(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有零参数监听
        /// </summary>
        public void ClearEventListeners(string eventName)
        {
            manager.ClearEventListeners(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有单参数监听
        /// </summary>
        public void ClearEventListeners<T>(string eventName)
        {
            manager.ClearEventListeners<T>(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有双参数监听
        /// </summary>
        public void ClearEventListeners<T1, T2>(string eventName)
        {
            manager.ClearEventListeners<T1, T2>(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有三参数监听
        /// </summary>
        public void ClearEventListeners<T1, T2, T3>(string eventName)
        {
            manager.ClearEventListeners<T1, T2, T3>(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有四参数监听
        /// </summary>
        public void ClearEventListeners<T1, T2, T3, T4>(string eventName)
        {
            manager.ClearEventListeners<T1, T2, T3, T4>(eventName);
        }


        #endregion

    }
}