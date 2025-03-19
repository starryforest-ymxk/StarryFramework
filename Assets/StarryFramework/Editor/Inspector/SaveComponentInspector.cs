using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework.Editor
{
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
            DataLoadFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(DataLoadFoldout, "�浵����", EditorStyles.boldLabel);
            
            if(DataLoadFoldout)
            {
                string dataState = s.CurrentLoadedDataIndex == -1 ? "δ����" : "�Ѽ���";
                EditorGUILayout.LabelField("�浵״̬", dataState);
                if (s.CurrentLoadedDataIndex != -1)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("�浵���", s.CurrentLoadedDataIndex.ToString());
                    DrawPlayerData(s);//��ǰ�浵����
                    EditorGUILayout.EndVertical();
                }


                string defaultLoad = s.DefaultDataIndex == -1 ? "��ֹ" : "����";
                EditorGUILayout.LabelField("Ĭ�ϼ���", defaultLoad);
                if (s.DefaultDataIndex != -1)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Ĭ�ϼ��ش浵���", s.DefaultDataIndex.ToString());
                    EditorGUILayout.EndVertical();
                }

                string autoSave = s.AutoSave ? "����" : "�ر�";
                EditorGUILayout.LabelField("�Զ��浵", autoSave);
                EditorGUILayout.LabelField("�Զ��浵���", s.AutoSaveDataInterval.ToString("F2") + "s");
                EditorGUILayout.LabelField("�ϴ��Զ�����ʱ��", s.LastAutoSaveTime.ToString("F2") + "s");

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
                        // ��ȡ���������
                        System.Type objectType = s.PlayerData.GetType();

                        // ʹ�÷����ȡ����������ֶ�
                        FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        foreach (FieldInfo field in fields)
                        {
                            // ͨ��SerializedObject��ȡ�ֶε�SerializedProperty
                            SerializedProperty serializedProperty = playerDataObject.FindProperty(field.Name);

                            // ��ʾ�ֶ����ƺ��ֶ�ֵ
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
            SettingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(SettingFoldout, "���ù���", EditorStyles.boldLabel);

            SerializeSetting(s.GameSettings);

            if (SettingFoldout && gameSettingsObject !=null)
            {
                System.Type objectType = s.GameSettings.GetType();

                // ʹ�÷����ȡ����������ֶ�
                FieldInfo[] fields = objectType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    // ͨ��SerializedObject��ȡ�ֶε�SerializedProperty
                    SerializedProperty serializedProperty = gameSettingsObject.FindProperty(field.Name);

                    // ��ʾ�ֶ����ƺ��ֶ�ֵ
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
            SaveInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(SaveInfoFoldout, "�浵ע��", EditorStyles.boldLabel);
            if(SaveInfoFoldout)
            {
                EditorGUILayout.LabelField("��ǰĬ�ϴ浵ע��", s.AutoSaveInfo);
                saveInfoListFoldout = EditorGUILayout.Foldout(saveInfoListFoldout, "�浵ע���б�");
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
            DataInfoFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(DataInfoFoldout, "�浵��Ϣ", EditorStyles.boldLabel);
            if(DataInfoFoldout)
            {
                EditorGUILayout.LabelField("�浵��Ϣ�б�", s.AutoSaveInfo);
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
