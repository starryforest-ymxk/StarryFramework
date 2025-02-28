using UnityEngine;
using UnityEngine.Events;
using System;

namespace StarryFramework
{
    public class ResourceComponent : BaseComponent
    {
        private ResourceManager _manager;
        private ResourceManager Manager => _manager ??= FrameworkManager.GetManager<ResourceManager>();
        
        private Type _targetType;
        private string _resourcePath = "";
        private LoadState _state = LoadState.Idle;
        private float _progress;
        
        public LoadState State => _state;
        public float Progress => _progress;
        public string ResourcePath => _resourcePath;
        public Type TargetType => _targetType;

        private ResourceRequest latestRequest;

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<ResourceManager>();
        }

        private void Update()
        {
            if(_state == LoadState.Loading)
            {
                _progress = latestRequest.progress;
            }
        }


        /// <summary>
        /// ͬ������һ����Դ
        /// </summary>
        /// <typeparam path="T">��Դ������</typeparam>
        /// <param path="path">��Դ��Resources�ļ���������·����</param>
        /// <param path="GameObjectInstantiate">�����Դ��GameObject�Ƿ�ֱ������</param>
        /// <returns>�����Ӧ��ԴΪGameobjet,�����ɲ��������壻������ǣ���ֱ�ӷ�������</returns>
        public T LoadRes<T>(string path, bool GameObjectInstantiate = false) where T : UnityEngine.Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            T t =  Manager.LoadRes<T>(path, GameObjectInstantiate);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
            return t;
        }

        /// <summary>
        /// ͬ������·����������Դ
        /// </summary>
        /// <typeparam path="T">��Դ������</typeparam>
        /// <param path="path">·��</param>
        /// <returns>���ص���Դ����</returns>
        public T[] LoadAllRes<T>(string path) where T : UnityEngine.Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            T[] t = Manager.LoadAllRes<T>(path);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset);
            return t;
        }

        /// <summary>
        /// �첽������Դ
        /// </summary>
        /// <typeparam path="T">��Դ����</typeparam>
        /// <param path="path">��Դ��Resources�ļ����µ�·����ʡ����չ��</param>
        /// <param path="callBack">��Դ������Ļص����Լ��ص���Դ����Ϊ����</param>
        /// <param path="GameObjectInstantiate">�����Դ��GameObject�Ƿ�ֱ������</param> 
        /// <returns>��Դ��������</returns>
        public ResourceRequest LoadAsync<T>(string path, UnityAction<T> callBack, bool GameObjectInstantiate = false) where T : UnityEngine.Object
        {
            _targetType = typeof(T);
            _resourcePath = path;
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadAsset);
            _state = LoadState.Loading;
            callBack += (a) => {
                _state = LoadState.Idle; 
                _progress = 1f; 
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadAsset); 
            };
            ResourceRequest r = Manager.LoadAsync<T>(path, callBack, GameObjectInstantiate);
            latestRequest = r;
            return r;
        }

        /// <summary>
        /// ֻ��ж�ط�GameObject����, GameObject������Destroy����
        /// </summary>
        /// <param path="_object"></param>
        public void Unload(UnityEngine.Object _object)
        {
            Manager.Unload(_object);
        }

        /// <summary>
        /// �ͷ�����û��ʹ�õ���Դ
        /// </summary>
        public void UnloadUnused()
        {
            Manager.UnloadUnused();
        }



    }
}

