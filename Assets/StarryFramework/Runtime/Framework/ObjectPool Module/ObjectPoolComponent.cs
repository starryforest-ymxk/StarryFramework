using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public class ObjectPoolComponent : BaseComponent
    {
        private List<ObjectPoolProperty> objectPools = new();
        public List<ObjectPoolProperty> ObjectPools => objectPools;

        private ObjectPoolManager _manager;
        private ObjectPoolManager Manager => _manager ??= FrameworkManager.GetManager<ObjectPoolManager>();

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<ObjectPoolManager>();
            objectPools = Manager.getObjectPoolProperties();
        }

        internal override void Shutdown()
        {
            objectPools.Clear();
        }



        #region public

        /// <summary>
        /// ע��ĳ��Object����Ķ����,��GameObject����
        /// </summary>
        /// <typeparam Name="T">����󶨵ļ̳���ObjectBase����</typeparam>
        /// <param Name="AutoReleaseInterval">�Զ��ͷŵ�ʱ����</param>
        /// <param Name="ExpireTime">�������ʱ��</param>
        /// <param Name="key">�����ǣ����ڸ�ͬһ������ע���������</param>
        public void Register<T>(float autoReleaseInterval, float expireTime, string key = "") where T : ObjectBase, new()
        {
           Manager.Register<T>(autoReleaseInterval, expireTime, key);
        }

        /// <summary>
        /// ע��ĳ��GameObject����Ķ����
        /// </summary>
        /// <typeparam Name="T">����󶨵ļ̳���GameObjectBase����</typeparam>
        /// <param Name="targetObject">����ع����Ŀ������</param>
        /// <param Name="AutoReleaseInterval">�Զ��ͷŵ�ʱ����</param>
        /// <param Name="ExpireTime">�������ʱ��</param>
        /// <param Name="fatherObject">�������ɺ�ĸ�����</param>
        /// <param Name="key">�����ǣ����ڸ�ͬһ������ע���������</param>
        public void Register<T>(GameObject targetObject, float autoReleaseInterval, float expireTime, GameObject fatherObject = null, string key = "") where T : GameObjectBase
        {
            Manager.Register<T>(targetObject, autoReleaseInterval, expireTime, fatherObject, key);
        }

        /// <summary>
        /// ע��ĳ��GameObject����Ķ����
        /// </summary>
        /// <typeparam Name="T">����󶨵ļ̳���GameObjectBase����</typeparam>
        /// <param Name="path">����ع����Ŀ�������·��</param>
        /// <param Name="AutoReleaseInterval">�Զ��ͷŵ�ʱ����</param>
        /// <param Name="ExpireTime">�������ʱ��</param>
        /// <param Name="fatherObject">�������ɺ�ĸ�����</param>
        /// <param Name="key">�����ǣ����ڸ�ͬһ������ע���������</param>
        public void Register<T>(string path, float autoReleaseInterval, float expireTime, GameObject fatherObject = null, string key = "") where T : GameObjectBase
        {
            Manager.Register<T>(path, autoReleaseInterval, expireTime, fatherObject, key);
        }

        /// <summary>
        /// ��ȡĳ������
        /// </summary>
        /// <typeparam Name="T">�����������</typeparam>
        /// <param Name="key">������</param>
        /// <returns>Ҫ��ȡ������</returns>
        public T Require<T>(string key = "") where T : class, IObjectBase
        {
            return Manager.Require<T>(key);
        }

        /// <summary>
        /// ����ĳ������
        /// </summary>
        /// <typeparam Name="T">�����������</typeparam>
        /// <param Name="obj">Ҫ���յ�����</param>
        /// <param Name="key">������</param>
        public void Recycle<T>(T obj, string key = "") where T : class, IObjectBase
        {
            Manager.Recycle(obj, key);
        }

        /// <summary>
        /// ������������ĳ������أ�ʹ�䲻���Զ��ͷ�����
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="Locked">trueδ������falseΪ�������</param>
        /// <param Name="key"></param>
        public void SetLocked<T>(bool locked, string key = "") where T : class, IObjectBase
        {
            Manager.SetLocked<T>(locked, key);
        }

        /// <summary>
        /// �ͷ�ĳ������
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="obj"></param>
        /// <param Name="key"></param>
        public void ReleaseObject<T>(T obj, string key = "") where T : class, IObjectBase
        {
            Manager.ReleaseObject(obj, key);
        }

        /// <summary>
        /// �ͷ�����δʹ�õ�����
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="key"></param>
        public void ReleaseAllUnused<T>(string key = "") where T : class, IObjectBase
        {
            Manager.ReleaseAllUnused<T>(key);
        }

        /// <summary>
        /// �ͷ���������
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="key"></param>
        public void ReleaseAllObjects<T>(string key = "") where T : class, IObjectBase
        {
            Manager.ReleaseAllObjects<T>(key);
        }

        /// <summary>
        /// �ͷŶ����
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="key"></param>
        public void ReleasePool<T>(string key = "") where T : class, IObjectBase
        {
            Manager.ReleasePool<T>(key);
        }

        #endregion
    }
}

