using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework.StarryUnityEditor
{
    [CustomEditor(typeof(SaveComponent))]
    public class SaveComponentInspector : StarryFrameworkInspector
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
            gameSettingsObject.Update();
        }


        public override void OnInspectorGUI()
        {
            Debug.Log(playerDataObject == null);

            base.OnInspectorGUI();

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            SaveComponent s = (SaveComponent)target;

            DrawInfos(s);
            EditorGUILayout.Space(5);

            DrawGameSettings(s);
            EditorGUILayout.Space(5);

            DrawSaveInfo(s);
            EditorGUILayout.Space(5);

            DrawPlayerDataInfo(s); 
            EditorGUILayout.Space(5);

            Repaint();
        }


        private void DrawInfos(SaveComponent s)
        {
            DataLoadFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(DataLoadFoldout, "存档管理", EditorStyles.boldLabel);
            
            if(DataLoadFoldout)
            {
                string dataState = s.CurrentLoadedDataIndex == -1 ? "未加载" : "已加载";
                EditorGUILayout.LabelField("存档状态", dataState);
                if (s.CurrentLoadedDataIndex != -1)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("存档编号", s.CurrentLoadedDataIndex.ToString());
                    DrawPlayerData(s);//当前存档内容
                    EditorGUILayout.EndVertical();
                }


                string defaultLoad = s.DefaultDataIndex == -1 ? "禁止" : "允许";
                EditorGUILayout.LabelField("默认加载", defaultLoad);
                if (s.DefaultDataIndex != -1)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("默认加载存档编号", s.DefaultDataIndex.ToString());
                    EditorGUILayout.EndVertical();
                }

                string autoSave = s.AutoSave ? "开启" : "关闭";
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
                        // 获取对象的类型
                        System.Type objectType = s.PlayerData.GetType();

                        // 使用反射获取对象的所有字段
                        FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        foreach (FieldInfo field in fields)
                        {
                            // 通过SerializedObject获取字段的SerializedProperty
                            SerializedProperty serializedProperty = playerDataObject.FindProperty(field.Name);

                            // 显示字段名称和字段值
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
            SettingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(SettingFoldout, "设置管理", EditorStyles.boldLabel);

            SerializeSetting(s.GameSettings);

            if (SettingFoldout && gameSettingsObject !=null)
            {
                System.Type objectType = s.GameSettings.GetType();

                // 使用反射获取对象的所有字段
                FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    // 通过SerializedObject获取字段的SerializedProperty
                    SerializedProperty serializedProperty = gameSettingsObject.FindProperty(field.Name);

                    // 显示字段名称和字段值
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
