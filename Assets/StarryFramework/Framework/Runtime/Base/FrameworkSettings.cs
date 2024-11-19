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
        
        [Header("Framework Internal Event")] [Tooltip("����ڲ��¼�������ʱ���Ƿ��ͬʱ�����ⲿͬ���¼���When an internal framework event is triggered, whether an external event with the same name will also be triggered simultaneously")] [Space(5)] 
        [SerializeField] internal bool InternalEventTrigger = true;
        
        [Header("Initial Scene Load")] [Tooltip("��Ϸ�������صĳ�ʼ�����������GameFramework�򲻼��ء�If the initial scene to load is the GameFramework, then it does nothing")] [Space(5)] 
        [SerializeField] [SceneIndex] internal int StartScene = 0;

        [Tooltip("��ʼ���������Ƿ�����Ĭ�϶�����Whether the initial scene loading enable the default animation")]
        [SerializeField] internal bool StartSceneAnimation = false;
        
        [Header("Modules Enabled")] [Space(5)]
        [Tooltip("��Ϸ��ܸ�ģ���Ƿ������Լ����ȼ���Խ�����б�ǰ�����ȼ�Խ�ߡ�Whether each module of the game framework is enabled and its priority, with higher priority given to those closer to the top of the list.")]
        [SerializeField] internal List<ModuleType> modules= new List<ModuleType>();

        internal bool ModuleInUse(ModuleType type)
        {
            return modules.Contains(type);
        }

        /// <summary>
        /// ��ʼ��
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
        /// ���ú����Լ��
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
