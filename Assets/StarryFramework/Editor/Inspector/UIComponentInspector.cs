using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    /// <summary>
    /// Custom inspector for UIComponent.
    /// Displays runtime UI form cache and UI group information.
    /// </summary>
    [CustomEditor(typeof(UIComponent))]
    public class UIComponentInspector : FrameworkInspector
    {
        private bool foldoutUIFormsCache;
        private bool foldoutUIGroups;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UIComponent ui = (UIComponent)target;
            
            serializedObject.Update();
            
            SerializedProperty mySerializableProperty = serializedObject.FindProperty("settings");
            EditorGUILayout.PropertyField(mySerializableProperty, true);
            
            serializedObject.ApplyModifiedProperties();
            
            if (EditorApplication.isPlaying)
            {
                DrawUIFormsCache(ui);
                DrawAllUIGroups(ui);
            }
            
            Repaint();
        }
        
        private void DrawUIFormsCache(UIComponent ui)
        {
            foldoutUIFormsCache = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutUIFormsCache, "UI Form Cache");

            if (foldoutUIFormsCache)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", ui.UIFormsCacheList.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var uiForm in ui.UIFormsCacheList)
                    {
                        DrawUIForm(uiForm, true);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAllUIGroups(UIComponent ui)
        {
            foldoutUIGroups = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutUIGroups, "UI Groups");

            if (foldoutUIGroups)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", ui.UIGroupsDic.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var uiGroup in ui.UIGroupsDic.Values)
                    {
                        DrawUIGroup(uiGroup);
                    }
                }
                EditorGUILayout.EndVertical();
            }
            
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawUIGroup(UIGroup uiGroup)
        {
            string groupName = uiGroup.Name;
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
            EditorGUILayout.BeginHorizontal();
            {
                uiGroup.Foldout = EditorGUI.Foldout(r, uiGroup.Foldout, GUIContent.none);
                EditorGUI.LabelField(r, groupName);
            }
            EditorGUILayout.EndHorizontal();
            if (uiGroup.Foldout)
            {
                //EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("Form Count", uiGroup.FormCount.ToString());
                    EditorGUILayout.LabelField("Pause", uiGroup.Pause.ToString());
                    if (uiGroup.CurrentForm != null)
                    {
                        EditorGUILayout.LabelField("Current Form");
                        EditorGUILayout.BeginVertical(StyleFramework.box);
                        DrawUIForm(uiGroup.CurrentForm);
                        EditorGUILayout.EndVertical();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Current Form", "null");
                    }
                    EditorGUILayout.LabelField("All Forms");
                    EditorGUILayout.BeginVertical(StyleFramework.box);
                    foreach (var uiFormInfo in uiGroup.FormInfosList)
                    {
                        DrawUIFormInfo(uiFormInfo);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
                //EditorGUI.EndDisabledGroup();
            }
        }
        
        private void DrawUIForm(UIForm uiForm, bool showInCache = false)
        {
            string assetName = uiForm.UIFormAssetName;
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);

            EditorGUILayout.BeginHorizontal();
            {
                if (showInCache)
                {
                    uiForm.FoldoutInCache = EditorGUI.Foldout(r, uiForm.FoldoutInCache, GUIContent.none);
                }
                else
                {
                    uiForm.Foldout = EditorGUI.Foldout(r, uiForm.Foldout, GUIContent.none);
                }
                EditorGUI.LabelField(r, assetName);
            }
            EditorGUILayout.EndHorizontal();

            if (showInCache?uiForm.FoldoutInCache:uiForm.Foldout)
            {
                //EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("SerialID", uiForm.SerialID.ToString());
                    EditorGUILayout.LabelField("UI Group", uiForm.UIGroup.Name);
                    EditorGUILayout.LabelField("Depth in group", uiForm.DepthInUIGroup.ToString());
                    EditorGUILayout.LabelField("Pause Covered", uiForm.PauseCoveredUIForm.ToString());
                    
                    MonoBehaviour currentFormLogic = uiForm.UIFormLogic as MonoBehaviour;
                    if (currentFormLogic != null)
                    {
                        EditorGUILayout.ObjectField("Form Logic", currentFormLogic, typeof(MonoBehaviour), true);
                    }
                    
                    //EditorGUILayout.ObjectField("UI GameObject", uiForm.UIObject, typeof(GameObject), true);
                    //EditorGUILayout.ObjectField("Object Handle", uiForm.ObjectHandle, typeof(GameObject), false);
                    
                    EditorGUILayout.LabelField("Release Tag", uiForm.ReleaseTag.ToString());
                }
                EditorGUILayout.EndVertical();
                //EditorGUI.EndDisabledGroup();
            }
        }
        
        private void DrawUIFormInfo(UIFormInfo uiFormInfo)
        {
            UIForm uiForm = uiFormInfo.UIForm;
            string assetName = uiForm.UIFormAssetName;
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);

            EditorGUILayout.BeginHorizontal();
            {
                uiFormInfo.Foldout = EditorGUI.Foldout(r, uiFormInfo.Foldout, GUIContent.none);
                EditorGUI.LabelField(r, assetName);
            }
            EditorGUILayout.EndHorizontal();

            if (uiFormInfo.Foldout)
            {
                //EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("SerialID", uiForm.SerialID.ToString());
                    EditorGUILayout.LabelField("Depth in group", uiForm.DepthInUIGroup.ToString());
                    EditorGUILayout.LabelField("Paused", uiFormInfo.Paused.ToString());
                    EditorGUILayout.LabelField("Covered", uiFormInfo.Covered.ToString());
                    EditorGUILayout.LabelField("Pause Covered", uiForm.PauseCoveredUIForm.ToString());
                    
                    MonoBehaviour currentFormLogic = uiForm.UIFormLogic as MonoBehaviour;
                    if (currentFormLogic != null)
                    {
                        EditorGUILayout.ObjectField("Form Logic", currentFormLogic, typeof(MonoBehaviour), true);
                    }
                    
                    //EditorGUILayout.ObjectField("UI GameObject", uiForm.UIObject, typeof(GameObject), true);
                    //EditorGUILayout.ObjectField("Object Handle", uiForm.ObjectHandle, typeof(GameObject), false);
                    
                    EditorGUILayout.LabelField("Release Tag", uiForm.ReleaseTag.ToString());
                }
                EditorGUILayout.EndVertical();
                //EditorGUI.EndDisabledGroup();
            }
        }


    }
}
