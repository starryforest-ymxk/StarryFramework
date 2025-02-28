using System.Collections;
using System.Collections.Generic;
using StarryFramework.Editor;
using UnityEditor;

namespace StarryFramework.Extentions.Editor
{
    [CustomEditor(typeof(AudioComponent))]
    public class AudioComponentInspector : FrameworkInspector
    {
        private bool foldout = true;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            AudioComponent a = (AudioComponent)target;
            
            serializedObject.Update();
            
            SerializedProperty mySerializableProperty = serializedObject.FindProperty("settings");
            EditorGUILayout.PropertyField(mySerializableProperty, true);
            
            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
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
            }
            
            Repaint();
        }
    }
}
