using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    internal class FrameworkSettings
    {

        internal int FrameworkSceneID = -1;

        [Header("游戏框架启用的Module")]
        [Tooltip("游戏框架各Module是否启用以及优先级，越靠近List前端优先级越高")]
        [SerializeField]
        internal List<ModuleType> modules= new List<ModuleType>();

        [SerializeField]
        internal SceneSettings SceneSettings = new SceneSettings();

        [SerializeField]
        internal EventSettings EventSettings = new EventSettings();

        [SerializeField]
        internal TimerSettings TimerSettings = new TimerSettings();

        [SerializeField]
        internal SaveSettings SaveSettings = new SaveSettings();

        [SerializeField]
        internal AudioSettings AudioSettings = new AudioSettings();

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
                    Debug.LogError("Same components are not allowed in the Mpdule List");
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
