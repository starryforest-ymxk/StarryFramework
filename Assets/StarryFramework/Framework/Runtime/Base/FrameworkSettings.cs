using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarryFramework
{
    [Serializable]
    internal class FrameworkSettings
    {

        internal int FrameworkSceneID = -1;

        [Header("Log Level")] [Space(5)] 
        [SerializeField] internal FrameworkDebugType debugType = FrameworkDebugType.Normal;
        
        [Header("Framework Internal Event")] [Tooltip("框架内部事件被触发时，是否会同时触发外部同名事件。When an internal framework event is triggered, whether an external event with the same name will also be triggered simultaneously")] [Space(5)] 
        [SerializeField] internal bool InternalEventTrigger = true;
        
        [Header("Initial Scene Load")] [Tooltip("游戏启动加载的初始场景，如果是GameFramework则不加载。If the initial scene to load is the GameFramework, then it does nothing")] [Space(5)] 
        [SerializeField] [SceneIndex] internal int StartScene = 0;

        [Tooltip("初始场景加载是否启用默认动画。Whether the initial scene loading enable the default animation")]
        [SerializeField] internal bool StartSceneAnimation = false;
        
        [Header("Modules Enabled")] [Space(5)]
        [Tooltip("游戏框架各模块是否启用以及优先级，越靠近列表前端优先级越高。Whether each module of the game framework is enabled and its priority, with higher priority given to those closer to the top of the list.")]
        [SerializeField] internal List<ModuleType> modules= new List<ModuleType>();

        internal bool ModuleInUse(ModuleType type)
        {
            return modules.Contains(type);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        internal void Init()
        {
#if UNITY_EDITOR

            var scenes = EditorBuildSettings.scenes;

            for (int i = 0; i < scenes.Length; i++)
            {

                string sceneName = Utilities.ScenePathToName(scenes[i].path);

                if (sceneName == "GameFramework")
                {
                    FrameworkSceneID = i;
                    break;
                }
            }
            if (FrameworkSceneID == -1)
            {
                Debug.LogError("Check Your Build Settings: You need to add the scene \"GameFramework\".");
            }
#else

            FrameworkSceneID = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
#endif
        }

        /// <summary>
        /// 设置合理性检查
        /// </summary>
        internal void SettingCheck()
        {
            List<ModuleType> check = new List<ModuleType>();
            foreach (ModuleType type in modules)
            {
                if (check.Contains(type))
                {
                    Debug.LogError("Same components are not allowed in the Module List");
                }
                else
                {
                    check.Add(type);
                }
            }
            check.Clear();
        }

    }
}
