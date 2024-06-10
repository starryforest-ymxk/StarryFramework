using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace StarryFramework.Editor
{
    [CustomEditor(typeof(ObjectPoolComponent))]
    public class ObjectPoolComponentInspector : FrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
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
                    EditorGUILayout.LabelField("Loocked", pool.Locked.ToString());
                    EditorGUILayout.EndVertical();
                }
            }

            Repaint();
        }
    }
}
