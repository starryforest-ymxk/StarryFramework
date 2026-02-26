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

#if UNITY_EDITOR
        public static void SetInstance(FrameworkSettings settings)
        {
            _instance = settings;
        }
        
        public static void ClearCache()
        {
            _instance = null;
        }
#endif

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

        public static List<ModuleType> CreateDefaultModules()
        {
            return new List<ModuleType>
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
            foreach (var issue in FrameworkSettingsValidator.Validate(this))
            {
                if (issue.Severity == FrameworkSettingsValidationSeverity.Error)
                {
                    Debug.LogError(issue.Message);
                }
                else
                {
                    Debug.LogWarning(issue.Message);
                }
            }
        }

#if UNITY_EDITOR
        internal static string DefaultSettingsFolderPath => "Assets/StarryFramework/Resources";
        internal static string DefaultSettingsAssetPath => $"{DefaultSettingsFolderPath}/{SETTINGS_PATH}.asset";

        internal static void EnsureDefaultSettingsFolder()
        {
            if (!AssetDatabase.IsValidFolder(DefaultSettingsFolderPath))
            {
                const string parentFolder = "Assets/StarryFramework";
                if (!AssetDatabase.IsValidFolder(parentFolder))
                {
                    AssetDatabase.CreateFolder("Assets", "StarryFramework");
                }
                AssetDatabase.CreateFolder(parentFolder, "Resources");
            }
        }

        internal static FrameworkSettings LoadDefaultSettingsAsset()
        {
            return AssetDatabase.LoadAssetAtPath<FrameworkSettings>(DefaultSettingsAssetPath);
        }

        internal static FrameworkSettings CreateSettingsAssetWithDefaults(string assetPath)
        {
            EnsureDefaultSettingsFolder();
            FrameworkSettings settings = CreateInstance<FrameworkSettings>();
            settings.modules = CreateDefaultModules();

            AssetDatabase.CreateAsset(settings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return settings;
        }

        private static FrameworkSettings CreateDefaultSettings()
        {
            string assetPath = DefaultSettingsAssetPath;
            FrameworkSettings settings = CreateSettingsAssetWithDefaults(assetPath);
            
            Debug.Log($"Created default FrameworkSettings at: {assetPath}");
            return settings;
        }

        [MenuItem("Tools/StarryFramework/Create Settings Asset", priority = 2)]
        private static void CreateSettingsAsset()
        {
            string assetPath = DefaultSettingsAssetPath;
            FrameworkSettings existing = LoadDefaultSettingsAsset();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Warning", $"FrameworkSettings already exists at: {assetPath}", "OK");
                Selection.activeObject = existing;
                return;
            }
            
            FrameworkSettings settings = CreateSettingsAssetWithDefaults(assetPath);
            
            Selection.activeObject = settings;
            EditorUtility.DisplayDialog("Success", $"Created FrameworkSettings at: {assetPath}", "OK");
        }

        [MenuItem("Tools/StarryFramework/Select Settings Asset", priority = 1)]
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
