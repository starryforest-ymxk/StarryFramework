using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework.Editor
{
    /// <summary>
    /// SaveComponent的自定义Inspector面板
    /// 运行时显示存档状态、玩家数据、游戏设置等信息
    /// </summary>
    [CustomEditor(typeof(SaveComponent))]
    public class SaveComponentInspector : FrameworkInspector
    {
        private bool DataLoadFoldout = true;
        private bool SaveInfoFoldout = true;
        private bool SettingFoldout = true;
        private bool DataInfoFoldout = true;

        private bool DataFoldout = false;

        private bool saveInfoListFoldout = false;

        private PlayerData _data;
        private GameSettings _gameSettings;
        private SerializedObject playerDataObject;
        private SerializedObject gameSettingsObject;

        private void SerializeData(PlayerData data)
        {
            
            if(data == null) 
            {
                playerDataObject = null;
            }
            else if(playerDataObject == null || data != _data)
            {
                playerDataObject = new SerializedObject(data);
                _data = data;
            }
            if(playerDataObject!=null)
                playerDataObject.Update();
        }

        private void SerializeSetting(GameSettings gameSettings)
        {
            
            if (gameSettings == null)
            {
                gameSettingsObject = null;
            }
            else if (gameSettingsObject == null || gameSettings != _gameSettings)
            {
                gameSettingsObject = new SerializedObject(gameSettings);
                _gameSettings = gameSettings;
            }
            if(gameSettingsObject != null)
                gameSettingsObject.Update();
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            SaveComponent s = (SaveComponent)target;
            
            serializedObject.Update();
            
            SerializedProperty mySerializableProperty = serializedObject.FindProperty("settings");
            EditorGUILayout.PropertyField(mySerializableProperty, true);
            
            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying)
            {
                DrawInfos(s);
                EditorGUILayout.Space(5);

                DrawGameSettings(s);
                EditorGUILayout.Space(5);

                DrawSaveInfo(s);
                EditorGUILayout.Space(5);

                DrawPlayerDataInfo(s); 
                EditorGUILayout.Space(5);
            }
            
            Repaint();
        }


        private void DrawInfos(SaveComponent s)
        {
            DataLoadFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(DataLoadFoldout, "存档加载", EditorStyles.boldLabel);
            
            if(DataLoadFoldout)
            {
                string dataState = s.CurrentLoadedDataIndex == -1 ? "未加载" : "已加载";
                EditorGUILayout.LabelField("存档状态", dataState);
                if (s.CurrentLoadedDataIndex != -1)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("存档索引", s.CurrentLoadedDataIndex.ToString());
                    DrawPlayerData(s);
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


            void DrawPlayerData(SaveComponent s)
            {
                DataFoldout = EditorGUILayout.Foldout(DataFoldout, "Player Data");

                SerializeData(s.PlayerData);

                if (DataFoldout && playerDataObject != null)
                {
                    EditorGUILayout.BeginVertical("box");
                    {
                        System.Type objectType = s.PlayerData.GetType();

                        FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        foreach (FieldInfo field in fields)
                        {
                            SerializedProperty serializedProperty = playerDataObject.FindProperty(field.Name);

                            if (serializedProperty != null)
                            {
                                EditorGUILayout.PropertyField(serializedProperty);
                            }
                        }

                    }
                    EditorGUILayout.EndVertical();

                    playerDataObject.ApplyModifiedProperties();

                }
                
            }
        }

        private void DrawGameSettings(SaveComponent s)
        {
            SettingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(SettingFoldout, "游戏设置", EditorStyles.boldLabel);

            SerializeSetting(s.GameSettings);

            if (SettingFoldout && gameSettingsObject !=null)
            {
                System.Type objectType = s.GameSettings.GetType();

                FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    SerializedProperty serializedProperty = gameSettingsObject.FindProperty(field.Name);

                    if (serializedProperty != null)
                    {
                        EditorGUILayout.PropertyField(serializedProperty);
                    }
                }

                gameSettingsObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawSaveInfo(SaveComponent s)
        {
            SaveInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(SaveInfoFoldout, "存档注释", EditorStyles.boldLabel);
            if(SaveInfoFoldout)
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
            DataInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(DataInfoFoldout, "存档信息", EditorStyles.boldLabel);
            if(DataInfoFoldout)
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
