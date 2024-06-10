using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static Codice.CM.WorkspaceServer.WorkspaceTreeDataStore;

namespace StarryFramework.Editor
{


    [CustomEditor(typeof(EventComponent))]
    public class EventComponentInspector : FrameworkInspector
    {
        private bool foldoutGroupLastEvent = true;
        private bool foldoutGroupEventsInfo = true;
        private Dictionary<string, bool> foldoutDic = new Dictionary<string, bool>();

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            EventComponent e = (EventComponent)target;

            DrawLastEventInfo(e);

            EditorGUILayout.Space(3);

            DrawAllEventsInfo(e);

            Repaint();
        }

        private void DrawLastEventInfo(EventComponent e)
        {
            foldoutGroupLastEvent = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutGroupLastEvent, "上次触发的事件信息");

            if (foldoutGroupLastEvent)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField("Last Invoked Event", e.LastEventName);
                EditorGUILayout.LabelField("Last Invoked Event Params", e.LastEventParam);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAllEventsInfo(EventComponent e)
        {
            foldoutGroupEventsInfo = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutGroupEventsInfo, "所有已注册的事件信息");

            if (foldoutGroupEventsInfo)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    Dictionary<string, Dictionary<string, int>> infoDic = e.GetAllEventsInfo();

                    if (infoDic.Count > 0)
                        foreach (var pair in infoDic)
                        {
                            int eventCount = 0;
                            foreach (var ei in pair.Value)
                            {
                                eventCount += ei.Value;
                            }
                            if (!foldoutDic.ContainsKey(pair.Key))
                            {
                                foldoutDic.Add(pair.Key, false);
                            }


                            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
                            EditorGUILayout.BeginHorizontal();
                            {
                                foldoutDic[pair.Key] = EditorGUI.Foldout(r, foldoutDic[pair.Key], GUIContent.none);
                                EditorGUI.LabelField(r, pair.Key, eventCount.ToString());
                            }
                            EditorGUILayout.EndHorizontal();

                            if (foldoutDic[pair.Key])
                            {
                                EditorGUILayout.BeginVertical(StyleFramework.box);

                                foreach (var ei in pair.Value)
                                {
                                    EditorGUILayout.LabelField(ei.Key, "     " + ei.Value.ToString());
                                }

                                EditorGUILayout.EndVertical();
                            }
                        }
                    else
                        EditorGUILayout.LabelField("None");

                }
                EditorGUILayout.EndVertical();
            }
        }
    }
}