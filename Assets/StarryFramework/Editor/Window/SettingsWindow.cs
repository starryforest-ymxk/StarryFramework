using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace StarryFramework.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private static SettingsWindow window;
        
        private SerializedObject _settingsSerializedObject;
        private Vector2 _scrollPosition;
        private FrameworkSettings _frameworkSettings;
        private Texture2D _logoTexture;
        
        private const string LOGO_PATH = "Assets/StarryFramework/Info/images/StarryFramework-Logo.png";
        private const string GITHUB_URL = "https://github.com/starryforest-ymxk/StarryFramework";
    
        [MenuItem("Window/StarryFramework/Settings Panel")] 
        private static void ShowSettingWindow()
        {
            window = EditorWindow.GetWindow<SettingsWindow>("StarryFramework");
            window.Show();
        }
        
        private void OnEnable()
        {
            LoadFrameworkSettings();
            LoadLogo();
        }
        
        private void LoadLogo()
        {
            _logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(LOGO_PATH);
            if (_logoTexture == null)
            {
                Debug.LogWarning($"StarryFramework Logo not found at: {LOGO_PATH}");
            }
        }
        
        private void LoadFrameworkSettings()
        {
            _frameworkSettings = FrameworkSettings.Instance;
            if (_frameworkSettings != null)
            {
                _settingsSerializedObject = new SerializedObject(_frameworkSettings);
            }
        }
    
        private void OnGUI()
        {
            DrawHeader();
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("框架设置/Framework Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            if (_frameworkSettings == null)
            {
                EditorGUILayout.HelpBox("FrameworkSettings asset not found. Please create one.", MessageType.Error);
                
                if (GUILayout.Button("Create FrameworkSettings Asset"))
                {
                    CreateFrameworkSettings();
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("Settings Asset", _frameworkSettings, typeof(FrameworkSettings), false);
                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    EditorGUIUtility.PingObject(_frameworkSettings);
                    Selection.activeObject = _frameworkSettings;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                if (_settingsSerializedObject != null)
                {
                    _settingsSerializedObject.Update();
                    
                    DrawFrameworkSettings();
                    
                    if (_settingsSerializedObject.ApplyModifiedProperties())
                    {
                        EditorUtility.SetDirty(_frameworkSettings);
                        AssetDatabase.SaveAssets();
                        SceneSetupOnPlay.BindCallback();
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical("box");
            
            if (_logoTexture != null)
            {
                float logoWidth = _logoTexture.width;
                float logoHeight = _logoTexture.height;
                float maxWidth = position.width - 40;
                
                if (logoWidth > maxWidth)
                {
                    float scale = maxWidth / logoWidth;
                    logoWidth = maxWidth;
                    logoHeight *= scale;
                }
                
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label(_logoTexture, GUILayout.Width(logoWidth), GUILayout.Height(logoHeight));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                
                EditorGUILayout.Space(5);
            }
            
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUIStyle linkStyle = new GUIStyle(EditorStyles.label);
            linkStyle.normal.textColor = new Color(0.4f, 0.6f, 1f);
            linkStyle.hover.textColor = new Color(0.6f, 0.8f, 1f);
            linkStyle.active.textColor = new Color(0.3f, 0.5f, 0.9f);
            linkStyle.fontSize = 12;
            
            if (GUILayout.Button("📖 快速开始 / Quick Start", linkStyle, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL(GITHUB_URL);
            }
            
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            
            GUILayout.Label("   | ", GUILayout.ExpandWidth(false));
            
            if (GUILayout.Button("📚 帮助文档 / Documentation", linkStyle, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL(GITHUB_URL);
            }
            
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawValidationMessages()
        {
            List<ModuleType> modules = _frameworkSettings.modules;
            List<string> errors = new List<string>();
            
            HashSet<ModuleType> uniqueCheck = new HashSet<ModuleType>();
            foreach (ModuleType module in modules)
            {
                if (!uniqueCheck.Add(module))
                {
                    errors.Add($"模块列表中存在重复的组件: {module}。Duplicate module in the list: {module}.");
                    break;
                }
            }
            
            if (_frameworkSettings.InternalEventTrigger && !modules.Contains(ModuleType.Event))
            {
                errors.Add("勾选了Internal Event Trigger但是没有启用Event组件。Internal Event Trigger is enabled but Event module is not in the list.");
            }
            
            if (_frameworkSettings.StartScene != 0 && !modules.Contains(ModuleType.Scene))
            {
                errors.Add("设置了初始加载场景但是没有启用Scene组件。Start Scene is set but Scene module is not in the list.");
            }
            
            if (errors.Count > 0)
            {
                EditorGUILayout.Space(5);
                foreach (string error in errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
                EditorGUILayout.Space(5);
            }
        }
        
        private void DrawFrameworkSettings()
        {
            EditorGUILayout.BeginVertical("box");
            
            SerializedProperty enterPlayModeWayProp = _settingsSerializedObject.FindProperty("enterPlayModeWay");
            SerializedProperty frameworkScenePathProp = _settingsSerializedObject.FindProperty("frameworkScenePath");
            SerializedProperty debugTypeProp = _settingsSerializedObject.FindProperty("debugType");
            SerializedProperty internalEventTriggerProp = _settingsSerializedObject.FindProperty("InternalEventTrigger");
            SerializedProperty startSceneProp = _settingsSerializedObject.FindProperty("StartScene");
            SerializedProperty startSceneAnimationProp = _settingsSerializedObject.FindProperty("StartSceneAnimation");
            SerializedProperty modulesProp = _settingsSerializedObject.FindProperty("modules");
            
            EditorGUILayout.PropertyField(enterPlayModeWayProp, new GUIContent("Enter PlayMode Way", "进入Play模式的方式。Enter Play Mode behavior."));
            
            EditorGUI.BeginChangeCheck();
            SceneAsset sceneAsset = null;
            if (!string.IsNullOrEmpty(_frameworkSettings.frameworkScenePath))
            {
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_frameworkSettings.frameworkScenePath);
            }
            SceneAsset newSceneAsset = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("GameFramework Scene", "GameFramework场景路径。Path to GameFramework scene."), sceneAsset, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                frameworkScenePathProp.stringValue = newSceneAsset != null ? AssetDatabase.GetAssetPath(newSceneAsset) : "";
            }
            
            if (_frameworkSettings.enterPlayModeWay == EnterPlayModeWay.FrameworkStart && string.IsNullOrEmpty(_frameworkSettings.frameworkScenePath))
            {
                EditorGUILayout.HelpBox("选择了FrameworkStart模式，但未配置GameFramework场景引用。请配置场景路径。FrameworkStart mode is selected, but GameFramework scene reference is not set. Please configure the scene path.", MessageType.Warning);
                
                if (GUILayout.Button("自动查找GameFramework场景 / Auto Find GameFramework Scene"))
                {
                    FindAndSetGameFrameworkScene(frameworkScenePathProp);
                }
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.PropertyField(debugTypeProp, new GUIContent("Debug Type"));
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.PropertyField(internalEventTriggerProp, new GUIContent("Internal Event Trigger", "框架内部事件被触发时，是否会同时触发外部同名事件。When an internal framework event is triggered, whether an external event with the same name will also be triggered simultaneously."));
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.PropertyField(startSceneProp, new GUIContent("Start Scene", "游戏启动加载的初始场景，如果是GameFramework则不加载。If the initial scene to load is the GameFramework, then it does nothing."));
            EditorGUILayout.PropertyField(startSceneAnimationProp, new GUIContent("Start Scene Animation", "初始场景加载是否启用默认动画。Whether the initial scene loading enable the default animation."));
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox("游戏框架各模块是否启用以及优先级，越靠近列表前端优先级越高。Whether each module of the game framework is enabled and its priority, with higher priority given to those closer to the top of the list.", MessageType.Info);
            DrawValidationMessages();
            
            EditorGUILayout.PropertyField(modulesProp, new GUIContent("Modules List"), true);
            
            EditorGUILayout.EndVertical();
        }
        
        private void FindAndSetGameFrameworkScene(SerializedProperty frameworkScenePathProp)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene GameFramework");
            
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("未找到场景 / Scene Not Found", 
                    "未在项目中找到名为'GameFramework'的场景。\nGameFramework scene not found in the project.", 
                    "确定 / OK");
                return;
            }
            
            if (guids.Length == 1)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                frameworkScenePathProp.stringValue = scenePath;
                _settingsSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_frameworkSettings);
                AssetDatabase.SaveAssets();
                
                EditorUtility.DisplayDialog("设置成功 / Success", 
                    $"已自动设置GameFramework场景路径：\n{scenePath}\n\nGameFramework scene path has been set to:\n{scenePath}", 
                    "确定 / OK");
            }
            else
            {
                GenericMenu menu = new GenericMenu();
                foreach (string guid in guids)
                {
                    string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                    menu.AddItem(new GUIContent(scenePath), false, () =>
                    {
                        frameworkScenePathProp.stringValue = scenePath;
                        _settingsSerializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(_frameworkSettings);
                        AssetDatabase.SaveAssets();
                    });
                }
                menu.ShowAsContext();
            }
        }
        
        private void CreateFrameworkSettings()
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
            
            string assetPath = $"{folderPath}/FrameworkSettings.asset";
            
            if (AssetDatabase.LoadAssetAtPath<FrameworkSettings>(assetPath) != null)
            {
                EditorUtility.DisplayDialog("Warning", $"FrameworkSettings already exists at: {assetPath}", "OK");
                _frameworkSettings = AssetDatabase.LoadAssetAtPath<FrameworkSettings>(assetPath);
                _settingsSerializedObject = new SerializedObject(_frameworkSettings);
                return;
            }
            
            FrameworkSettings settings = ScriptableObject.CreateInstance<FrameworkSettings>();
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
            
            _frameworkSettings = settings;
            _settingsSerializedObject = new SerializedObject(_frameworkSettings);
            
            Selection.activeObject = settings;
            EditorUtility.DisplayDialog("Success", $"Created FrameworkSettings at: {assetPath}", "OK");
        }
    }
}