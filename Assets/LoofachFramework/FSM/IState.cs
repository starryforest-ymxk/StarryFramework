using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FSM
{
    /// <summary>
    /// 状态接口
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// 进入状态时执行内容
        /// </summary>
        public void OnEnter();
        /// <summary>
        /// 在状态中每帧执行内容
        /// </summary>
        public void OnFixedUpdate();
        /// <summary>
        /// 退出状态时执行内容
        /// </summary>
        public void OnExit();
    }
}
