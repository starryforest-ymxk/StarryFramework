using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    /// <summary>
    /// ResourceComponent的自定义Inspector面板
    /// 运行时显示资源加载状态、进度、路径等信息
    /// </summary>
    [CustomEditor(typeof(ResourceComponent))]
    public class ResourceComponentInspector : FrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("仅在运行时可用", MessageType.Info);
                return;
            }

            ResourceComponent r = (ResourceComponent)target;

            EditorGUILayout.LabelField("目标资源类型", r.TargetType == null ? "Null" : r.TargetType.ToString());

            EditorGUILayout.LabelField("目标资源路径", r.ResourcePath);

            EditorGUILayout.LabelField("异步加载资源状态", r.State.ToString());

            EditorGUILayout.LabelField("异步加载资源进度", r.Progress.ToString("F2"));

            var rect = GUILayoutUtility.GetRect(18, 18, "TextField");

            EditorGUI.ProgressBar(rect, r.Progress, $"{r.Progress * 100:F2}%");

            Repaint();
        }
    }
}

