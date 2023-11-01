using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.StarryUnityEditor
{
    [CustomEditor(typeof(AudioComponent))]
    public class AudioComponentInspector : StarryFrameworkInspector
    {
        private bool foldout = true;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            AudioComponent a = (AudioComponent)target;

            EditorGUILayout.LabelField("current bgm", a.CurrentBGM);

            EditorGUILayout.LabelField("current bgm state", a.BGMState.ToString());

            foldout = EditorGUILayout.Foldout(foldout, "current bgm list");

            if (foldout)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    if (a.CurrentBGMList != null && a.CurrentBGMList.Count > 0)
                        foreach (var m in a.CurrentBGMList)
                        {
                            EditorGUILayout.LabelField(""/*m.ToString()*/);
                        }
                    else
                        EditorGUILayout.LabelField("None");
                }
                EditorGUILayout.EndVertical();

            }

            Repaint();
        }
    }
}
