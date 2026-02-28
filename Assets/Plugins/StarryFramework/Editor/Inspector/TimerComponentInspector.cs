using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    /// <summary>
    /// Custom inspector for TimerComponent.
    /// Displays runtime timer state and diagnostic information.
    /// </summary>
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
            foldoutTimers = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTimers, "Timers");

            if (foldoutTimers)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", t.Timers.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var timer in t.Timers)
                    {
                        string name = timer.Name == "" ? "Unnamed" : timer.Name;
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
            foldoutTriggerTimers = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutTriggerTimers, "Trigger Timers");

            if (foldoutTriggerTimers)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", t.TriggerTimers.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var timer in t.TriggerTimers)
                    {
                        string name = timer.Name == "" ? "Unnamed" : timer.Name;
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
            foldoutAsyncTimers = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutAsyncTimers, "Async Timers");

            if (foldoutAsyncTimers)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", t.AsyncTimers.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var timer in t.AsyncTimers)
                    {
                        string name = timer.Name == "" ? "Unnamed" : timer.Name;
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
            foldoutClear = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutClear, "Auto Cleanup");
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
