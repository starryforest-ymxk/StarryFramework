using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StarryFramework.Editor
{
    public class SettingsWindow : EditorWindow
    {
        private static SettingsWindow window;
        
        private EnterPlayModeWay _enterPlayModeWay;
        private SceneAsset _sceneAsset;
    
        [MenuItem("Window/StarryFramework/Settings")] 
        private static void ShowSettingWindow()
        {
            window = EditorWindow.GetWindow<SettingsWindow>("Framework setings");
            window.Show();
        }
        
        // 窗口启用时加载保存的设置
        private void OnEnable()
        {
            _enterPlayModeWay = (EnterPlayModeWay)EditorPrefs.GetInt("EnterPlayModeWay", 0);
            string scenePath = EditorPrefs.GetString("FrameworkScenePath", "");
            if (!string.IsNullOrEmpty(scenePath))
            {
                _sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            }
        }
    
        private void OnGUI()
        {
            GUI.changed = false;
            GUILayout.Label("Framework Settings", EditorStyles.boldLabel);

            _enterPlayModeWay = (EnterPlayModeWay)EditorGUILayout.EnumPopup("Enter PlayMode Way", _enterPlayModeWay);
            _sceneAsset = (SceneAsset)EditorGUILayout.ObjectField("GameFramework Scene", _sceneAsset, typeof(SceneAsset), false);
            
            if (GUI.changed)
            {
                SaveSettings();
            }

        }
        
        private void SaveSettings()
        {
            EditorPrefs.SetInt("EnterPlayModeWay", (int)_enterPlayModeWay);
            EditorPrefs.SetString("FrameworkScenePath", _sceneAsset != null ? AssetDatabase.GetAssetPath(_sceneAsset) : "");
            SceneSetupOnPlay.BindCallback();
        }
    }
}


