using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{
    [CustomEditor(typeof(SaveComponent))]
    public class SaveComponentInspector : FrameworkInspector
    {
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
            EditorGUILayout.PropertyField(settingsProperty, true);
            
            serializedObject.ApplyModifiedProperties();

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

                DrawPlayerDataInfo(s); 
                EditorGUILayout.Space(5);
            }
            
            Repaint();
        }

        private void DrawPlayerDataRuntime(SaveComponent s)
        {
            
            if (!s.PlayerDataLoaded) return;
            PlayerData data = s.PlayerData;

            playerDataFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(playerDataFoldout, "玩家数据（运行时）", EditorStyles.boldLabel);
            
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
            GameSettings settings = s.GameSettings;

            gameSettingsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(gameSettingsFoldout, "游戏设置（运行时）", EditorStyles.boldLabel);
            
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
            dataLoadFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(dataLoadFoldout, "存档状态", EditorStyles.boldLabel);
            
            if(dataLoadFoldout)
            {
                string dataState = s.PlayerDataLoaded ? "已加载" : "未加载" ;
                EditorGUILayout.LabelField("存档状态", dataState);
                if (s.CurrentLoadedDataIndex != -1) // 存档已加载的另一种表示
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("存档索引", s.CurrentLoadedDataIndex.ToString());
                    EditorGUILayout.EndVertical();
                }

                string defaultLoad = s.DefaultDataIndex == -1 ? "禁止" : "启用";
                EditorGUILayout.LabelField("默认加载", defaultLoad);
                if (s.DefaultDataIndex != -1)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("默认加载存档索引", s.DefaultDataIndex.ToString());
                    EditorGUILayout.EndVertical();
                }

                string autoSave = s.AutoSave ? "启用" : "关闭";
                EditorGUILayout.LabelField("自动存档", autoSave);
                EditorGUILayout.LabelField("自动存档间隔", s.AutoSaveDataInterval.ToString("F2") + "s");
                EditorGUILayout.LabelField("上次自动保存时间", s.LastAutoSaveTime.ToString("F2") + "s");
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSaveInfo(SaveComponent s)
        {
            saveInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(saveInfoFoldout, "存档注释", EditorStyles.boldLabel);
            if(saveInfoFoldout)
            {
                EditorGUILayout.LabelField("当前默认存档注释", s.AutoSaveInfo);
                saveInfoListFoldout = EditorGUILayout.Foldout(saveInfoListFoldout, "存档注释列表");
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

        private void DrawPlayerDataInfo(SaveComponent s)
        {
            dataInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(dataInfoFoldout, "存档信息", EditorStyles.boldLabel);
            if(dataInfoFoldout)
            {
                EditorGUILayout.LabelField("存档信息列表", s.AutoSaveInfo);
                EditorGUILayout.BeginVertical("box");
                {
                    if (s.DataInfoDic != null && s.DataInfoDic.Count != 0)
                    {
                        foreach (var item in s.DataInfoDic.Values)
                        {
                            EditorGUILayout.LabelField($"{item.index}      {item.time}",   item.note);
                        }
                    }
                    else
                        EditorGUILayout.LabelField("None");
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
