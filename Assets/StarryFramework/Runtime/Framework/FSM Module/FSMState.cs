using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace StarryFramework
{
    public abstract class FSMState<T> where T : class
    {
        /// <summary>
        /// ����״̬����ʱ�����
        /// </summary>
        /// <param Name="fsm">����ǰ״̬��״̬��</param>
        protected internal virtual void OnInit(IFSM<T> fsm) { }

        /// <summary>
        /// ����״̬ʱ����
        /// </summary>
        /// <param Name="fsm">����ǰ״̬��״̬��</param>
        protected internal virtual void OnEnter(IFSM<T> fsm) { }

        /// <summary>
        /// ���ڵ�ǰ״̬ʱÿһ֡����
        /// </summary>
        /// <param Name="fsm">����ǰ״̬��״̬��</param>
        protected internal virtual void OnUpdate(IFSM<T> fsm) { }

        /// <summary>
        /// �뿪״̬ʱ����
        /// </summary>
        /// <param Name="fsm">����ǰ״̬��״̬��</param>
        /// <param Name="isShutdown">�Ƿ�Ϊע��״̬��ʱ����</param>
        protected internal virtual void OnLeave(IFSM<T> fsm, bool isShutdown) { }

        /// <summary>
        /// ע��״̬��ʱ����
        /// </summary>
        /// <param Name="fsm">����ǰ״̬��״̬��</param>
        protected internal virtual void OnDestroy(IFSM<T> fsm) { }

        /// <summary>
        /// �л�״̬
        /// </summary>
        /// <typeparam Name="S">Ŀ��״̬����</typeparam>
        /// <param Name="fsm">����ǰ״̬��״̬��</param>
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
        /// �л�״̬
        /// </summary>
        /// <param Name="fsm">����ǰ״̬��״̬��</param>
        /// <param Name="stateType">Ŀ��״̬����</param>
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

