using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


namespace StarryFramework
{
    public class EventComponent : BaseComponent
    {

        private EventManager _manager;
        private EventManager Manager => _manager ??= FrameworkManager.GetManager<EventManager>(); 
    
        private bool hasBoundTriggers;
        private string lastEventName = "Null";
        private string lastEventParam = "Null";

        public string LastEventName => lastEventName;
        public string LastEventParam => lastEventParam;
        
        private readonly Dictionary<string,UnityAction> triggerActions = new();
        

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<EventManager>();
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
            object data = FrameworkManager.GetManager<SaveManager>().PlayerDataObject;
            if (data == null)
            {
                FrameworkManager.Debugger.LogWarning("玩家数据对象为空，跳过事件布尔字段联动绑定。");
                return;
            }
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
        private void InitActionsDic(object data)
        {
            foreach (System.Reflection.FieldInfo info in data.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (info.FieldType == typeof(bool))
                {
                    if (!triggerActions.ContainsKey(info.Name))
                    {
                        triggerActions.Add(info.Name, () =>
                        {
                            try
                            {
                                info.SetValue(data, true);
                            }
                            catch (Exception exception)
                            {
                                FrameworkManager.Debugger.LogError($"事件布尔字段绑定写入失败。字段: {info.Name}，原因: {exception.Message}");
                            }
                        });
                    }
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
            return Manager.GetAllEventsInfo();
        }


        #region 添加事件监听

        /// <summary>
        /// 添加事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener(string eventName, UnityAction action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加一个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T>(string eventName, UnityAction<T> action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加两个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加三个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// 添加四个参数的事件监听，action请勿传入匿名函数或Lambda表达式
        /// </summary>
        public void AddEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            Manager.AddEventListener(eventName, action);
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
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除一个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除两个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除三个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// 删除四个参数的事件监听
        /// </summary>
        public void RemoveEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
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
            Manager.InvokeEvent(eventName);
        }

        /// <summary>
        /// 触发一个参数的事件
        /// </summary>
        public void InvokeEvent<T>(string eventName, T t)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T).ToString();
            Manager.InvokeEvent(eventName,t);
        }

        /// <summary>
        /// 触发两个参数的事件
        /// </summary>
        public void InvokeEvent<T1, T2>(string eventName, T1 t1, T2 t2)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1)+" , "+ typeof(T2);
            Manager.InvokeEvent(eventName, t1, t2);
        }

        /// <summary>
        /// 触发三个参数的事件
        /// </summary>
        public void InvokeEvent<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1) + " , " + typeof(T2) + " , " + typeof(T3);
            Manager.InvokeEvent(eventName, t1, t2, t3);
        }

        /// <summary>
        /// 触发四个参数的事件
        /// </summary>
        public void InvokeEvent<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1) + " , " + typeof(T2) + " , " + typeof(T3)+ " , " + typeof(T4);
            Manager.InvokeEvent(eventName, t1, t2, t3, t4);
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

                Manager.InvokeEvent(eventName);
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

                Manager.InvokeEvent(eventName, t);
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
            lastEventParam = typeof(T1) + " , " + typeof(T2);
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);

                Manager.InvokeEvent(eventName, t1, t2);
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
            lastEventParam = typeof(T1) + " , " + typeof(T2) + " , " + typeof(T3);
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);

                Manager.InvokeEvent(eventName, t1, t2, t3);
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
            lastEventParam = typeof(T1) + " , " + typeof(T2) + " , " + typeof(T3) + " , " + typeof(T4);
            StartCoroutine(invoke());
            IEnumerator invoke()
            {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delayTime);
                else
                    yield return new WaitForSeconds(delayTime);

                Manager.InvokeEvent(eventName, t1, t2, t3, t4);
            }
        }

        #endregion

        #region 清除事件监听

        /// <summary>
        /// 清除某个事件的所有监听
        /// </summary>
        public void ClearAllEventLinsteners(string eventName)
        {
            Manager.ClearAllEventLinsteners(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有零参数监听
        /// </summary>
        public void ClearEventListeners(string eventName)
        {
            Manager.ClearEventListeners(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有单参数监听
        /// </summary>
        public void ClearEventListeners<T>(string eventName)
        {
            Manager.ClearEventListeners<T>(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有双参数监听
        /// </summary>
        public void ClearEventListeners<T1, T2>(string eventName)
        {
            Manager.ClearEventListeners<T1, T2>(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有三参数监听
        /// </summary>
        public void ClearEventListeners<T1, T2, T3>(string eventName)
        {
            Manager.ClearEventListeners<T1, T2, T3>(eventName);
        }

        /// <summary>
        /// 清除某个事件的所有四参数监听
        /// </summary>
        public void ClearEventListeners<T1, T2, T3, T4>(string eventName)
        {
            Manager.ClearEventListeners<T1, T2, T3, T4>(eventName);
        }


        #endregion

    }
}