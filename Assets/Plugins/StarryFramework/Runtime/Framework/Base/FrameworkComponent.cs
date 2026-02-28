using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarryFramework
{
    public static class FrameworkComponent
    {
        private static Dictionary<Type,BaseComponent> components = new Dictionary<Type, BaseComponent>();


        private static bool HasComponent(Type type)
        {
            if (components.ContainsKey(type))
                return true;
            else
                return false;
        }

        private static void AddComponent(BaseComponent baseComponent)
        {
            Type type = baseComponent.GetType();

            if (HasComponent(type))
            {
                Debug.LogError($"Component {type} has already existed.");
            }
            else
            {
                components.Add(type,baseComponent);
            }

        }

        private static void RemoveComponent(BaseComponent baseComponent)
        {
            Type type = baseComponent.GetType();

            if (!HasComponent(type))
            {
                Debug.LogError($"Component {type} does not exist.");
            }
            else
            {
                components.Remove(type);
            }
        }

        internal static void RegisterComponent(BaseComponent baseComponent)
        {
            if(baseComponent is null)
            {
                Debug.LogError("Component is null");
            }

            AddComponent(baseComponent);
        }

        internal static void DeleteComponent(BaseComponent baseComponent)
        {
            if (baseComponent is null)
            {
                Debug.LogError("Component is null");
            }

            RemoveComponent(baseComponent);
        }

        internal static T GetComponent<T>() where T: BaseComponent
        {
            return (T)GetComponent(typeof(T));
        }

        internal static BaseComponent GetComponent(string typeName)
        {
            Type type = Type.GetType("StarryFramework." + typeName);

            return GetComponent(type);
        }

        internal static BaseComponent GetComponent(ModuleType moduleType)
        {
            Type type = Type.GetType("StarryFramework." + moduleType.ToString() + "Component");

            return GetComponent(type);
        }

        internal static BaseComponent GetComponent(Type type)
        {
            if (type is null)
            {
                FrameworkManager.Debugger.LogError("Invalid type");
                return null;
            }
            else if (!HasComponent(type))
            {
                if(type != Type.GetType("StarryFramework.EventComponent"))
                    FrameworkManager.Debugger.LogError($"Component {type} has not existed.");
                return null;
            }
            else
            {
                return components[type];
            }
        }

        internal static void Shutdown(ShutdownType shutdownType)
        {

            FrameworkManager.Debugger.Log($"Shutdown StarryFramework [{shutdownType}]...");

            FrameworkManager.BeforeShutDown();

            foreach(var component in components)
            {
                component.Value.Shutdown();
            }

            components.Clear();

            FrameworkManager.ShutDown();

            if (shutdownType == ShutdownType.None)
            {
                return;
            }
            else if (shutdownType == ShutdownType.Restart)
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(FrameworkManager.Setting.FrameworkSceneID,LoadSceneMode.Single);
                return;
            }
            else if (shutdownType == ShutdownType.Quit)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
                return;



            }
        }



    }
}

