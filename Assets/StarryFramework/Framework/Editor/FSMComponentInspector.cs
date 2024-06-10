using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{

    [CustomEditor(typeof(FSMComponent))]
    internal sealed class FSMComponentInspector : FrameworkInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            FSMComponent t = (FSMComponent)target;

            EditorGUILayout.LabelField("FSM Count", t.GetFSMCount().ToString());

            FSMBase[] fsms = t.GetAllFSMs();
            foreach (FSMBase fsm in fsms)
            {
                DrawFSM(fsm);
            }
            Repaint();
        }

        private void DrawFSM(FSMBase fsm)
        {
            EditorGUILayout.Space(6);

            string content = fsm.IsRunning() ? $"{fsm.GetCurrentStateName()}, {fsm.GetCurrentStateTime():F2} s" : (fsm.IsDestroyed() ? "Destroyed" : "Not Running");
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
            EditorGUILayout.BeginHorizontal();
            {
                fsm.Foldout = EditorGUI.Foldout(r, fsm.Foldout, GUIContent.none);
                EditorGUI.LabelField(r, fsm.FullName, content);
            }
            EditorGUILayout.EndHorizontal();

            if (fsm.Foldout)
            {
                IVariable[] variables = fsm.GetAllData();
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    foreach (var a in variables)
                    {
                        EditorGUILayout.LabelField(a.Key, a.GetValueString());
                    }
                }
                EditorGUILayout.EndVertical();
            }

        }
    }
}


