using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace StarryFramework.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private static SettingsWindow window;
        
        private SerializedObject _settingsSerializedObject;
        private Vector2 _scrollPosition;
        private FrameworkSettings _frameworkSettings;
        private Texture2D _logoTexture;
        
        private const string LOGO_PATH = "Assets/Plugins/StarryFramework/Info/images/StarryFramework-Logo.png";
        private const string QuickStart_URL = "https://github.com/starryforest-ymxk/StarryFramework/blob/master/Assets/StarryFramework/Info/README.md#%E5%BF%AB%E9%80%9F%E5%BC%80%E5%A7%8B";
        private const string APIReference_URL = "https://github.com/starryforest-ymxk/StarryFramework/blob/master/Assets/StarryFramework/Info/API%E9%80%9F%E6%9F%A5%E6%89%8B%E5%86%8C.md";
    
        [MenuItem("Tools/StarryFramework/Settings Panel", priority = 0)] 
        private static void ShowSettingWindow()
        {
            ShowWindow();
        }
        
        public static void ShowWindow()
        {
            window = EditorWindow.GetWindow<SettingsWindow>("StarryFramework");
            window.Show();
        }
        
        private void OnEnable()
        {
            LoadFrameworkSettings();
            LoadLogo();
        }
        
        private void OnFocus()
        {
            RefreshFrameworkSettings();
        }
        
        private void RefreshFrameworkSettings()
        {
            FrameworkSettings currentInstance = FrameworkSettings.Instance;
            if (_frameworkSettings != currentInstance)
            {
                _frameworkSettings = currentInstance;
                if (_frameworkSettings != null)
                {
                    _settingsSerializedObject = new SerializedObject(_frameworkSettings);
                }
            }
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
            EditorGUILayout.LabelField("Framework Settings", EditorStyles.boldLabel);
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
                
                EditorGUI.BeginChangeCheck();
                FrameworkSettings newSettings = EditorGUILayout.ObjectField("Settings Asset", _frameworkSettings, typeof(FrameworkSettings), false) as FrameworkSettings;
                
                if (EditorGUI.EndChangeCheck() && newSettings != _frameworkSettings)
                {
                    if (newSettings != null)
                    {
                        _frameworkSettings = newSettings;
                        _settingsSerializedObject = new SerializedObject(_frameworkSettings);
                        FrameworkSettings.SetInstance(newSettings);
                        FrameworkManager.Debugger.Log($"Framework Setting has been updated: {AssetDatabase.GetAssetPath(newSettings)}");
                    }
                    else
                    {
                        FrameworkManager.Debugger.LogWarning("Framework Setting can not be null. Keep the current setting.");
                    }
                }
                
                if (GUILayout.Button("Ping", GUILayout.Width(50)))
                {
                    EditorGUIUtility.PingObject(_frameworkSettings);
                    Selection.activeObject = _frameworkSettings;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Global Framework Settings", EditorStyles.miniBoldLabel);
                string assetPath = AssetDatabase.GetAssetPath(_frameworkSettings);
                EditorGUILayout.LabelField("Path:", assetPath, EditorStyles.wordWrappedLabel);
                EditorGUILayout.EndVertical();
                
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
            
            if (GUILayout.Button("Quick Start", linkStyle, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL(QuickStart_URL);
            }
            
            EditorGUIUtility.AddCursorRect(GUILayoutUtility.GetLastRect(), MouseCursor.Link);
            
            GUILayout.Label("   | ", GUILayout.ExpandWidth(false));
            
            if (GUILayout.Button("API Quick Reference", linkStyle, GUILayout.ExpandWidth(false)))
            {
                Application.OpenURL(APIReference_URL);
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
            List<FrameworkSettingsValidationIssue> issues = FrameworkSettingsValidator.Validate(_frameworkSettings);
            if (issues.Count > 0)
            {
                EditorGUILayout.Space(5);
                foreach (FrameworkSettingsValidationIssue issue in issues)
                {
                    MessageType messageType = issue.Severity == FrameworkSettingsValidationSeverity.Error
                        ? MessageType.Error
                        : MessageType.Warning;

                    string codePrefix = string.IsNullOrEmpty(issue.Code) ? string.Empty : $"[{issue.Code}] ";
                    string displayText = $"{codePrefix}{issue.Message}";
                    if (!string.IsNullOrEmpty(issue.SuggestedFix))
                    {
                        displayText += $"\nSuggestion: {issue.SuggestedFix}";
                    }

                    EditorGUILayout.HelpBox(displayText, messageType);
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
            
            EditorGUILayout.PropertyField(enterPlayModeWayProp, new GUIContent("Enter PlayMode Way", "Enter Play Mode behavior."));
            
            EditorGUI.BeginChangeCheck();
            SceneAsset sceneAsset = null;
            if (!string.IsNullOrEmpty(_frameworkSettings.frameworkScenePath))
            {
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(_frameworkSettings.frameworkScenePath);
            }
            SceneAsset newSceneAsset = (SceneAsset)EditorGUILayout.ObjectField(new GUIContent("GameFramework Scene", "Path to the GameFramework scene."), sceneAsset, typeof(SceneAsset), false);
            if (EditorGUI.EndChangeCheck())
            {
                frameworkScenePathProp.stringValue = newSceneAsset != null ? AssetDatabase.GetAssetPath(newSceneAsset) : "";
            }
            
            if (_frameworkSettings.enterPlayModeWay == EnterPlayModeWay.FrameworkStart && string.IsNullOrEmpty(_frameworkSettings.frameworkScenePath))
            {
                EditorGUILayout.HelpBox("FrameworkStart mode is selected, but the GameFramework scene reference is not set. Please configure the scene path.", MessageType.Warning);
                
                if (GUILayout.Button("Auto Find GameFramework Scene"))
                {
                    FindAndSetGameFrameworkScene(frameworkScenePathProp);
                }
            }
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.PropertyField(debugTypeProp, new GUIContent("Debug Type"));
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.PropertyField(internalEventTriggerProp, new GUIContent("Internal Event Trigger", "When an internal framework event is triggered, also trigger an external event with the same name."));
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.PropertyField(startSceneProp, new GUIContent("Start Scene", "Initial scene to load when the game starts. If it is the GameFramework scene, no extra scene is loaded."));
            EditorGUILayout.PropertyField(startSceneAnimationProp, new GUIContent("Start Scene Animation", "Whether to use the default transition animation when loading the initial scene."));
            
            EditorGUILayout.Space(10);
            
            EditorGUILayout.HelpBox("Enable/disable framework modules and define their priority order. Modules closer to the top have higher priority.", MessageType.Info);
            EditorGUILayout.HelpBox(
                "This panel edits framework-level settings only (startup flow, module enable list/order, logging). " +
                "Per-module detailed settings (Scene/Save/Timer/UI/Audio etc.) are configured on the corresponding module components in the GameFramework scene.",
                MessageType.Info);
            
            EditorGUILayout.Space(10);
            
            DrawValidationMessages();
            
            EditorGUILayout.PropertyField(modulesProp, new GUIContent("Modules List"), true);
            
            EditorGUILayout.EndVertical();
        }
        
        private void FindAndSetGameFrameworkScene(SerializedProperty frameworkScenePathProp)
        {
            string[] guids = AssetDatabase.FindAssets("t:Scene GameFramework");
            
            if (guids.Length == 0)
            {
                EditorUtility.DisplayDialog("Scene Not Found", 
                    "No scene named 'GameFramework' was found in the project.", 
                    "OK");
                return;
            }
            
            if (guids.Length == 1)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                frameworkScenePathProp.stringValue = scenePath;
                _settingsSerializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_frameworkSettings);
                AssetDatabase.SaveAssets();
                
                EditorUtility.DisplayDialog("Success", 
                    $"GameFramework scene path has been set to:\n{scenePath}", 
                    "OK");
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
            string assetPath = FrameworkSettings.DefaultSettingsAssetPath;
            FrameworkSettings existing = FrameworkSettings.LoadDefaultSettingsAsset();
            if (existing != null)
            {
                EditorUtility.DisplayDialog("Warning", $"FrameworkSettings already exists at: {assetPath}", "OK");
                _frameworkSettings = existing;
                _settingsSerializedObject = new SerializedObject(_frameworkSettings);
                return;
            }

            FrameworkSettings settings = FrameworkSettings.CreateSettingsAssetWithDefaults(assetPath);

            _frameworkSettings = settings;
            _settingsSerializedObject = new SerializedObject(_frameworkSettings);

            Selection.activeObject = settings;
            EditorUtility.DisplayDialog("Success", $"Created FrameworkSettings at: {assetPath}", "OK");
        }
    }
}
