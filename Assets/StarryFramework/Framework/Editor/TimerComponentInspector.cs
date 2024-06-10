using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace StarryFramework.Editor
{
    [CustomEditor(typeof(TimerComponent))]
    public class TimerComponentInspector : FrameworkInspector
    {

        private bool foldoutTimers = true;
        private bool foldoutTriggerTimers = true;
        private bool foldoutAsyncTimers = true;
        private bool foldoutClear = true;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            TimerComponent t = (TimerComponent)target;
            
            serializedObject.Update();
            
            SerializedProperty mySerializableProperty = serializedObject.FindProperty("settings");
            EditorGUILayout.PropertyField(mySerializableProperty, true);
            
            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
                DrawAutoClear(t);
                EditorGUILayout.Space(5);
                DrawTimers(t);
                EditorGUILayout.Space(5);
                DrawTriggerTimers(t);
                EditorGUILayout.Space(5);
                DrawAsyncTimers(t);
            }
            
            Repaint();
        }

        public void DrawTimers(TimerComponent t)
        {
            foldoutTimers = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTimers, "计时器信息");

            if (foldoutTimers)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", t.timers.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var timer in t.timers)
                    {
                        string name = timer.Name == "" ? "Unamed" : timer.Name;
                        string timerState = timer.TimerState.ToString();
                        Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);

                        EditorGUILayout.BeginHorizontal();
                        {
                            timer.Foldout = EditorGUI.Foldout(r, timer.Foldout, GUIContent.none);
                            EditorGUI.LabelField(r, name, timerState);
                        }
                        EditorGUILayout.EndHorizontal();

                        if (timer.Foldout)
                        {
                            EditorGUILayout.BeginVertical(StyleFramework.box);
                            {
                                EditorGUILayout.LabelField("start value", timer.StartValue.ToString("F2"));
                                EditorGUILayout.LabelField("current value", timer.Time.ToString("F2"));
                                EditorGUILayout.LabelField("ignore time scale", timer.IgnoreTimeScale.ToString());
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void DrawTriggerTimers(TimerComponent t)
        {
            foldoutTriggerTimers = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTriggerTimers, "触发计时器信息");

            if (foldoutTriggerTimers)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", t.triggerTimers.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var timer in t.triggerTimers)
                    {
                        string name = timer.Name == "" ? "Unamed" : timer.Name;
                        string timerState = timer.TimerState.ToString();
                        Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);

                        EditorGUILayout.BeginHorizontal();
                        {
                            timer.Foldout = EditorGUI.Foldout(r, timer.Foldout, GUIContent.none);
                            EditorGUI.LabelField(r, name, timerState);
                        }
                        EditorGUILayout.EndHorizontal();

                        if (timer.Foldout)
                        {
                            EditorGUILayout.BeginVertical(StyleFramework.box);
                            {
                                EditorGUILayout.LabelField("repeat", timer.Repeat.ToString());
                                EditorGUILayout.LabelField("trigger interval", timer.TimeDelta.ToString("F2"));
                                EditorGUILayout.LabelField("start time", timer.StartTime.ToString("F2"));
                                EditorGUILayout.LabelField("ignore time scale", timer.IgnoreTimeScale.ToString());
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void DrawAsyncTimers(TimerComponent t)
        {
            foldoutAsyncTimers = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAsyncTimers, "异步计时器信息");

            if (foldoutAsyncTimers)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", t.asyncTimers.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var timer in t.asyncTimers)
                    {
                        string name = timer.Name == "" ? "Unamed" : timer.Name;
                        string timerState = timer.TimerState.ToString();
                        EditorGUILayout.LabelField(name, timerState);
                    }

                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void DrawAutoClear(TimerComponent t)
        {
            foldoutClear = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutClear, "计时器自动回收");
            if(foldoutClear)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Unused trigger timer clean interval", t.ClearUnusedTriggerTimersInterval.ToString("F0"));
                    EditorGUILayout.LabelField("Unused async timer clean interval", t.ClearUnusedAsyncTimersInterval.ToString("F0"));
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }


}
