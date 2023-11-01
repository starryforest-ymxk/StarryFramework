using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FSM
{
    /// <summary>
    /// 状态基类
    /// </summary>
    /// <typeparam Name="FSM">状态机类型</typeparam>
    /// <typeparam Name="Parameters">参数</typeparam>
    public abstract class BaseState<FSM, Parameters>
    {
        protected FSM manager;
        protected Parameters parameter;        
    }
}