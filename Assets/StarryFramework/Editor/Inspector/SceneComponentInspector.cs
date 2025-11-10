using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    /// <summary>
    /// SceneComponent的自定义Inspector面板
    /// 运行时显示当前场景信息和场景运行时间
    /// </summary>
    [CustomEditor(typeof(SceneComponent))]
    public class SceneComponentInspector : FrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            SceneComponent s = (SceneComponent)target;
            
            serializedObject.Update();
            
            SerializedProperty mySerializableProperty = serializedObject.FindProperty("settings");
            EditorGUILayout.PropertyField(mySerializableProperty, true);
            
            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Current Active Scene", s.CurrentActiveScene.name);
                EditorGUILayout.LabelField("Scene Loaded Time", s.SceneLoadedTime.ToString("F2"));
                EditorGUILayout.LabelField("Scene Running Time", s.SceneTime.ToString("F2"));
            }

            Repaint();
        }
    }
}

