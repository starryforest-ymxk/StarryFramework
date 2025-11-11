using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace StarryFramework
{
    [CreateAssetMenu(fileName = "FrameworkSettings", menuName = "StarryFramework/Framework Settings", order = 0)]
    public class FrameworkSettings : ScriptableObject
    {
        private const string SETTINGS_PATH = "FrameworkSettings";
        private static FrameworkSettings _instance;

        public static FrameworkSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<FrameworkSettings>(SETTINGS_PATH);
                    
#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        Debug.LogWarning($"FrameworkSettings not found in Resources folder. Creating default settings at 'Assets/StarryFramework/Resources/{SETTINGS_PATH}.asset'");
                        _instance = CreateDefaultSettings();
                    }
#else
                    if (_instance == null)
                    {
                        Debug.LogError($"FrameworkSettings not found! Please ensure there is a FrameworkSettings asset in 'Resources/{SETTINGS_PATH}'");
                    }
#endif
                }
                return _instance;
            }
        }

        [HideInInspector]
        public int FrameworkSceneID = -1;

        [Header("编辑器设置/Editor Settings")]
        [Space(5)]
        [Tooltip("进入Play模式的方式。Enter Play Mode behavior.")]
        public EnterPlayModeWay enterPlayModeWay = EnterPlayModeWay.NormalStart;

        [Tooltip("GameFramework场景路径。Path to GameFramework scene.")]
        public string frameworkScenePath = "";

        [Header("日志等级/Log Level")] 
        [Space(5)] 
        public FrameworkDebugType debugType = FrameworkDebugType.Normal;
        
        [Header("框架内部事件/Framework Internal Event")] 
        [Tooltip("框架内部事件被触发时，是否会同时触发外部同名事件。When an internal framework event is triggered, whether an external event with the same name will also be triggered simultaneously.")] 
        [Space(5)] 
        public bool InternalEventTrigger = true;
        
        [Header("初始场景加载/Initial Scene Load")] 
        [Tooltip("游戏启动加载的初始场景，如果是GameFramework则不加载。If the initial scene to load is the GameFramework, then it does nothing.")] 
        [Space(5)] 
        [SceneIndex] 
        public int StartScene = 0;

        [Tooltip("初始场景加载是否启用默认动画。Whether the initial scene loading enable the default animation.")]
        public bool StartSceneAnimation = false;
        
        [Header("启用的模块/Modules Enabled")] 
        [Space(5)]
        [Tooltip("游戏框架各模块是否启用以及优先级，越靠近列表前端优先级越高。Whether each module of the game framework is enabled and its priority, with higher priority given to those closer to the top of the list.")]
        public List<ModuleType> modules = new List<ModuleType>();

        public bool ModuleInUse(ModuleType type)
        {
            return modules.Contains(type);
        }

        public void Init()
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

        public void SettingCheck()
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

#if UNITY_EDITOR
        private static FrameworkSettings CreateDefaultSettings()
        {
            FrameworkSettings settings = CreateInstance<FrameworkSettings>();
            
            settings.modules = new List<ModuleType>
            {
                ModuleType.Scene,
                ModuleType.Event,
                ModuleType.Timer,
                ModuleType.Resource,
                ModuleType.ObjectPool,
                ModuleType.FSM,
                ModuleType.Save,
                ModuleType.UI
            };
            
            string folderPath = "Assets/StarryFramework/Resources";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/StarryFramework";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "StarryFramework");
                }
                AssetDatabase.CreateFolder(parentFolder, "Resources");
            }
            
            string assetPath = $"{folderPath}/{SETTINGS_PATH}.asset";
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"Created default FrameworkSettings at: {assetPath}");
            return settings;
        }

        [MenuItem("Window/StarryFramework/Create Settings Asset")]
        private static void CreateSettingsAsset()
        {
            string folderPath = "Assets/StarryFramework/Resources";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string parentFolder = "Assets/StarryFramework";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "StarryFramework");
                }
                AssetDatabase.CreateFolder(parentFolder, "Resources");
            }
            
            string assetPath = $"{folderPath}/{SETTINGS_PATH}.asset";
            
            if (AssetDatabase.LoadAssetAtPath<FrameworkSettings>(assetPath) != null)
            {
                EditorUtility.DisplayDialog("Warning", $"FrameworkSettings already exists at: {assetPath}", "OK");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<FrameworkSettings>(assetPath);
                return;
            }
            
            FrameworkSettings settings = CreateInstance<FrameworkSettings>();
            settings.modules = new List<ModuleType>
            {
                ModuleType.Scene,
                ModuleType.Event,
                ModuleType.Timer,
                ModuleType.Resource,
                ModuleType.ObjectPool,
                ModuleType.FSM,
                ModuleType.Save,
                ModuleType.UI
            };
            
            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Selection.activeObject = settings;
            EditorUtility.DisplayDialog("Success", $"Created FrameworkSettings at: {assetPath}", "OK");
        }

        [MenuItem("Window/StarryFramework/Select Settings Asset")]
        private static void SelectSettingsAsset()
        {
            FrameworkSettings settings = Instance;
            if (settings != null)
            {
                Selection.activeObject = settings;
                EditorGUIUtility.PingObject(settings);
            }
        }
#endif
    }
}
