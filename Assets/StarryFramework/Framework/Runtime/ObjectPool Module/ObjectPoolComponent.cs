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
        /// 注册某个Object物体的对象池,非GameObject对象
        /// </summary>
        /// <typeparam Name="T">物体绑定的继承自ObjectBase的类</typeparam>
        /// <param Name="AutoReleaseInterval">自动释放的时间间隔</param>
        /// <param Name="ExpireTime">物体过期时间</param>
        /// <param Name="key">物体标记，用于给同一个物体注册多个对象池</param>
        public void Register<T>(float autoReleaseInterval, float expireTime, string key = "") where T : ObjectBase, new()
        {
           Manager.Register<T>(autoReleaseInterval, expireTime, key);
        }

        /// <summary>
        /// 注册某个GameObject物体的对象池
        /// </summary>
        /// <typeparam Name="T">物体绑定的继承自GameObjectBase的类</typeparam>
        /// <param Name="targetObject">对象池管理的目标物体</param>
        /// <param Name="AutoReleaseInterval">自动释放的时间间隔</param>
        /// <param Name="ExpireTime">物体过期时间</param>
        /// <param Name="fatherObject">物体生成后的父物体</param>
        /// <param Name="key">物体标记，用于给同一个物体注册多个对象池</param>
        public void Register<T>(GameObject targetObject, float autoReleaseInterval, float expireTime, GameObject fatherObject = null, string key = "") where T : GameObjectBase
        {
            Manager.Register<T>(targetObject, autoReleaseInterval, expireTime, fatherObject, key);
        }

        /// <summary>
        /// 注册某个GameObject物体的对象池
        /// </summary>
        /// <typeparam Name="T">物体绑定的继承自GameObjectBase的类</typeparam>
        /// <param Name="path">对象池管理的目标物体的路径</param>
        /// <param Name="AutoReleaseInterval">自动释放的时间间隔</param>
        /// <param Name="ExpireTime">物体过期时间</param>
        /// <param Name="fatherObject">物体生成后的父物体</param>
        /// <param Name="key">物体标记，用于给同一个物体注册多个对象池</param>
        public void Register<T>(string path, float autoReleaseInterval, float expireTime, GameObject fatherObject = null, string key = "") where T : GameObjectBase
        {
            Manager.Register<T>(path, autoReleaseInterval, expireTime, fatherObject, key);
        }

        /// <summary>
        /// 获取某个物体
        /// </summary>
        /// <typeparam Name="T">对象池物体类</typeparam>
        /// <param Name="key">物体标记</param>
        /// <returns>要获取的物体</returns>
        public T Require<T>(string key = "") where T : class, IObjectBase
        {
            return Manager.Require<T>(key);
        }

        /// <summary>
        /// 回收某个物体
        /// </summary>
        /// <typeparam Name="T">对象池物体类</typeparam>
        /// <param Name="obj">要回收的物体</param>
        /// <param Name="key">物体标记</param>
        public void Recycle<T>(T obj, string key = "") where T : class, IObjectBase
        {
            Manager.Recycle(obj, key);
        }

        /// <summary>
        /// 锁定或解除锁定某个对象池，使其不会自动释放物体
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="Locked">true未锁定，false为解除锁定</param>
        /// <param Name="key"></param>
        public void SetLocked<T>(bool locked, string key = "") where T : class, IObjectBase
        {
            Manager.SetLocked<T>(locked, key);
        }

        /// <summary>
        /// 释放某个物体
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="obj"></param>
        /// <param Name="key"></param>
        public void ReleaseObject<T>(T obj, string key = "") where T : class, IObjectBase
        {
            Manager.ReleaseObject(obj, key);
        }

        /// <summary>
        /// 释放所有未使用的物体
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="key"></param>
        public void ReleaseAllUnused<T>(string key = "") where T : class, IObjectBase
        {
            Manager.ReleaseAllUnused<T>(key);
        }

        /// <summary>
        /// 释放所有物体
        /// </summary>
        /// <typeparam Name="T"></typeparam>
        /// <param Name="key"></param>
        public void ReleaseAllObjects<T>(string key = "") where T : class, IObjectBase
        {
            Manager.ReleaseAllObjects<T>(key);
        }

        /// <summary>
        /// 释放对象池
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

