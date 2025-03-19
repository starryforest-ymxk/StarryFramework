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
        /// 注册匿名计时器，在用完时一定要通过DeleteTimer方法回收计时器
        /// </summary>
        /// <param Name="ignoreTimeScale">是否忽略时间缩放（使用真实时间）</param>
        /// <param Name="startValue">计时起始值</param>
        /// <param Name="bindUpdateAction">绑定在计时器上每帧调用的函数</param>
        /// <returns>计时器对象</returns>
        public ITimer RegisterTimer(bool ignoreTimeScale = false, float startValue = 0f, UnityAction bindUpdateAction = null)
        {
            return Manager.RegisterTimer(ignoreTimeScale, startValue, bindUpdateAction);
        }

        /// <summary>
        /// 回收匿名计时器
        /// </summary>
        /// <param Name="timer">需要回收的匿名计时器</param>
        public void DeleteTimer(ITimer timer)
        {
            Manager.DeleteTimer(timer as Timer);
        }

        /// <summary>
        /// 注册非匿名计时器
        /// </summary>
        /// <param Name="name">计时器名称</param>
        /// <param Name="ignoreTimeScale">是否忽略时间缩放</param>
        /// <param Name="startValue">计时起始值</param>
        public void RegisterTimer(string name, bool ignoreTimeScale = false, float startValue = 0f)
        {
            Manager.RegisterTimer(name, ignoreTimeScale, startValue);
        }
        /// <summary>
        /// 回收非匿名计时器
        /// </summary>
        /// <param Name="name"></param>
        public void DeleteTimer(string name)
        {
            Manager.DeleteTimer(name);
        }

        /// <summary>
        /// 给非匿名计时器绑定Update事件
        /// </summary>
        /// <param Name="name"></param>
        /// <param Name="action">Update事件，每帧调用一次</param>
        public void BindUpdateAction(string name, UnityAction action)
        {
            Manager.BindUpdateAction(name, action);
        }
        /// <summary>
        /// 查看非匿名计时器状态
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public TimerState GetTimerState(string name)
        {
            return Manager.GetTimerState(name);
        }
        /// <summary>
        /// 查看非匿名计时器时间
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public float GetTimerTime(string name)
        {
            return Manager.GetTimerTime(name);
        }
        /// <summary>
        /// 暂停非匿名计时器
        /// </summary>
        /// <param Name="name"></param>
        public void PauseTimer(string name)
        {
            Manager.PauseTimer(name);
        }
        /// <summary>
        /// 停止计时器（恢复初始值）
        /// </summary>
        /// <param Name="name"></param>
        public void StopTimer(string name)
        {
            Manager.StopTimer(name);
        }
        /// <summary>
        /// 恢复计时器
        /// </summary>
        /// <param Name="name"></param>
        public void ResumeTimer(string name)
        {
            Manager.ActivateTimer(name);
        }
        /// <summary>
        /// 启动计时器
        /// </summary>
        /// <param Name="name"></param>
        public void StartTimer(string name)
        {
            Manager.StartTimer(name);
        }
        /// <summary>
        /// 重置计时器到初始值
        /// </summary>
        /// <param Name="name"></param>
        public void ResetTimer(string name)
        {
            Manager.ResetTimer(name);
        }

        #endregion

        #region TriggerTimer

        /// <summary>
        /// 注册触发计时器
        /// 计时器分为匿名与非匿名两种，
        /// 若注册匿名计时器，则计时器会自动启动；
        /// 如果匿名计时器非循环触发，框架则会自动回收；如果匿名计时器循环触发，则使用完要用ClearUnnamedTriggerTimers()回收
        /// 若注册非匿名计时器，则计时器需要用StartTriggerTimer()手动启动，用DeleteTriggerTimer()手动回收
        /// </summary>
        /// <param Name="timeDelta">计时器触发事件的时间</param>
        /// <param Name="action">绑定的触发事件</param>
        /// <param Name="repeat">是否为循环触发，即每隔timeDelta触发一次事件</param>
        /// <param Name="name">若留为“”则为匿名计时器</param>
        /// <param Name="ignoreTimeScale">是否忽略时间缩放</param>
        public void RegisterTriggerTimer(float timeDelta, UnityAction action, bool repeat = false, string name = "", bool ignoreTimeScale = false)
        {
            Manager.RegisterTriggerTimer(timeDelta, action, ignoreTimeScale, repeat, name);
        }

        /// <summary>
        /// 删除非匿名触发计时器
        /// </summary>
        /// <param Name="name"></param>
        public void DeleteTriggerTimer(string name)
        {
            Manager.DeleteTriggerTimer(name);
        }

        /// <summary>
        /// 获得触发计时器状态
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public TimerState GetTriggerTimerState(string name)
        {
            return Manager.GetTriggerTimerState(name);
        }

        /// <summary>
        /// 暂停触发计时器
        /// </summary>
        /// <param Name="name"></param>
        public void PauseTriggerTimer(string name)
        {
            Manager.PauseTriggerTimer(name);
        }

        /// <summary>
        /// 恢复触发计时器
        /// </summary>
        /// <param Name="name"></param>
        public void ResumeTriggerTimer(string name)
        {
            Manager.ActivateTriggerTimer(name);
        }

        /// <summary>
        /// 停止触发计时器（并恢复初始值）
        /// </summary>
        /// <param Name="name"></param>
        public void StopTriggerTimer(string name)
        {
            Manager.StopTriggerTimer(name);
        }

        /// <summary>
        /// 启动触发计时器
        /// </summary>
        /// <param Name="name"></param>
        public void StartTriggerTimer(string name)
        {
            Manager.StartTriggerTimer(name);
        }

        /// <summary>
        /// 清楚所有匿名触发计时器
        /// </summary>
        public void ClearUnnamedTriggerTimers()
        {
            Manager.ClearUnnamedTriggerTimers();
        }


        #endregion

        #region AsyncTimer
        /// <summary>
        /// 注册异步触发计时器
        /// 计时器分为匿名与非匿名两种，
        /// 若注册匿名计时器，则计时器会自动启动；
        /// 如果匿名计时器非循环触发，框架则会自动回收；如果匿名计时器循环触发，则使用完要用ClearUnnamedAsyncTimers()回收
        /// 若注册非匿名计时器，则计时器需要用StartAsyncTimer()手动启动，用DeleteAsyncTimer()手动回收
        /// </summary>
        /// <param Name="timeDelta">计时器触发事件的时间</param>
        /// <param Name="action">绑定的触发事件</param>
        /// <param Name="repeat">是否为循环触发，即每隔timeDelta触发一次事件</param>
        /// <param Name="name">若留为“”则为匿名计时器</param>
        public void RegisterAsyncTimer(float timeDelta, UnityAction action, bool repeat = false, string name = "")
        {
            Manager.RegisterAsyncTimer(timeDelta,action,repeat,name);
        }

        /// <summary>
        /// 删除非匿名异步触发计时器
        /// </summary>
        /// <param Name="name"></param>
        internal void DeleteAsyncTimer(string name)
        {
            Manager.DeleteAsyncTimer(name);
        }

        /// <summary>
        /// 获得异步触发计时器状态
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public TimerState GetAsyncTimerState(string name)
        {
            return Manager.GetAsyncTimerState(name);
        }

        /// <summary>
        /// 启动异步触发计时器
        /// </summary>
        /// <param Name="name"></param>
        public void StartAsyncTimer(string name)
        {
            Manager.StartAsyncTimer(name);
        }

        /// <summary>
        /// 停止异步触发计时器
        /// </summary>
        /// <param Name="name"></param>
        public void StopAsyncTimer(string name)
        {
            Manager.StopAsyncTimer(name);
        }

        /// <summary>
        /// 清除所有匿名异步触发计时器
        /// </summary>
        public void ClearUnnamedAsyncTimers()
        {
            Manager.ClearUnnamedAsyncTimers();
        }

        #endregion

    }
}

