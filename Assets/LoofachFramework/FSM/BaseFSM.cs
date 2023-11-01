using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.FSM
{
    /// <summary>
    /// 状态机基类
    /// 状态转换的实现
    /// 以及一些在大多状态下生效、不需要和其他物体传递参数的方法
    /// </summary>
    /// <typeparam Name="stateList">状态枚举</typeparam>
    /// <typeparam Name="parameters">参数</typeparam>
    public abstract class BaseFSM<stateList, parameters> : MonoBehaviour where parameters : BaseParameters
    {
        public parameters parameter;
        protected Dictionary<stateList, IState> StatesDic = new Dictionary<stateList, IState>();
        protected int flip = 1;
        [HideInInspector] public GameObject player;
        [SerializeField] public IState currentState;
        private Vector3 scale_right;
        private Vector3 scale_left;
        protected virtual void Awake()
        {
            scale_right = transform.localScale;
            scale_left = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            player = GameObject.FindGameObjectWithTag("Player");
            parameter = this.GetComponent<parameters>();
            parameter.animator = this.GetComponent<Animator>();
            parameter.rg2d = this.GetComponent<Rigidbody2D>();
        }
        protected virtual void FixedUpdate()
        {
            if (!(parameter.isDead || parameter.isDizzy))
            {                
                currentState.OnFixedUpdate();
                FlipTo();
                sight();
            }
            GroundCheck();
            parameter.anm_info = parameter.animator.GetCurrentAnimatorStateInfo(0);
            //movetowards不会改变刚体的速度                        
        }
        /// <summary>
        /// 状态的切换
        /// </summary>
        /// <param Name="nextState">下一个状态在字典中的key</param>
        public void TransitionState(stateList nextState)
        {
            if (currentState != null)
            {
                currentState.OnExit();
            }
            currentState = StatesDic[nextState];
            currentState.OnEnter();
        }
        #region 所有敌人通用的，且大多数状态通用的方法
        /// <summary>
        /// 面朝目标
        /// </summary>
        protected void FlipTo()
        {
            if (parameter.target != null)
            {
                if (parameter.target.transform.position.x > this.transform.position.x)
                {
                    this.transform.localScale = scale_right;
                    flip = 1;
                }
                else if (parameter.target.transform.position.x < this.transform.position.x)
                {
                    this.transform.localScale = scale_left;
                    flip = -1;
                }
            }
        }
        /// <summary>
        /// 敌人的视野判定和发现玩家的行为
        /// </summary>
        protected abstract void sight();
        /// <summary>
        /// 地形判断以及行为
        /// </summary>
        protected virtual void GroundCheck()
        {
            parameter.ground_check_left = Physics2D.Raycast((Vector2)transform.position + Vector2.down * parameter._ground_check_vertical_offset + Vector2.left * parameter._ground_check_horizontal_offset, Vector2.down, parameter._ground_check_length, parameter._ground_mask);
            parameter.ground_check_right = Physics2D.Raycast((Vector2)transform.position + Vector2.down * parameter._ground_check_vertical_offset + Vector2.right * parameter._ground_check_horizontal_offset, Vector2.down, parameter._ground_check_length, parameter._ground_mask);
            int dir = Math.Sign(transform.localScale.x);
            parameter._onEdge = !(dir < 0 ? parameter.ground_check_left : parameter.ground_check_right);   //如果不是两边都能检测到地面，判断为在边缘
            parameter._onBackEdge = !(dir > 0 ? parameter.ground_check_left : parameter.ground_check_right);   //如果不是两边都能检测到地面，判断为在边缘
        }
        /// <summary>
        /// 敌人被弹反
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerator getParry();
        //造成伤害的三个方法        
        protected abstract void anm_show_hitbox(int i);
        protected abstract void anm_hide_hitbox(int i);
        protected abstract void OnTriggerEnter2D(Collider2D other);
        protected abstract void anm_back_to_idle();
        protected abstract void anm_SetTrigger(string name);
        protected abstract void OnDrawGizmos();
        #endregion

    }
}
