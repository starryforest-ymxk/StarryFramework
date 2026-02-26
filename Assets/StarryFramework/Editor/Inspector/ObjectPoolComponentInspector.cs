using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    /// <summary>
    /// Custom inspector for ObjectPoolComponent.
    /// Displays runtime object pool information and configuration.
    /// </summary>
    [CustomEditor(typeof(ObjectPoolComponent))]
    public class ObjectPoolComponentInspector : FrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available only in Play Mode.", MessageType.Info);
                return;
            }

            ObjectPoolComponent o = (ObjectPoolComponent)target;

            EditorGUILayout.LabelField("ObjectPool List", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("ObjectPool Count", o.ObjectPools.Count.ToString());

            foreach (var pool in o.ObjectPools)
            {
                pool.foldout = EditorGUILayout.Foldout(pool.foldout, pool.FullName);
                if (pool.foldout)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Object Count", pool.Count.ToString());
                    EditorGUILayout.LabelField("Auto Release Interval", pool.AutoReleaseInterval.ToString("F2"));
                    EditorGUILayout.LabelField("Last Release Time", pool.LastReleaseTime.ToString("F2"));
                    EditorGUILayout.LabelField("Object Expire Time", pool.ExpireTime.ToString("F2"));
                    EditorGUILayout.LabelField("Locked", pool.Locked.ToString());
                    EditorGUILayout.EndVertical();
                }
            }

            Repaint();
        }
    }
}
