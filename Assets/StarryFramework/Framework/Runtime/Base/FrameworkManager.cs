using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    internal static class FrameworkManager
    {
        private static readonly Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

        //��¼module�����Լ����ȼ�
        private static readonly List<Type> managerTypeList = new List<Type>();


        private static FrameworkState state = FrameworkState.Stop;
        internal static FrameworkState FrameworkState => state;

        private static FrameworkSettings frameworkSetting;
        internal static FrameworkSettings Setting => frameworkSetting;

        //GameFramework �ڲ��¼�������
        private static FrameworkEventManager _eventManager;
        internal static FrameworkEventManager EventManager
        {
            get { return _eventManager ??= new FrameworkEventManager(); }
        }

        //GameFramework �ڲ�Debugger
        private static FrameworkDebugger _debugger;
        internal static FrameworkDebugger Debugger
        {
            get { return _debugger ??= new FrameworkDebugger(); }
        }

        #region Settingע��

        internal static void RegisterSetting(FrameworkSettings setting)
        {
            frameworkSetting = setting;
        }

        #endregion

        // BeforeShutDown() �� ShutDown() ��FrameworkComponent.ShutDown()����(���ô���)
        // ����������MainComponent��MonoBehaviour����

        #region �������
        internal static void BeforeAwake()
        {
            state = FrameworkState.Awake;
        }

        internal static void Awake()
        {
            foreach (ModuleType type in Setting.modules)
            {
                var managerType = GetManagerType(type);
                if (managerType == null)
                {
                    Debug.LogError($"{type} Module does not exist in the framework.");
                }
                else managerTypeList.Add(managerType);
            }
        }

        internal static void Init()
        {
            state = FrameworkState.Init;
            foreach (Type type in managerTypeList)
            {
                managers[type].Init();
            }
        }

        internal static void AfterInit()
        {
            state = FrameworkState.Runtime;
        }

        internal static void Update()
        {
            foreach (Type type in managerTypeList)
            {
                managers[type].Update();
            }
        }

        internal static void BeforeShutDown()
        {
            state = FrameworkState.ShutDown;
            Debugger.SetDebugActive(false);
        }

        internal static void ShutDown()
        {

            managerTypeList.Reverse();

            foreach (Type type in managerTypeList)
            {
                managers[type].ShutDown();
            }

            managerTypeList.Clear();

            managers.Clear();

            EventManager.ShutDown();

            state = FrameworkState.Stop;
        }

        #endregion


        #region Manager����

        #region �ڲ�����
        private static bool HasManager(Type type)
        {
            if (managers.ContainsKey(type))
                return true;
            else
                return false;
        }

        private static void AddManager(IManager manager)
        {
            Type type = manager.GetType();

            if (HasManager(type))
            {
                Debug.LogError($"Manager {type} has already existed.");
            }
            else
            {
                managers.Add(type, manager);
            }

        }

        private static void RemoveManager(Type type)
        {

            if (!HasManager(type))
            {
                Debug.LogError($"Manager {type} does not exist.");
            }
            else
            {
                managers.Remove(type);
            }
        }

        private static Type GetManagerType(ModuleType managerType)
        {
            return Type.GetType("StarryFramework." + managerType.ToString() + "Manager");
        }

        #endregion

        #region �ⲿ�ӿ�

        internal static T GetManager<T>() where T : IManager, new()
        {
            Type type = typeof(T);

            if (!HasManager(type))
            {
                T t = new T();
                t.Awake();
                AddManager(t);
            }

            return (T)managers[type];
        }
        internal static void DeleteManager<T>() where T : IManager, new()
        {
            RemoveManager(typeof(T));
        }

        #endregion

        #endregion


    }

    
}
