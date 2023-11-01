using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Game.FSM
{
    /// <summary>
    /// 参数基类
    /// 主要用于储存敌人的参数
    /// 以及一些需要传入传出参数的方法
    /// </summary>
    public abstract class BaseParameters : MonoBehaviour
    {

        [HideInInspector] public int health;  //生命值        
        [HideInInspector] public float max_tough; //满韧性
        [HideInInspector] public float current_tough; //当前韧性        
        [HideInInspector] public float sight_distance;    //视野距离
        [HideInInspector] public float sight_half_angel;  //视野角度        
        [HideInInspector] public float getParry_dizzy_time;   //被弹反眩晕时间
        [HideInInspector] public float hit_force; //击退力度（用速度模拟）
        [HideInInspector] public bool _onEdge; //是否在地形边缘
        [HideInInspector] public bool _onBackEdge; //是否在地形边缘
        public float _ground_check_horizontal_offset;
        public float _ground_check_vertical_offset;
        public float _ground_check_length;   //边缘检测的射线长度

        [HideInInspector] public Animator animator;
        [HideInInspector] public Rigidbody2D rg2d;
        [HideInInspector] public AnimatorStateInfo anm_info;
        public LayerMask _ground_mask;   //地面mask
        [HideInInspector] public RaycastHit2D ground_check_left;  //检测左下角地形
        [HideInInspector] public RaycastHit2D ground_check_right; //检测右下角地形
        [HideInInspector] public bool isDead;
        [HideInInspector] public bool isDizzy;
        [HideInInspector] public GameObject target;
        [HideInInspector] public Collider2D[] hitBoxs;
        [HideInInspector] public Transform[] patrol_points;
        [HideInInspector] public Transform[] chase_points;
        public virtual void getHit(int Damege, float tough_reduce)
        {
            health -= Damege;
            current_tough -= tough_reduce;
        }
    }
}