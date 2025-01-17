using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace StarryFramework.Editor
{
    [CustomEditor(typeof(ResourceComponent))]
    public class ResourceComponentInspector : FrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            ResourceComponent r = (ResourceComponent)target;

            EditorGUILayout.LabelField("Ŀ����Դ����", r.TargetType == null? "Null":r.TargetType.ToString());

            EditorGUILayout.LabelField("Ŀ����Դ·��", r.ResourcePath);

            EditorGUILayout.LabelField("�첽������Դ״̬", r.State.ToString());

            EditorGUILayout.LabelField("�첽������Դ����", r.Progress.ToString("F2"));

            var rect = GUILayoutUtility.GetRect(18,18,"TextField");

            EditorGUI.ProgressBar(rect, r.Progress, $"{r.Progress*100 :F2}%");

            Repaint();
        }
    }
}
