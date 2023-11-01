using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    internal static class FrameworkManager
    {
        private static Dictionary<Type, IManager> managers = new Dictionary<Type, IManager>();

        //记录module启用以及优先级
        private static List<Type> managerTypeList = new List<Type>();

        private static FrameworkSettings frameworkSetting;

        private static FrameworkState state = FrameworkState.Stop;

        internal static FrameworkState frameworkState => state;

        internal static FrameworkSettings setting => frameworkSetting;

        //GameFramework 内部事件管理器
        private static FrameworkEventManager _eventManager;
        internal static FrameworkEventManager eventManager
        {
            get
            {
                if (_eventManager == null)
                {
                    _eventManager = new FrameworkEventManager();
                }
                return _eventManager;
            }
        }


        #region Setting注册

        internal static void RegisterSetting(FrameworkSettings setting)
        {
            frameworkSetting = setting;
        }

        #endregion

        // BeforeShutDown() 和 ShutDown() 由FrameworkComponent.ShutDown()触发(调用触发)
        // 其余流程由MainComponent的MonoBehaviour驱动

        #region 组件流程
        internal static void BeforeAwake()
        {
            state = FrameworkState.Awake;
        }

        internal static void Awake()
        {

            foreach (ModuleType type in setting.modules)
            {
                managerTypeList.Add(GetManagerType(type));
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

            eventManager.ShutDown();

            state = FrameworkState.Stop;
        }

        #endregion


        #region Manager管理

        #region 内部方法
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

        #region 外部接口

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
