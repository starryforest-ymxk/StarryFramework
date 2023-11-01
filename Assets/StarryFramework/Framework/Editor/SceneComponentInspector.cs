using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace StarryFramework.StarryUnityEditor
{
    [CustomEditor(typeof(SceneComponent))]
    public class SceneComponentInspector : StarryFrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            SceneComponent s = (SceneComponent)target;

            EditorGUILayout.LabelField("Current Active Scene", s.CurrentActiveScene.name);

            EditorGUILayout.LabelField("Scene Loaded Time", s.SceneLoadedTime.ToString("F2"));

            EditorGUILayout.LabelField("Scene Running Time", s.SceneTime.ToString("F2"));

            Repaint();
        }
    }
}

