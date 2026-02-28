using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    [CustomEditor(typeof(SaveComponent))]
    public class SaveComponentInspector : FrameworkInspector
    {
        private const string DefaultEditorSaveDataDirectoryPath = "Assets/SaveData";
        private bool dataLoadFoldout = true;
        private bool saveInfoFoldout = true;
        private bool dataInfoFoldout = true;
        private bool playerDataFoldout = true;
        private bool gameSettingsFoldout = true;
        private bool saveInfoListFoldout = false;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            SaveComponent s = (SaveComponent)target;
            
            serializedObject.Update();
            
            SerializedProperty settingsProperty = serializedObject.FindProperty("settings");
            DrawSaveSettings(settingsProperty);
            
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);
            DrawPlayerDataInfo();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.Space(10);
                
                DrawPlayerDataRuntime(s);
                EditorGUILayout.Space(5);

                DrawGameSettingsRuntime(s);
                EditorGUILayout.Space(5);

                DrawInfos(s);
                EditorGUILayout.Space(5);

                DrawSaveInfo(s);
                EditorGUILayout.Space(5);
            }
            
            Repaint();
        }

        private void DrawSaveSettings(SerializedProperty settingsProperty)
        {
            if (settingsProperty == null)
            {
                EditorGUILayout.HelpBox("Save settings property was not found.", MessageType.Error);
                return;
            }

            EditorGUILayout.PropertyField(settingsProperty, false);
            if (!settingsProperty.isExpanded)
            {
                return;
            }

            EditorGUI.indentLevel++;

            DrawSettingsChildProperty(settingsProperty, "AutoSave");
            DrawSettingsChildProperty(settingsProperty, "AutoSaveDataInterval");
            DrawEditorSaveDataDirectoryPathField(settingsProperty.FindPropertyRelative("EditorSaveDataDirectoryPath"));
            DrawSettingsChildProperty(settingsProperty, "SaveInfoList", includeChildren: true);
            EditorGUILayout.HelpBox("SaveDataProvider is discovered automatically via [SaveDataProvider] attribute.", MessageType.Info);

            EditorGUI.indentLevel--;
        }

        private static void DrawSettingsChildProperty(SerializedProperty settingsProperty, string propertyName, bool includeChildren = false)
        {
            SerializedProperty property = settingsProperty.FindPropertyRelative(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, includeChildren);
            }
        }

        private void DrawEditorSaveDataDirectoryPathField(SerializedProperty pathProperty)
        {
            if (pathProperty == null)
            {
                return;
            }

            GUIContent label = new GUIContent(pathProperty.displayName, pathProperty.tooltip);
            string currentValue = pathProperty.stringValue ?? string.Empty;
            const float buttonWidth = 70f;
            const float spacing = 4f;

            Rect rowRect = EditorGUILayout.GetControlRect();
            Rect fieldRect = rowRect;
            fieldRect.width -= buttonWidth + spacing;

            Rect buttonRect = rowRect;
            buttonRect.x = fieldRect.xMax + spacing;
            buttonRect.width = buttonWidth;

            string newValue = EditorGUI.TextField(fieldRect, label, currentValue);
            if (GUI.Button(buttonRect, "Select..."))
            {
                SelectEditorSaveDataDirectory(pathProperty);
            }

            if (!string.Equals(newValue, currentValue, StringComparison.Ordinal))
            {
                pathProperty.stringValue = NormalizeAssetsRelativePath(newValue);
            }

            if (!IsValidAssetsRelativePath(pathProperty.stringValue))
            {
                EditorGUILayout.HelpBox("Use a path under Assets (for example: Assets/SaveData).", MessageType.Warning);
            }
        }

        private void SelectEditorSaveDataDirectory(SerializedProperty pathProperty)
        {
            string startPath = GetFolderPickerStartPath(pathProperty.stringValue);
            string selectedFolder = EditorUtility.OpenFolderPanel("Select Editor Save Data Directory", startPath, string.Empty);
            if (string.IsNullOrEmpty(selectedFolder))
            {
                return;
            }

            if (!TryConvertAbsolutePathToAssetsRelative(selectedFolder, out string assetsRelativePath))
            {
                EditorUtility.DisplayDialog("Invalid Folder", "Please select a folder under this project's Assets folder.", "OK");
                return;
            }

            pathProperty.stringValue = assetsRelativePath;
        }

        private static string GetFolderPickerStartPath(string assetsRelativePath)
        {
            if (TryConvertAssetsRelativeToAbsolutePath(assetsRelativePath, out string absolutePath) && System.IO.Directory.Exists(absolutePath))
            {
                return absolutePath;
            }

            return Application.dataPath;
        }

        private static string NormalizeAssetsRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return path.Trim().Replace('\\', '/');
        }

        private static bool IsValidAssetsRelativePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return true;
            }

            string normalizedPath = NormalizeAssetsRelativePath(path);
            return normalizedPath.Equals("Assets", StringComparison.OrdinalIgnoreCase) ||
                   normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool TryConvertAssetsRelativeToAbsolutePath(string assetsRelativePath, out string absolutePath)
        {
            absolutePath = null;
            if (!IsValidAssetsRelativePath(assetsRelativePath))
            {
                return false;
            }

            string normalizedPath = NormalizeAssetsRelativePath(assetsRelativePath);
            if (string.IsNullOrEmpty(normalizedPath) || normalizedPath.Equals("Assets", StringComparison.OrdinalIgnoreCase))
            {
                absolutePath = Application.dataPath;
                return true;
            }

            string subPath = normalizedPath.Substring("Assets/".Length).Replace('/', System.IO.Path.DirectorySeparatorChar);
            absolutePath = System.IO.Path.Combine(Application.dataPath, subPath);
            return true;
        }

        private static bool TryConvertAbsolutePathToAssetsRelative(string absolutePath, out string assetsRelativePath)
        {
            assetsRelativePath = null;
            if (string.IsNullOrWhiteSpace(absolutePath))
            {
                return false;
            }

            string normalizedSelectedPath = System.IO.Path.GetFullPath(absolutePath).Replace('\\', '/').TrimEnd('/');
            string normalizedAssetsPath = System.IO.Path.GetFullPath(Application.dataPath).Replace('\\', '/').TrimEnd('/');

            bool isAssetsRoot = normalizedSelectedPath.Equals(normalizedAssetsPath, StringComparison.OrdinalIgnoreCase);
            bool isAssetsChild = normalizedSelectedPath.StartsWith(normalizedAssetsPath + "/", StringComparison.OrdinalIgnoreCase);
            if (!isAssetsRoot && !isAssetsChild)
            {
                return false;
            }

            if (isAssetsRoot)
            {
                assetsRelativePath = "Assets";
                return true;
            }

            string relativeSuffix = normalizedSelectedPath.Substring(normalizedAssetsPath.Length + 1);
            assetsRelativePath = "Assets/" + relativeSuffix;
            return true;
        }

        private void DrawPlayerDataRuntime(SaveComponent s)
        {
            
            if (!s.PlayerDataLoaded) return;
            object data = s.GetPlayerDataObject();
            if (data == null) return;

            playerDataFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(playerDataFoldout, "Player Data (Runtime)", EditorStyles.boldLabel);
            
            if (playerDataFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                DrawObjectFields(data, data.GetType());
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawGameSettingsRuntime(SaveComponent s)
        {
            if (!s.GameSettingsLoaded) return;
            object settings = s.GetGameSettingsObject();
            if (settings == null) return;

            gameSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(gameSettingsFoldout, "Game Settings (Runtime)", EditorStyles.boldLabel);
            
            if (gameSettingsFoldout)
            {
                EditorGUILayout.BeginVertical("box");
                DrawObjectFields(settings, settings.GetType());
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawObjectFields(object obj, Type type)
        {
            if (obj == null) return;

            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(obj);
                object newValue = DrawField(field.Name, value, field.FieldType);
                
                if (newValue != null && !newValue.Equals(value))
                {
                    field.SetValue(obj, newValue);
                    EditorUtility.SetDirty(target);
                }
            }
        }

        private object DrawField(string fieldName, object value, Type fieldType)
        {
            if (fieldType == typeof(int))
            {
                return EditorGUILayout.IntField(ObjectNames.NicifyVariableName(fieldName), (int)value);
            }
            else if (fieldType == typeof(float))
            {
                return EditorGUILayout.FloatField(ObjectNames.NicifyVariableName(fieldName), (float)value);
            }
            else if (fieldType == typeof(double))
            {
                return EditorGUILayout.DoubleField(ObjectNames.NicifyVariableName(fieldName), (double)value);
            }
            else if (fieldType == typeof(bool))
            {
                return EditorGUILayout.Toggle(ObjectNames.NicifyVariableName(fieldName), (bool)value);
            }
            else if (fieldType == typeof(string))
            {
                return EditorGUILayout.TextField(ObjectNames.NicifyVariableName(fieldName), (string)value ?? "");
            }
            else if (fieldType == typeof(Vector2))
            {
                return EditorGUILayout.Vector2Field(ObjectNames.NicifyVariableName(fieldName), (Vector2)value);
            }
            else if (fieldType == typeof(Vector3))
            {
                return EditorGUILayout.Vector3Field(ObjectNames.NicifyVariableName(fieldName), (Vector3)value);
            }
            else if (fieldType == typeof(Vector4))
            {
                return EditorGUILayout.Vector4Field(ObjectNames.NicifyVariableName(fieldName), (Vector4)value);
            }
            else if (fieldType == typeof(Color))
            {
                return EditorGUILayout.ColorField(ObjectNames.NicifyVariableName(fieldName), (Color)value);
            }
            else if (fieldType.IsEnum)
            {
                return EditorGUILayout.EnumPopup(ObjectNames.NicifyVariableName(fieldName), (Enum)value);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(fieldType))
            {
                return EditorGUILayout.ObjectField(ObjectNames.NicifyVariableName(fieldName), (UnityEngine.Object)value, fieldType, true);
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                DrawList(fieldName, value as IList, fieldType);
                return value;
            }
            else if (fieldType.IsArray)
            {
                DrawArray(fieldName, value as Array, fieldType);
                return value;
            }
            else if (fieldType.IsClass && fieldType.IsSerializable)
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName), $"({fieldType.Name})");
                if (value != null)
                {
                    EditorGUI.indentLevel++;
                    DrawObjectFields(value, fieldType);
                    EditorGUI.indentLevel--;
                }
                return value;
            }
            else
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName), $"{value} ({fieldType.Name})");
                return value;
            }
        }

        private void DrawList(string fieldName, IList list, Type listType)
        {
            if (list == null)
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName), "null");
                return;
            }

            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName), $"List<{listType.GetGenericArguments()[0].Name}> (Count: {list.Count})");
            
            EditorGUI.indentLevel++;
            for (int i = 0; i < list.Count; i++)
            {
                Type elementType = listType.GetGenericArguments()[0];
                object element = list[i];
                object newElement = DrawField($"Element {i}", element, elementType);
                
                if (newElement != null && !newElement.Equals(element))
                {
                    list[i] = newElement;
                }
            }
            EditorGUI.indentLevel--;
        }

        private void DrawArray(string fieldName, Array array, Type arrayType)
        {
            if (array == null)
            {
                EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName), "null");
                return;
            }

            Type elementType = arrayType.GetElementType();
            EditorGUILayout.LabelField(ObjectNames.NicifyVariableName(fieldName), $"{elementType.Name}[] (Length: {array.Length})");
            
            EditorGUI.indentLevel++;
            for (int i = 0; i < array.Length; i++)
            {
                object element = array.GetValue(i);
                object newElement = DrawField($"Element {i}", element, elementType);
                
                if (newElement != null && !newElement.Equals(element))
                {
                    array.SetValue(newElement, i);
                }
            }
            EditorGUI.indentLevel--;
        }

        private void DrawInfos(SaveComponent s)
        {
            dataLoadFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(dataLoadFoldout, "Save State", EditorStyles.boldLabel);
            
            if(dataLoadFoldout)
            {
                string dataState = s.PlayerDataLoaded ? "Loaded" : "Not Loaded";
                EditorGUILayout.LabelField("Save State", dataState);
                if (s.CurrentLoadedDataIndex != -1) // Alternative indicator that a save is currently loaded.
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Current Save Index", s.CurrentLoadedDataIndex.ToString());
                    EditorGUILayout.EndVertical();
                }

                string defaultLoad = s.DefaultDataIndex == -1 ? "Disabled" : "Enabled";
                EditorGUILayout.LabelField("Default Load", defaultLoad);
                if (s.DefaultDataIndex != -1)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Default Save Index", s.DefaultDataIndex.ToString());
                    EditorGUILayout.EndVertical();
                }

                string autoSave = s.AutoSave ? "Enabled" : "Disabled";
                EditorGUILayout.LabelField("Auto Save", autoSave);
                EditorGUILayout.LabelField("Auto Save Interval", s.AutoSaveDataInterval.ToString("F2") + "s");
                EditorGUILayout.LabelField("Last Auto Save Time", s.LastAutoSaveTime.ToString("F2") + "s");
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSaveInfo(SaveComponent s)
        {
            saveInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(saveInfoFoldout, "Save Notes", EditorStyles.boldLabel);
            if(saveInfoFoldout)
            {
                EditorGUILayout.LabelField("Current Default Save Note", s.AutoSaveInfo);
                saveInfoListFoldout = EditorGUILayout.Foldout(saveInfoListFoldout, "Save Note List");
                if (saveInfoListFoldout)
                {
                    EditorGUILayout.BeginVertical("box");
                    if (s.SaveInfoList != null && s.SaveInfoList.Count != 0)
                        for (int i = 0; i < s.SaveInfoList.Count; i++)
                        {
                            EditorGUILayout.LabelField(i.ToString(), s.SaveInfoList[i]);
                        }
                    else
                        EditorGUILayout.LabelField("None");
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup(); 
        }

        private void DrawPlayerDataInfo()
        {
            dataInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(dataInfoFoldout, "Saved Data List (Disk)", EditorStyles.boldLabel);
            if (dataInfoFoldout)
            {
                string configuredAssetsPath = GetConfiguredEditorSaveDataDirectoryPath();
                EditorGUILayout.LabelField("Save Directory", configuredAssetsPath);

                if (!IsValidAssetsRelativePath(configuredAssetsPath))
                {
                    EditorGUILayout.HelpBox("Use a path under Assets (for example: Assets/SaveData).", MessageType.Warning);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    return;
                }

                if (!TryConvertAssetsRelativeToAbsolutePath(configuredAssetsPath, out string absolutePath))
                {
                    EditorGUILayout.HelpBox("Failed to resolve save directory path.", MessageType.Error);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    return;
                }

                EditorGUILayout.BeginVertical("box");
                {
                    if (!Directory.Exists(absolutePath))
                    {
                        EditorGUILayout.LabelField("None");
                        EditorGUILayout.LabelField("Status", "Save directory not found.");
                    }
                    else if (TryReadSaveInfosFromDisk(absolutePath, out List<PlayerDataInfo> saveInfos, out List<string> invalidFiles))
                    {
                        if (saveInfos.Count == 0)
                        {
                            EditorGUILayout.LabelField("None");
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Count", saveInfos.Count.ToString());
                            foreach (PlayerDataInfo item in saveInfos)
                            {
                                string note = string.IsNullOrEmpty(item.note) ? "(No Note)" : item.note;
                                EditorGUILayout.LabelField($"{item.index}      {item.time}", note);
                            }
                        }

                        if (invalidFiles.Count > 0)
                        {
                            EditorGUILayout.Space(4);
                            EditorGUILayout.HelpBox($"Skipped {invalidFiles.Count} invalid save info file(s).", MessageType.Warning);
                            foreach (string invalidFile in invalidFiles)
                            {
                                EditorGUILayout.LabelField("Invalid", invalidFile);
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("None");
                        EditorGUILayout.LabelField("Status", "Failed to read save directory.");
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private string GetConfiguredEditorSaveDataDirectoryPath()
        {
            SerializedProperty settingsProperty = serializedObject.FindProperty("settings");
            SerializedProperty pathProperty = settingsProperty?.FindPropertyRelative("EditorSaveDataDirectoryPath");
            string configuredPath = NormalizeAssetsRelativePath(pathProperty?.stringValue);
            return string.IsNullOrWhiteSpace(configuredPath) ? DefaultEditorSaveDataDirectoryPath : configuredPath;
        }

        private static bool TryReadSaveInfosFromDisk(string absolutePath, out List<PlayerDataInfo> saveInfos, out List<string> invalidFiles)
        {
            saveInfos = new List<PlayerDataInfo>();
            invalidFiles = new List<string>();

            try
            {
                foreach (string filePath in Directory.EnumerateFiles(absolutePath, "SaveDataInfo*.save"))
                {
                    try
                    {
                        string json = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                        PlayerDataInfo info = JsonConvert.DeserializeObject<PlayerDataInfo>(json);
                        if (info == null)
                        {
                            invalidFiles.Add(Path.GetFileName(filePath));
                            continue;
                        }

                        saveInfos.Add(info);
                    }
                    catch
                    {
                        invalidFiles.Add(Path.GetFileName(filePath));
                    }
                }

                saveInfos.Sort((a, b) => a.index.CompareTo(b.index));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
