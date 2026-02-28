using System;
using System.Collections.Generic;
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
        private readonly Dictionary<string, bool> uiGroupFoldouts = new();
        private readonly Dictionary<string, bool> uiFormFoldouts = new();
        private readonly Dictionary<string, bool> uiFormInfoFoldouts = new();
        
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
                                $"[{topmost.SerialID}] Group={topmost.UIGroup?.Name ?? "null"}, Policy={topmost.OpenPolicy}, InstanceKey={FormatInstanceKey(topmost.InstanceKey)}");

                            foreach (UIForm form in forms)
                            {
                                DrawUIForm(form, $"active:{assetGroup.Key}");
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
                        DrawUIForm(uiForm, "cache");
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
            string foldoutKey = BuildUIGroupFoldoutKey(groupName);
            bool isExpanded = GetFoldoutState(uiGroupFoldouts, foldoutKey);
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);
            EditorGUILayout.BeginHorizontal();
            {
                isExpanded = EditorGUI.Foldout(r, isExpanded, GUIContent.none);
                EditorGUI.LabelField(r, groupName);
            }
            EditorGUILayout.EndHorizontal();
            uiGroupFoldouts[foldoutKey] = isExpanded;

            if (isExpanded)
            {
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("Form Count", uiGroup.FormCount.ToString());
                    EditorGUILayout.LabelField("Pause", uiGroup.Pause.ToString());
                    if (uiGroup.CurrentForm != null)
                    {
                        EditorGUILayout.LabelField("Current Form");
                        EditorGUILayout.BeginVertical(StyleFramework.box);
                        DrawUIForm(uiGroup.CurrentForm, $"group:{groupName}:current");
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
                        DrawUIFormInfo(uiFormInfo, $"group:{groupName}:all");
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
        }
        
        private void DrawUIForm(UIForm uiForm, string scope)
        {
            string assetName = uiForm.UIFormAssetName;
            string foldoutKey = BuildUIFormFoldoutKey(scope, uiForm);
            bool isExpanded = GetFoldoutState(uiFormFoldouts, foldoutKey);
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);

            EditorGUILayout.BeginHorizontal();
            {
                isExpanded = EditorGUI.Foldout(r, isExpanded, GUIContent.none);
                EditorGUI.LabelField(r, assetName);
            }
            EditorGUILayout.EndHorizontal();
            uiFormFoldouts[foldoutKey] = isExpanded;

            if (isExpanded)
            {
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("SerialID", uiForm.SerialID.ToString());
                    EditorGUILayout.LabelField("UI Group", uiForm.UIGroup != null ? uiForm.UIGroup.Name : "null");
                    EditorGUILayout.LabelField("Depth in group", uiForm.DepthInUIGroup.ToString());
                    EditorGUILayout.LabelField("Pause Covered", uiForm.PauseCoveredUIForm.ToString());
                    EditorGUILayout.LabelField("Open Policy", uiForm.OpenPolicy.ToString());
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
        
        private void DrawUIFormInfo(UIFormInfo uiFormInfo, string scope)
        {
            UIForm uiForm = uiFormInfo.UIForm;
            string assetName = uiForm.UIFormAssetName;
            string foldoutKey = BuildUIFormInfoFoldoutKey(scope, uiFormInfo);
            bool isExpanded = GetFoldoutState(uiFormInfoFoldouts, foldoutKey);
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none);

            EditorGUILayout.BeginHorizontal();
            {
                isExpanded = EditorGUI.Foldout(r, isExpanded, GUIContent.none);
                EditorGUI.LabelField(r, assetName);
            }
            EditorGUILayout.EndHorizontal();
            uiFormInfoFoldouts[foldoutKey] = isExpanded;

            if (isExpanded)
            {
                EditorGUILayout.BeginVertical(StyleFramework.box);
                {
                    EditorGUILayout.LabelField("SerialID", uiForm.SerialID.ToString());
                    EditorGUILayout.LabelField("Depth in group", uiForm.DepthInUIGroup.ToString());
                    EditorGUILayout.LabelField("Paused", uiFormInfo.Paused.ToString());
                    EditorGUILayout.LabelField("Covered", uiFormInfo.Covered.ToString());
                    EditorGUILayout.LabelField("Pause Covered", uiForm.PauseCoveredUIForm.ToString());
                    EditorGUILayout.LabelField("Open Policy", uiForm.OpenPolicy.ToString());
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

        private static bool GetFoldoutState(Dictionary<string, bool> foldouts, string key)
        {
            if (foldouts == null || key == null)
            {
                return false;
            }

            return foldouts.TryGetValue(key, out bool state) && state;
        }

        private static string BuildUIGroupFoldoutKey(string groupName)
        {
            return $"group:{groupName ?? string.Empty}";
        }

        private static string BuildUIFormFoldoutKey(string scope, UIForm uiForm)
        {
            if (uiForm == null)
            {
                return $"{scope}|null";
            }

            string groupName = uiForm.UIGroup?.Name ?? string.Empty;
            return $"{scope}|{uiForm.SerialID}|{uiForm.UIFormAssetName ?? string.Empty}|{groupName}|{uiForm.InstanceKey ?? string.Empty}";
        }

        private static string BuildUIFormInfoFoldoutKey(string scope, UIFormInfo uiFormInfo)
        {
            UIForm uiForm = uiFormInfo?.UIForm;
            if (uiForm == null)
            {
                return $"{scope}|null";
            }

            string groupName = uiForm.UIGroup?.Name ?? string.Empty;
            return $"{scope}|{uiForm.SerialID}|{uiForm.UIFormAssetName ?? string.Empty}|{groupName}|{uiForm.InstanceKey ?? string.Empty}";
        }

        private static string FormatInstanceKey(string instanceKey)
        {
            return string.IsNullOrEmpty(instanceKey) ? "<null>" : instanceKey;
        }


    }
}
