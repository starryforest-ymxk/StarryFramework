using System;
using System.Linq;
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
        private bool foldoutActiveForms;
        private bool foldoutOpeningRequests;
        private string topmostQueryAssetName;
        private string topmostQueryResult = "N/A";
        
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
                DrawOpeningRequests(ui);
                DrawActiveForms(ui);
                DrawTopmostQuery(ui);
                DrawUIFormsCache(ui);
                DrawAllUIGroups(ui);
            }
            
            Repaint();
        }

        private void DrawOpeningRequests(UIComponent ui)
        {
            foldoutOpeningRequests = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpeningRequests, "Opening Requests");

            if (foldoutOpeningRequests)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    string[] requestKeys = ui.GetOpeningRequestKeysSnapshot();
                    EditorGUILayout.LabelField("Count", ui.OpeningRequestCount.ToString());
                    EditorGUILayout.Space(2);

                    foreach (string requestKey in requestKeys)
                    {
                        EditorGUILayout.LabelField(requestKey);
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawActiveForms(UIComponent ui)
        {
            foldoutActiveForms = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutActiveForms, "Active Forms");

            if (foldoutActiveForms)
            {
                UIForm[] activeForms = ui.GetAllActiveUIFormsSnapshot();

                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", activeForms.Length.ToString());
                    EditorGUILayout.LabelField("ActiveFormCount", ui.ActiveFormCount.ToString());
                    EditorGUILayout.LabelField("ActiveAssetKeyCount", ui.ActiveAssetKeyCount.ToString());
                    EditorGUILayout.Space(2);

                    var formsByAsset = activeForms
                        .GroupBy(form => form.UIFormAssetName ?? string.Empty, StringComparer.Ordinal)
                        .OrderBy(group => group.Key, StringComparer.Ordinal);

                    foreach (var assetGroup in formsByAsset)
                    {
                        UIForm[] forms = assetGroup
                            .OrderByDescending(form => form.LastFocusSequence)
                            .ThenByDescending(form => form.SerialID)
                            .ToArray();

                        if (forms.Length == 0)
                        {
                            continue;
                        }

                        UIForm topmost = forms[0];

                        EditorGUILayout.BeginVertical(StyleFramework.box);
                        {
                            EditorGUILayout.LabelField("Asset", assetGroup.Key);
                            EditorGUILayout.LabelField("Instance Count", forms.Length.ToString());
                            EditorGUILayout.LabelField(
                                "Topmost",
                                $"[{topmost.SerialID}] Group={topmost.UIGroup?.Name ?? "null"}, InstanceKey={FormatInstanceKey(topmost.InstanceKey)}");

                            foreach (UIForm form in forms)
                            {
                                DrawUIForm(form);
                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTopmostQuery(UIComponent ui)
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Topmost Query");
                topmostQueryAssetName = EditorGUILayout.TextField("Asset Name", topmostQueryAssetName);

                if (GUILayout.Button("Query Topmost"))
                {
                    UIForm topmost = ui.GetTopUIForm(topmostQueryAssetName);
                    if (topmost == null)
                    {
                        topmostQueryResult = "null";
                    }
                    else
                    {
                        topmostQueryResult =
                            $"[{topmost.SerialID}] Group={topmost.UIGroup?.Name ?? "null"}, InstanceKey={FormatInstanceKey(topmost.InstanceKey)}";
                    }
                }

                EditorGUILayout.LabelField("Result", topmostQueryResult);
            }
            EditorGUILayout.EndVertical();
        }
        
        private void DrawUIFormsCache(UIComponent ui)
        {
            foldoutUIFormsCache = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutUIFormsCache, "UI Form Cache");

            if (foldoutUIFormsCache)
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Count", ui.UIFormsCacheSnapshot.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var uiForm in ui.UIFormsCacheSnapshot)
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
                    EditorGUILayout.LabelField("Count", ui.UIGroups.Count.ToString());
                    EditorGUILayout.Space(2);
                    foreach (var uiGroup in ui.UIGroups.Values)
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
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("SerialID", uiForm.SerialID.ToString());
                    EditorGUILayout.LabelField("UI Group", uiForm.UIGroup != null ? uiForm.UIGroup.Name : "null");
                    EditorGUILayout.LabelField("Depth in group", uiForm.DepthInUIGroup.ToString());
                    EditorGUILayout.LabelField("Pause Covered", uiForm.PauseCoveredUIForm.ToString());
                    EditorGUILayout.LabelField("Instance Key", FormatInstanceKey(uiForm.InstanceKey));
                    EditorGUILayout.LabelField("Last Focus Sequence", uiForm.LastFocusSequence.ToString());
                    EditorGUILayout.LabelField("Is Opened", uiForm.IsOpened.ToString());
                    
                    MonoBehaviour currentFormLogic = uiForm.UIFormLogic as MonoBehaviour;
                    if (currentFormLogic != null)
                    {
                        EditorGUILayout.ObjectField("Form Logic", currentFormLogic, typeof(MonoBehaviour), true);
                    }
                    
                    EditorGUILayout.LabelField("Release Tag", uiForm.ReleaseTag.ToString());
                }
                EditorGUILayout.EndVertical();
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
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("SerialID", uiForm.SerialID.ToString());
                    EditorGUILayout.LabelField("Depth in group", uiForm.DepthInUIGroup.ToString());
                    EditorGUILayout.LabelField("Paused", uiFormInfo.Paused.ToString());
                    EditorGUILayout.LabelField("Covered", uiFormInfo.Covered.ToString());
                    EditorGUILayout.LabelField("Pause Covered", uiForm.PauseCoveredUIForm.ToString());
                    EditorGUILayout.LabelField("Instance Key", FormatInstanceKey(uiForm.InstanceKey));
                    EditorGUILayout.LabelField("Last Focus Sequence", uiForm.LastFocusSequence.ToString());
                    EditorGUILayout.LabelField("Is Opened", uiForm.IsOpened.ToString());
                    
                    MonoBehaviour currentFormLogic = uiForm.UIFormLogic as MonoBehaviour;
                    if (currentFormLogic != null)
                    {
                        EditorGUILayout.ObjectField("Form Logic", currentFormLogic, typeof(MonoBehaviour), true);
                    }
                    
                    EditorGUILayout.LabelField("Release Tag", uiForm.ReleaseTag.ToString());
                }
                EditorGUILayout.EndVertical();
            }
        }

        private static string FormatInstanceKey(string instanceKey)
        {
            return string.IsNullOrEmpty(instanceKey) ? "<null>" : instanceKey;
        }


    }
}
