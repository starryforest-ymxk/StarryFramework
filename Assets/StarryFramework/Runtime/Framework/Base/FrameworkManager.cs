using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    internal static class FrameworkManager
    {
        private static readonly Dictionary<Type, IManager> managers = new();
        private static readonly Dictionary<ModuleType, Type> moduleManagerTypeMap = new();
        private static readonly Dictionary<Type, ModuleType> componentModuleTypeMap = new();
        private static readonly ConcurrentQueue<Action> mainThreadActions = new();

        //记录module启用以及优先级
        private static readonly List<Type> managerTypeList = new();
        
        private static FrameworkState state = FrameworkState.Stop;
        internal static FrameworkState FrameworkState => state;

        private static FrameworkSettings frameworkSetting;
        internal static FrameworkSettings Setting => frameworkSetting;

        //GameFramework 内部事件管理器
        private static FrameworkEventManager _eventManager;
        internal static FrameworkEventManager EventManager
        {
            get { return _eventManager ??= new FrameworkEventManager(); }
        }

        //GameFramework 内部Debugger
        private static FrameworkDebugger _debugger;
        internal static FrameworkDebugger Debugger
        {
            get { return _debugger ??= new FrameworkDebugger(); }
        }

        static FrameworkManager()
        {
            RegisterModuleManagerType(ModuleType.Scene, typeof(SceneManager));
            RegisterModuleManagerType(ModuleType.Timer, typeof(TimerManager));
            RegisterModuleManagerType(ModuleType.Event, typeof(EventManager));
            RegisterModuleManagerType(ModuleType.Save, typeof(SaveManager));
            RegisterModuleManagerType(ModuleType.Resource, typeof(ResourceManager));
            RegisterModuleManagerType(ModuleType.ObjectPool, typeof(ObjectPoolManager));
            RegisterModuleManagerType(ModuleType.FSM, typeof(FSMManager));
            RegisterModuleManagerType(ModuleType.UI, typeof(UIManager));

            RegisterModuleComponentType(ModuleType.Scene, typeof(SceneComponent));
            RegisterModuleComponentType(ModuleType.Timer, typeof(TimerComponent));
            RegisterModuleComponentType(ModuleType.Event, typeof(EventComponent));
            RegisterModuleComponentType(ModuleType.Save, typeof(SaveComponent));
            RegisterModuleComponentType(ModuleType.Resource, typeof(ResourceComponent));
            RegisterModuleComponentType(ModuleType.ObjectPool, typeof(ObjectPoolComponent));
            RegisterModuleComponentType(ModuleType.FSM, typeof(FSMComponent));
            RegisterModuleComponentType(ModuleType.UI, typeof(UIComponent));
        }

        #region Setting注册

        internal static void RegisterSetting(FrameworkSettings setting)
        {
            frameworkSetting = setting;
        }

        #endregion

        #region Module Mapping / MainThread Dispatch

        internal static void RegisterModuleManagerType(ModuleType moduleType, Type managerType)
        {
            if (managerType == null)
            {
                Debug.LogError($"Trying to register null manager type for module [{moduleType}]");
                return;
            }

            moduleManagerTypeMap[moduleType] = managerType;
        }

        internal static void RegisterModuleComponentType(ModuleType moduleType, Type componentType)
        {
            if (componentType == null)
            {
                Debug.LogError($"Trying to register null component type for module [{moduleType}]");
                return;
            }

            if (!typeof(BaseComponent).IsAssignableFrom(componentType))
            {
                Debug.LogError($"Type [{componentType}] is not a BaseComponent and cannot be registered as module component.");
                return;
            }

            componentModuleTypeMap[componentType] = moduleType;
        }

        internal static bool TryGetModuleType(BaseComponent component, out ModuleType moduleType)
        {
            moduleType = default;
            if (component == null)
            {
                return false;
            }

            Type currentType = component.GetType();
            while (currentType != null && typeof(BaseComponent).IsAssignableFrom(currentType))
            {
                if (componentModuleTypeMap.TryGetValue(currentType, out moduleType))
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            // Legacy fallback for custom modules not yet registered explicitly.
            return Enum.TryParse(component.gameObject.name, out moduleType);
        }

        internal static void PostToMainThread(Action action)
        {
            if (action == null)
            {
                return;
            }

            mainThreadActions.Enqueue(action);
        }

        private static void ProcessMainThreadActions()
        {
            while (mainThreadActions.TryDequeue(out var action))
            {
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private static void ClearMainThreadActions()
        {
            while (mainThreadActions.TryDequeue(out _)) { }
        }

        #endregion

        // BeforeShutDown() 和 ShutDown() 由FrameworkComponent.ShutDown()触发(调用触发)
        // 其余流程由MainComponent的MonoBehaviour驱动

        #region 组件流程
        internal static void BeforeAwake()
        {
            ClearMainThreadActions();
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
            ProcessMainThreadActions();
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

            ClearMainThreadActions();

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
            if (moduleManagerTypeMap.TryGetValue(managerType, out var mappedType))
            {
                return mappedType;
            }

            // Legacy fallback for unregistered/custom modules.
            return Type.GetType("StarryFramework." + managerType + "Manager");
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
