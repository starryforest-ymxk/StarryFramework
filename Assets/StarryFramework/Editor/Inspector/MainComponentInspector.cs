using UnityEngine;
using UnityEditor;

namespace StarryFramework.Editor
{
    [CustomEditor(typeof(MainComponent))]
    public class MainComponentInspector : UnityEditor.Editor
    {
        private SerializedProperty _frameRateProperty;
        private SerializedProperty _gameSpeedProperty;
        private SerializedProperty _runInBackgroundProperty;
        private SerializedProperty _neverSleepProperty;

        private void OnEnable()
        {
            _frameRateProperty = serializedObject.FindProperty("frameRate");
            _gameSpeedProperty = serializedObject.FindProperty("gameSpeed");
            _runInBackgroundProperty = serializedObject.FindProperty("runInBackground");
            _neverSleepProperty = serializedObject.FindProperty("neverSleep");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Unity Setting", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
            
            EditorGUILayout.PropertyField(_frameRateProperty);
            EditorGUILayout.PropertyField(_gameSpeedProperty);
            EditorGUILayout.PropertyField(_runInBackgroundProperty);
            EditorGUILayout.PropertyField(_neverSleepProperty);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Framework Setting", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            FrameworkSettings currentSettings = FrameworkSettings.Instance;

            if (currentSettings != null)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("全局框架设置 / Global Framework Settings", EditorStyles.miniBoldLabel);
                
                EditorGUI.BeginChangeCheck();
                FrameworkSettings newSettings = EditorGUILayout.ObjectField("Settings Asset", currentSettings, typeof(FrameworkSettings), false) as FrameworkSettings;
                
                if (EditorGUI.EndChangeCheck() && newSettings != null && newSettings != currentSettings)
                {
                    FrameworkSettings.SetInstance(newSettings);
                    Debug.Log($"<color=cyan>[Framework Settings]</color> 全局设置已更新为: {AssetDatabase.GetAssetPath(newSettings)}");
                    GUIUtility.ExitGUI();
                }
                
                string assetPath = AssetDatabase.GetAssetPath(currentSettings);
                EditorGUILayout.LabelField("路径 / Path:", assetPath, EditorStyles.wordWrappedLabel);
                
                EditorGUILayout.Space(5);
                EditorGUILayout.HelpBox(
                    "FrameworkSettings controls framework-level startup flow, logging and module enable/order. " +
                    "Per-module detailed settings are configured on each module component in the GameFramework scene.",
                    MessageType.Info);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("在 Settings Panel 中编辑 / Edit in Settings Panel"))
                {
                    SettingsWindow.ShowWindow();
                }
                
                if (GUILayout.Button("Ping"))
                {
                    EditorGUIUtility.PingObject(currentSettings);
                    Selection.activeObject = currentSettings;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("未找到 FrameworkSettings！请创建一个。\nFrameworkSettings not found! Please create one.", MessageType.Warning);
                if (GUILayout.Button("创建 FrameworkSettings / Create FrameworkSettings"))
                {
                    FrameworkSettings.ClearCache();
                    var newSettings = FrameworkSettings.Instance;
                    EditorUtility.SetDirty(target);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
