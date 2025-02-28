using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{

    [DisallowMultipleComponent]
    public class TimerComponent : BaseComponent
    {
        private TimerManager _manager;
        private TimerManager Manager => _manager ??= FrameworkManager.GetManager<TimerManager>();

        [SerializeField] private TimerSettings settings;
        public List<Timer> Timers => Manager.timers;
        public List<TriggerTimer> TriggerTimers => Manager.triggerTimers;
        public List<AsyncTimer> AsyncTimers => Manager.asyncTimers;
        public float ClearUnusedTriggerTimersInterval => Manager.ClearUnusedTriggerTimersInterval;
        public float ClearUnusedAsyncTimersInterval => Manager.ClearUnusedAsyncTimersInterval;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(EditorApplication.isPlaying && _manager != null)
                (_manager as IManager).SetSettings(settings);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<TimerManager>();
            (_manager as IManager).SetSettings(settings);
        }



        #region Timer

        /// <summary>
        /// ע��������ʱ����������ʱһ��Ҫͨ��DeleteTimer�������ռ�ʱ��
        /// </summary>
        /// <param Name="ignoreTimeScale">�Ƿ����ʱ�����ţ�ʹ����ʵʱ�䣩</param>
        /// <param Name="startValue">��ʱ��ʼֵ</param>
        /// <param Name="bindUpdateAction">���ڼ�ʱ����ÿ֡���õĺ���</param>
        /// <returns>��ʱ������</returns>
        public ITimer RegisterTimer(bool ignoreTimeScale = false, float startValue = 0f, UnityAction bindUpdateAction = null)
        {
            return Manager.RegisterTimer(ignoreTimeScale, startValue, bindUpdateAction);
        }

        /// <summary>
        /// ����������ʱ��
        /// </summary>
        /// <param Name="timer">��Ҫ���յ�������ʱ��</param>
        public void DeleteTimer(ITimer timer)
        {
            Manager.DeleteTimer(timer as Timer);
        }

        /// <summary>
        /// ע���������ʱ��
        /// </summary>
        /// <param Name="name">��ʱ������</param>
        /// <param Name="ignoreTimeScale">�Ƿ����ʱ������</param>
        /// <param Name="startValue">��ʱ��ʼֵ</param>
        public void RegisterTimer(string name, bool ignoreTimeScale = false, float startValue = 0f)
        {
            Manager.RegisterTimer(name, ignoreTimeScale, startValue);
        }
        /// <summary>
        /// ���շ�������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void DeleteTimer(string name)
        {
            Manager.DeleteTimer(name);
        }

        /// <summary>
        /// ����������ʱ����Update�¼�
        /// </summary>
        /// <param Name="name"></param>
        /// <param Name="action">Update�¼���ÿ֡����һ��</param>
        public void BindUpdateAction(string name, UnityAction action)
        {
            Manager.BindUpdateAction(name, action);
        }
        /// <summary>
        /// �鿴��������ʱ��״̬
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public TimerState GetTimerState(string name)
        {
            return Manager.GetTimerState(name);
        }
        /// <summary>
        /// �鿴��������ʱ��ʱ��
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public float GetTimerTime(string name)
        {
            return Manager.GetTimerTime(name);
        }
        /// <summary>
        /// ��ͣ��������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void PauseTimer(string name)
        {
            Manager.PauseTimer(name);
        }
        /// <summary>
        /// ֹͣ��ʱ�����ָ���ʼֵ��
        /// </summary>
        /// <param Name="name"></param>
        public void StopTimer(string name)
        {
            Manager.StopTimer(name);
        }
        /// <summary>
        /// �ָ���ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void ResumeTimer(string name)
        {
            Manager.ActivateTimer(name);
        }
        /// <summary>
        /// ������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void StartTimer(string name)
        {
            Manager.StartTimer(name);
        }
        /// <summary>
        /// ���ü�ʱ������ʼֵ
        /// </summary>
        /// <param Name="name"></param>
        public void ResetTimer(string name)
        {
            Manager.ResetTimer(name);
        }

        #endregion

        #region TriggerTimer

        /// <summary>
        /// ע�ᴥ����ʱ��
        /// ��ʱ����Ϊ��������������֣�
        /// ��ע��������ʱ�������ʱ�����Զ�������
        /// ���������ʱ����ѭ���������������Զ����գ����������ʱ��ѭ����������ʹ����Ҫ��ClearUnnamedTriggerTimers()����
        /// ��ע���������ʱ�������ʱ����Ҫ��StartTriggerTimer()�ֶ���������DeleteTriggerTimer()�ֶ�����
        /// </summary>
        /// <param Name="timeDelta">��ʱ�������¼���ʱ��</param>
        /// <param Name="action">�󶨵Ĵ����¼�</param>
        /// <param Name="repeat">�Ƿ�Ϊѭ����������ÿ��timeDelta����һ���¼�</param>
        /// <param Name="name">����Ϊ������Ϊ������ʱ��</param>
        /// <param Name="ignoreTimeScale">�Ƿ����ʱ������</param>
        public void RegisterTriggerTimer(float timeDelta, UnityAction action, bool repeat = false, string name = "", bool ignoreTimeScale = false)
        {
            Manager.RegisterTriggerTimer(timeDelta, action, ignoreTimeScale, repeat, name);
        }

        /// <summary>
        /// ɾ��������������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void DeleteTriggerTimer(string name)
        {
            Manager.DeleteTriggerTimer(name);
        }

        /// <summary>
        /// ��ô�����ʱ��״̬
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public TimerState GetTriggerTimerState(string name)
        {
            return Manager.GetTriggerTimerState(name);
        }

        /// <summary>
        /// ��ͣ������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void PauseTriggerTimer(string name)
        {
            Manager.PauseTriggerTimer(name);
        }

        /// <summary>
        /// �ָ�������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void ResumeTriggerTimer(string name)
        {
            Manager.ActivateTriggerTimer(name);
        }

        /// <summary>
        /// ֹͣ������ʱ�������ָ���ʼֵ��
        /// </summary>
        /// <param Name="name"></param>
        public void StopTriggerTimer(string name)
        {
            Manager.StopTriggerTimer(name);
        }

        /// <summary>
        /// ����������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void StartTriggerTimer(string name)
        {
            Manager.StartTriggerTimer(name);
        }

        /// <summary>
        /// �����������������ʱ��
        /// </summary>
        public void ClearUnnamedTriggerTimers()
        {
            Manager.ClearUnnamedTriggerTimers();
        }


        #endregion

        #region AsyncTimer
        /// <summary>
        /// ע���첽������ʱ��
        /// ��ʱ����Ϊ��������������֣�
        /// ��ע��������ʱ�������ʱ�����Զ�������
        /// ���������ʱ����ѭ���������������Զ����գ����������ʱ��ѭ����������ʹ����Ҫ��ClearUnnamedAsyncTimers()����
        /// ��ע���������ʱ�������ʱ����Ҫ��StartAsyncTimer()�ֶ���������DeleteAsyncTimer()�ֶ�����
        /// </summary>
        /// <param Name="timeDelta">��ʱ�������¼���ʱ��</param>
        /// <param Name="action">�󶨵Ĵ����¼�</param>
        /// <param Name="repeat">�Ƿ�Ϊѭ����������ÿ��timeDelta����һ���¼�</param>
        /// <param Name="name">����Ϊ������Ϊ������ʱ��</param>
        public void RegisterAsyncTimer(float timeDelta, UnityAction action, bool repeat = false, string name = "")
        {
            Manager.RegisterAsyncTimer(timeDelta,action,repeat,name);
        }

        /// <summary>
        /// ɾ���������첽������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        internal void DeleteAsyncTimer(string name)
        {
            Manager.DeleteAsyncTimer(name);
        }

        /// <summary>
        /// ����첽������ʱ��״̬
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public TimerState GetAsyncTimerState(string name)
        {
            return Manager.GetAsyncTimerState(name);
        }

        /// <summary>
        /// �����첽������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void StartAsyncTimer(string name)
        {
            Manager.StartAsyncTimer(name);
        }

        /// <summary>
        /// ֹͣ�첽������ʱ��
        /// </summary>
        /// <param Name="name"></param>
        public void StopAsyncTimer(string name)
        {
            Manager.StopAsyncTimer(name);
        }

        /// <summary>
        /// ������������첽������ʱ��
        /// </summary>
        public void ClearUnnamedAsyncTimers()
        {
            Manager.ClearUnnamedAsyncTimers();
        }

        #endregion

    }
}

