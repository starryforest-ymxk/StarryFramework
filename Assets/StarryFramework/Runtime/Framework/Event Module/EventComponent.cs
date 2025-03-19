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
        /// ֻ������Saveģ��Żᱻ���ã����ڽ�PlayerData������Saveģ��ͬ��
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
                        triggerActions.Add(info.Name, () => info.SetValue(data, true));
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


        #region ����¼�����

        /// <summary>
        /// ����¼�������action����������������Lambda���ʽ
        /// </summary>
        public void AddEventListener(string eventName, UnityAction action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// ���һ���������¼�������action����������������Lambda���ʽ
        /// </summary>
        public void AddEventListener<T>(string eventName, UnityAction<T> action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// ��������������¼�������action����������������Lambda���ʽ
        /// </summary>
        public void AddEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// ��������������¼�������action����������������Lambda���ʽ
        /// </summary>
        public void AddEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            Manager.AddEventListener(eventName, action);
        }

        /// <summary>
        /// ����ĸ��������¼�������action����������������Lambda���ʽ
        /// </summary>
        public void AddEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            Manager.AddEventListener(eventName, action);
        }

        #endregion

        #region ɾ���¼�����

        /// <summary>
        /// ɾ���¼�����
        /// </summary>
        public void RemoveEventListener(string eventName, UnityAction action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// ɾ��һ���������¼�����
        /// </summary>
        public void RemoveEventListener<T>(string eventName, UnityAction<T> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// ɾ�������������¼�����
        /// </summary>
        public void RemoveEventListener<T1, T2>(string eventName, UnityAction<T1, T2> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// ɾ�������������¼�����
        /// </summary>
        public void RemoveEventListener<T1, T2, T3>(string eventName, UnityAction<T1, T2, T3> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }

        /// <summary>
        /// ɾ���ĸ��������¼�����
        /// </summary>
        public void RemoveEventListener<T1, T2, T3, T4>(string eventName, UnityAction<T1, T2, T3, T4> action)
        {
            if (FrameworkManager.FrameworkState == FrameworkState.ShutDown)
                return;
            Manager.RemoveEventListener(eventName, action);
        }


        #endregion
         
        #region �����¼�

        /// <summary>
        /// �����¼�
        /// </summary>
        public void InvokeEvent(string eventName)
        {
            lastEventName = eventName;
            lastEventParam = "None";
            Manager.InvokeEvent(eventName);
        }

        /// <summary>
        /// ����һ���������¼�
        /// </summary>
        public void InvokeEvent<T>(string eventName, T t)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T).ToString();
            Manager.InvokeEvent(eventName,t);
        }

        /// <summary>
        /// ���������������¼�
        /// </summary>
        public void InvokeEvent<T1, T2>(string eventName, T1 t1, T2 t2)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1)+" , "+ typeof(T2);
            Manager.InvokeEvent(eventName, t1, t2);
        }

        /// <summary>
        /// ���������������¼�
        /// </summary>
        public void InvokeEvent<T1, T2, T3>(string eventName, T1 t1, T2 t2, T3 t3)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1) + " , " + typeof(T2) + " , " + typeof(T3);
            Manager.InvokeEvent(eventName, t1, t2, t3);
        }

        /// <summary>
        /// �����ĸ��������¼�
        /// </summary>
        public void InvokeEvent<T1, T2, T3, T4>(string eventName, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            lastEventName = eventName;
            lastEventParam = typeof(T1) + " , " + typeof(T2) + " , " + typeof(T3)+ " , " + typeof(T4);
            Manager.InvokeEvent(eventName, t1, t2, t3, t4);
        }

        #endregion

        #region ������ʱ�¼�

        /// <summary>
        /// ��ʱ�����¼�
        /// </summary>
        /// <param Name="delayTime">��ʱʱ�䣬��Ϊ��λ</param>
        /// <param Name="realtime">�Ƿ������ʵʱ�䣨unscaledTime��</param>
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
        /// ��ʱ����һ���������¼�
        /// </summary>
        /// <param Name="delayTime">��ʱʱ�䣬��Ϊ��λ</param>
        /// <param Name="realtime">�Ƿ������ʵʱ�䣨unscaledTime��</param>
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
        /// ��ʱ�������������¼�
        /// </summary>
        /// <param Name="delayTime">��ʱʱ�䣬��Ϊ��λ</param>
        /// <param Name="realtime">�Ƿ������ʵʱ�䣨unscaledTime��</param>
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
        /// ��ʱ���������������¼�
        /// </summary>
        /// <param Name="delayTime">��ʱʱ�䣬��Ϊ��λ</param>
        /// <param Name="realtime">�Ƿ������ʵʱ�䣨unscaledTime��</param>
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
        /// ��ʱ�����ĸ��������¼�
        /// </summary>
        /// <param Name="delayTime">��ʱʱ�䣬��Ϊ��λ</param>
        /// <param Name="realtime">�Ƿ������ʵʱ�䣨unscaledTime��</param>
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

        #region ����¼�����

        /// <summary>
        /// ���ĳ���¼������м���
        /// </summary>
        public void ClearAllEventLinsteners(string eventName)
        {
            Manager.ClearAllEventLinsteners(eventName);
        }

        /// <summary>
        /// ���ĳ���¼����������������
        /// </summary>
        public void ClearEventListeners(string eventName)
        {
            Manager.ClearEventListeners(eventName);
        }

        /// <summary>
        /// ���ĳ���¼������е���������
        /// </summary>
        public void ClearEventListeners<T>(string eventName)
        {
            Manager.ClearEventListeners<T>(eventName);
        }

        /// <summary>
        /// ���ĳ���¼�������˫��������
        /// </summary>
        public void ClearEventListeners<T1, T2>(string eventName)
        {
            Manager.ClearEventListeners<T1, T2>(eventName);
        }

        /// <summary>
        /// ���ĳ���¼�����������������
        /// </summary>
        public void ClearEventListeners<T1, T2, T3>(string eventName)
        {
            Manager.ClearEventListeners<T1, T2, T3>(eventName);
        }

        /// <summary>
        /// ���ĳ���¼��������Ĳ�������
        /// </summary>
        public void ClearEventListeners<T1, T2, T3, T4>(string eventName)
        {
            Manager.ClearEventListeners<T1, T2, T3, T4>(eventName);
        }


        #endregion

    }
}