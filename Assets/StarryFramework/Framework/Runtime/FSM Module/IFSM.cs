using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public interface IFSM<T> where T : class
    {
        /// <summary>
        /// 获取状态机名称
        /// </summary>
        /// <returns></returns>
        public string GetName();

        /// <summary>
        /// 获取状态机全程（状态机拥有者类型+状态机名称）
        /// </summary>
        /// <returns></returns>
        public string GetFullName();

        /// <summary>
        /// 获得状态机拥有者
        /// </summary>
        /// <returns></returns>
        public T GetOwner();

        /// <summary>
        /// 获取状态数量
        /// </summary>
        /// <returns></returns>
        public int GetStateCount();

        /// <summary>
        /// 状态机是否运行中
        /// </summary>
        /// <returns></returns>
        public bool IsRunning();

        /// <summary>
        /// 状态机是否被摧毁
        /// </summary>
        /// <returns></returns>
        public bool IsDestroyed();

        /// <summary>
        /// 获取当前运行的状态
        /// </summary>
        /// <returns></returns>
        public FSMState<T> GetCurrentState();

        /// <summary>
        /// 获取当前状态已经运行时长（秒）
        /// </summary>
        /// <returns></returns>
        public float GetCurrentStateTime();

        /// <summary>
        /// 查询是否拥有某状态
        /// </summary>
        /// <param Name="stateType"></param>
        /// <returns></returns>
        public bool HasState(Type stateType);

        /// <summary>
        /// 查询是否拥有某状态
        /// </summary>
        /// <typeparam Name="S"></typeparam>
        /// <returns></returns>
        public bool HasState<S>() where S : FSMState<T>;

        /// <summary>
        /// 获取某状态
        /// </summary>
        /// <param Name="stateType"></param>
        /// <returns></returns>
        public FSMState<T> GetState(Type stateType);

        /// <summary>
        /// 获取某状态
        /// </summary>
        /// <typeparam Name="S"></typeparam>
        /// <returns></returns>
        public S GetState<S>() where S : FSMState<T>;

        /// <summary>
        /// 获取所有状态
        /// </summary>
        /// <returns></returns>
        public List<FSMState<T>> GetAllStates();

        /// <summary>
        /// 查询是否有某数据
        /// </summary>
        /// <param Name="key"></param>
        /// <returns></returns>
        public bool HasData(string key);

        /// <summary>
        /// 获取某数据
        /// </summary>
        /// <typeparam Name="D"></typeparam>
        /// <param Name="key"></param>
        /// <returns></returns>
        public D GetData<D>(string key);

        /// <summary>
        /// 设置某数据
        /// </summary>
        /// <typeparam Name="D">数据类型</typeparam>
        /// <param Name="key">键</param>
        /// <param Name="value">值</param>
        public void SetData<D>(string key, D value);

        /// <summary>
        /// 移除某数据
        /// </summary>
        /// <param Name="key"></param>
        public void RemoveData(string key);

        /// <summary>
        /// 开始运行状态机
        /// </summary>
        /// <typeparam Name="S">初始状态类型</typeparam>
        public void Start<S>() where S : FSMState<T>;

        /// <summary>
        /// 开始运行状态机
        /// </summary>
        /// <param Name="stateType">初始状态类型</param>
        public void Start(Type stateType);

    }
}

