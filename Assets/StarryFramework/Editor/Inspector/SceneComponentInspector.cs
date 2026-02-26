using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    /// <summary>
    /// Custom inspector for SceneComponent.
    /// Displays current scene information and runtime timing.
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
            EditorGUILayout.HelpBox(
                "SceneSettings is consumed locally by SceneComponent (default scene transition animation timing). " +
                "It is scene-level configuration and is not injected into SceneManager.",
                MessageType.Info);
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

