using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StarryFramework
{
    public abstract class FSMState<T> where T : class
    {
        /// <summary>
        /// 创建状态机的时候调用
        /// </summary>
        /// <param Name="fsm">管理当前状态的状态机</param>
        protected internal virtual void OnInit(IFSM<T> fsm) { }

        /// <summary>
        /// 进入状态时调用
        /// </summary>
        /// <param Name="fsm">管理当前状态的状态机</param>
        protected internal virtual void OnEnter(IFSM<T> fsm) { }

        /// <summary>
        /// 处于当前状态时每一帧调用
        /// </summary>
        /// <param Name="fsm">管理当前状态的状态机</param>
        protected internal virtual void OnUpdate(IFSM<T> fsm) { }

        /// <summary>
        /// 离开状态时调用
        /// </summary>
        /// <param Name="fsm">管理当前状态的状态机</param>
        /// <param Name="isShutdown">是否为注销状态机时调用</param>
        protected internal virtual void OnLeave(IFSM<T> fsm, bool isShutdown) { }

        /// <summary>
        /// 注销状态机时调用
        /// </summary>
        /// <param Name="fsm">管理当前状态的状态机</param>
        protected internal virtual void OnDestroy(IFSM<T> fsm) { }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <typeparam Name="S">目标状态类型</typeparam>
        /// <param Name="fsm">管理当前状态的状态机</param>
        protected internal virtual void ChangeState<S>(IFSM<T> fsm) where S : FSMState<T> 
        { 
            FSM<T> _fsm = fsm as FSM<T>;
            if(_fsm != null)
            {
                _fsm.ChangeState<S>();
            }
            else
            {
                FrameworkManager.Debugger.LogError("fsm is null!");
            }
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param Name="fsm">管理当前状态的状态机</param>
        /// <param Name="stateType">目标状态类型</param>
        protected internal virtual void ChangeState(IFSM<T> fsm, Type stateType)
        {
            FSM<T> _fsm = fsm as FSM<T>;
            if(_fsm == null)
            {
                FrameworkManager.Debugger.LogError("fsm is null!");
            }
            else if(stateType == null)
            {
                FrameworkManager.Debugger.LogError("TimerState type is null!");
            }
            else
            {
                _fsm.ChangeState(stateType);
            }
        }
    }
    
}

