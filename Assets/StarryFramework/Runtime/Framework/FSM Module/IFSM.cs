using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public interface IFSM<T> where T : class
    {
        /// <summary>
        /// ��ȡ״̬������
        /// </summary>
        /// <returns></returns>
        public string GetName();

        /// <summary>
        /// ��ȡ״̬��ȫ�̣�״̬��ӵ��������+״̬�����ƣ�
        /// </summary>
        /// <returns></returns>
        public string GetFullName();

        /// <summary>
        /// ���״̬��ӵ����
        /// </summary>
        /// <returns></returns>
        public T GetOwner();

        /// <summary>
        /// ��ȡ״̬����
        /// </summary>
        /// <returns></returns>
        public int GetStateCount();

        /// <summary>
        /// ״̬���Ƿ�������
        /// </summary>
        /// <returns></returns>
        public bool IsRunning();

        /// <summary>
        /// ״̬���Ƿ񱻴ݻ�
        /// </summary>
        /// <returns></returns>
        public bool IsDestroyed();

        /// <summary>
        /// ��ȡ��ǰ���е�״̬
        /// </summary>
        /// <returns></returns>
        public FSMState<T> GetCurrentState();

        /// <summary>
        /// ��ȡ��ǰ״̬�Ѿ�����ʱ�����룩
        /// </summary>
        /// <returns></returns>
        public float GetCurrentStateTime();

        /// <summary>
        /// ��ѯ�Ƿ�ӵ��ĳ״̬
        /// </summary>
        /// <param Name="stateType"></param>
        /// <returns></returns>
        public bool HasState(Type stateType);

        /// <summary>
        /// ��ѯ�Ƿ�ӵ��ĳ״̬
        /// </summary>
        /// <typeparam Name="S"></typeparam>
        /// <returns></returns>
        public bool HasState<S>() where S : FSMState<T>;

        /// <summary>
        /// ��ȡĳ״̬
        /// </summary>
        /// <param Name="stateType"></param>
        /// <returns></returns>
        public FSMState<T> GetState(Type stateType);

        /// <summary>
        /// ��ȡĳ״̬
        /// </summary>
        /// <typeparam Name="S"></typeparam>
        /// <returns></returns>
        public S GetState<S>() where S : FSMState<T>;

        /// <summary>
        /// ��ȡ����״̬
        /// </summary>
        /// <returns></returns>
        public List<FSMState<T>> GetAllStates();

        /// <summary>
        /// ��ѯ�Ƿ���ĳ����
        /// </summary>
        /// <param Name="key"></param>
        /// <returns></returns>
        public bool HasData(string key);

        /// <summary>
        /// ��ȡĳ����
        /// </summary>
        /// <typeparam Name="D"></typeparam>
        /// <param Name="key"></param>
        /// <returns></returns>
        public D GetData<D>(string key);

        /// <summary>
        /// ����ĳ����
        /// </summary>
        /// <typeparam Name="D">��������</typeparam>
        /// <param Name="key">��</param>
        /// <param Name="value">ֵ</param>
        public void SetData<D>(string key, D value);

        /// <summary>
        /// �Ƴ�ĳ����
        /// </summary>
        /// <param Name="key"></param>
        public void RemoveData(string key);

        /// <summary>
        /// ��ʼ����״̬��
        /// </summary>
        /// <typeparam Name="S">��ʼ״̬����</typeparam>
        public void Start<S>() where S : FSMState<T>;

        /// <summary>
        /// ��ʼ����״̬��
        /// </summary>
        /// <param Name="stateType">��ʼ״̬����</param>
        public void Start(Type stateType);

    }
}

