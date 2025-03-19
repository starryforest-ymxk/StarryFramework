using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;

namespace StarryFramework
{
    internal class SaveManager : IManager
    {

        private SaveSettings settings;

        //��ǰ��ϷĬ�ϼ��صĴ浵��ţ������Զ��浵�Լ����ٴ浵
        //ֻ����Ϸ��ʼǰ����ֵ��Ϊ-1����������Ϸ����ť�Ż���Ч
        private int defaultDataIndex = -1;
        //��ǰ��Ϸ�Ѽ��ش浵���
        private int currentLoadedDataIndex = -1;

        private float autoSaveDataInterval;

        private float lastAutoSaveTime;

        //��ͣ�Զ��浵��־λ
        private bool startAutoSave;

        private string autoSaveInfo = "";

        private List<string> saveInfoList = new();

        private PlayerData playerData;

        private GameSettings gameSettings;



        internal int DefaultDataIndex => defaultDataIndex; 
        internal int CurrentLoadedDataIndex => currentLoadedDataIndex;
        internal float AutoSaveDataInterval =>autoSaveDataInterval;
        internal float LastAutoSaveTime => lastAutoSaveTime;
        internal bool AutoSave => startAutoSave;
        internal string AutoSaveInfo => autoSaveInfo;
        internal List<string> SaveInfoList => saveInfoList;
        internal PlayerData PlayerData => playerData;
        internal GameSettings GameSettings => gameSettings;


        //Ŀǰȫ���Ĵ浵��Ϣ�ʵ�

        internal Dictionary<int, PlayerDataInfo> infoDic = new Dictionary<int, PlayerDataInfo>();


        void IManager.Awake()
        {
            InitInfoDic();
            LoadSetting();
            InitCurrentDataIndex();
        }
        void IManager.Init()
        {
            SetInfoList(settings.SaveInfoList);
            autoSaveDataInterval = settings.AutoSaveDataInterval;
        }
        void IManager.Update()
        {
            if(startAutoSave)
            {
                if (Time.time > lastAutoSaveTime + autoSaveDataInterval)
                {
                    SaveData(autoSaveInfo);
                    lastAutoSaveTime = Time.time;

                }
            }
        }
        void IManager.ShutDown()
        {
            SaveSetting();
            SaveCurrentDataIndex();
            infoDic.Clear();
            saveInfoList.Clear();
            playerData = null;
            gameSettings = null;
            infoDic = null;
            saveInfoList = null;
            autoSaveInfo = null;
        }

        void IManager.SetSettings(IManagerSettings settings)
        {
            this.settings = settings as SaveSettings;
        }

        #region �ڲ�����

        private void SetInfoList(List<string> infos)
        {
            if (infos == null)
                FrameworkManager.Debugger.LogError("Info List can not be null.");
            else
            {
                infos.ForEach(i => saveInfoList.Add(i));
                if (infos.Count != 0)
                {
                    autoSaveInfo = infos[0];
                }

            }
        }

        #region �浵��Ϣ����

        private void InitInfoDic()
        {
            string path = Path.Combine(Application.dataPath, "SaveData");

            if (!Directory.Exists(path)) return;

            foreach (string filePath in Directory.EnumerateFiles(path, "SaveDataInfo*.save"))
            {
                try
                {
                    string js = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    PlayerDataInfo info = JsonUtility.FromJson<PlayerDataInfo>(js);
                    infoDic.Add(info.index, info);
                }
                catch
                {
                    FrameworkManager.Debugger.LogWarning("�浵��Ϣ��");
                    File.Move(filePath, Path.Combine(Application.dataPath, "SaveData", $"Corrupted{Path.GetFileName(filePath)}"));
                    File.Delete(filePath);
                    string metaFilePath = filePath + ".meta";
                    if (File.Exists(metaFilePath))
                    {
                        File.Move(metaFilePath, Path.Combine(Application.dataPath, "SaveData", $"Corrupted{Path.GetFileName(filePath)}.meta"));
                        File.Delete(metaFilePath);
                    }
                }
            }

        }

        private PlayerDataInfo UpdateInfo(int index, string note)
        {
            if (infoDic == null)
            {
                FrameworkManager.Debugger.LogError("Info�ֵ���δ��ʼ��");
                return null;
            }
            if (infoDic.ContainsKey(index))
            {
                infoDic[index].UpdateDataInfo(note);
            }
            else
            {
                PlayerDataInfo info = new PlayerDataInfo(index, note);
                infoDic.Add(index, info);
            }
            return infoDic[index];
        }

        private PlayerDataInfo GetInfo(int index)
        {
            if (infoDic == null)
            {
                FrameworkManager.Debugger.LogError("Info�ֵ���δ��ʼ��");
                return null;
            }
            if (infoDic.TryGetValue(index, out var info))
            {
                return info;
            }
            else
            {
                FrameworkManager.Debugger.LogError("�浵��Ϣ������");
                return null;
            }
        }

        #endregion

        #region ��Ϸ�浵��Ź���

        /// <summary>
        /// [���õ�ǰ�Ѽ��ص���Ϸ���]
        /// ֻ����������»ᷢ���ı䣺
        /// 1 ��������Ϸʱ���˱�ű�Ϊ����Ϸ�Ĵ浵���
        /// 2 ���ش浵ʱ���˱�ű�Ϊ�Ѽ��صĴ浵���
        /// 3 ж�ص�ǰ�浵ʱ���˱������Ϊ-1
        /// </summary>
        private void SetCurrentLoadedDataIndex(int index)
        {
            if (index < -1 || index >= 1000)
            {
                FrameworkManager.Debugger.LogError("�浵��ų�����Χ");
            }
            else
                currentLoadedDataIndex = index;
        }

        /// <summary>
        /// [����Ĭ�ϼ��ص���Ϸ���]
        /// ֻ����������»ᷢ���ı䣺
        /// 1 ����Ϸʱ��ϵͳ��ȡ�ϴε���Ϸ���
        /// 2 ��������Ϸʱ���˱�ű�Ϊ����Ϸ�Ĵ浵���
        /// 3 ������ż��ش浵ʱ���˱�ű�Ϊ���صĴ浵���
        /// 4 ɾ��Ĭ�ϴ浵ʱ���˱������Ϊ-1
        /// </summary>
        private void SetDefaultDataIndex(int index)
        {
            if(index < -1 || index >= 1000)
            {
                FrameworkManager.Debugger.LogError("�浵��ų�����Χ");
            }
            else
                defaultDataIndex= index;
        }
        private void InitCurrentDataIndex()
        {
            //����Ϸʱ��ϵͳ��ȡ�ϴε���Ϸ���
            defaultDataIndex = PlayerPrefs.GetInt("DefaultDataIndex", -1);
        }
        private void SaveCurrentDataIndex()
        {
            PlayerPrefs.SetInt("DefaultDataIndex", defaultDataIndex);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// ��ȡ�µĴ浵��ţ��ɹ�����0-999��ʧ�ܷ���-1
        /// </summary>
        /// <returns>���صı��</returns>
        private int GetNewSaveIndex()
        {
            for (int i = 0; i < 1000; i++)
            {
                if (!infoDic.ContainsKey(i))
                {
                    return i;
                }
            }
            FrameworkManager.Debugger.LogError("�浵��������");
            return -1;
        }

        #endregion

        #endregion

        #region �Զ��浵��ʱ����ͣ
        internal void StartAutoSaveTimer()
        {
            if(currentLoadedDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("�浵δ����");
                return;
            }

            lastAutoSaveTime = Time.time;
            startAutoSave = true;
        }

        internal void StopAutoSaveTimer()
        {
            startAutoSave = false;
        }

        #endregion

        #region �浵����

        /// <summary>
        /// �����´浵
        /// </summary>
        /// <param name="isNewGame">�Ƿ�������Ϸ</param>
        /// <param name="note">�浵��Ϣ</param>
        internal void CreateNewData(bool isNewGame, string note = "")
        {

            int newIndex = GetNewSaveIndex();
            if(newIndex==-1)
            {
                FrameworkManager.Debugger.LogError("�����´浵ʧ��");
                return;
            }
            if(isNewGame)
            {
                playerData = ScriptableObject.CreateInstance<PlayerData>();
                SetDefaultDataIndex(newIndex); //��������Ϸʱ���˱�ű�Ϊ����Ϸ�Ĵ浵���
                SetCurrentLoadedDataIndex(newIndex);
                autoSaveInfo = saveInfoList.Count > 0 ? saveInfoList[0] : "";
            }

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
            var dataPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{newIndex:000}.save");
            var infoPath = Path.Combine(Application.dataPath, "SaveData", $"SaveDataInfo{newIndex:000}.save");
            var dataJs = JsonUtility.ToJson(playerData, true);
            var infoJs = JsonUtility.ToJson(note != "" ? UpdateInfo(newIndex, note) : UpdateInfo(newIndex, autoSaveInfo), true);
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            if (isNewGame)
            {
                if (settings.AutoSave) StartAutoSaveTimer();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            }
            else
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }
        /// <summary>
        /// ����浵,���ٴ浵���Զ��浵
        /// </summary>
        internal void SaveData(string note = "")
        {
            if (currentLoadedDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("�浵��δ����");
                return;
            }
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
            var dataPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{currentLoadedDataIndex:000}.save");
            var infoPath = Path.Combine(Application.dataPath, "SaveData", $"SaveDataInfo{currentLoadedDataIndex:000}.save");
            var dataJs = JsonUtility.ToJson(playerData,true);
            var infoJs = JsonUtility.ToJson(note != "" ? UpdateInfo(currentLoadedDataIndex, note) : UpdateInfo(currentLoadedDataIndex, autoSaveInfo), true);
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }

        /// <summary>
        /// ����浵�����i���ֶ�ѡ��
        /// </summary>
        /// <param name="i">����浵�ı��</param>
        /// <param name="note">�浵ע��</param>
        internal void SaveData(int i, string note = "")
        {
            if (i >= 1000 || i < 0)
            {
                FrameworkManager.Debugger.LogError("�浵��Ų��Ϸ�(0-999)");
                return;
            }
            if (currentLoadedDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("�浵��δ����");
                return;
            }
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
            var dataPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{i:000}.save");
            var infoPath = Path.Combine(Application.dataPath, "SaveData", $"SaveDataInfo{i:000}.save");
            var dataJs = JsonUtility.ToJson(playerData,true);
            var infoJs = JsonUtility.ToJson(note != "" ? UpdateInfo(i, note) : UpdateInfo(i, autoSaveInfo), true);
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }
        /// <summary>
        /// ��ȡ��ǰ�浵
        /// </summary>
        internal bool LoadData()
        {
            if(defaultDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("��ǰ�浵�ѱ�ɾ��");
                return false;
            }
            string dataPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{defaultDataIndex:000}.save");
            string dataMetaPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{defaultDataIndex:000}.save.meta");
            if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(dataPath)) return false;
            try
            {
                var js = File.ReadAllText(dataPath, System.Text.Encoding.UTF8);
                if(playerData == null)
                    playerData = ScriptableObject.CreateInstance<PlayerData>();
                JsonUtility.FromJsonOverwrite(js, playerData);
            }
            catch
            {
                FrameworkManager.Debugger.LogError("�浵��");
                File.Move(dataPath, Path.Combine(Application.dataPath, "SaveData", $"CorruptedSaveData{defaultDataIndex:000}.save"));
                File.Delete(dataPath);
                if (File.Exists(dataMetaPath))
                {
                    File.Move(dataPath, Path.Combine(Application.dataPath, "SaveData", $"CorruptedSaveData{defaultDataIndex:000}.save.meta"));
                    File.Delete(dataMetaPath);
                }
                return false;
            }
            SetCurrentLoadedDataIndex(defaultDataIndex);
            if (settings.AutoSave) StartAutoSaveTimer();
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            return true;
        }
        /// <summary>
        /// ��ȡ��ǰ�浵��Ϣ
        /// </summary>
        internal PlayerDataInfo LoadDataInfo()
        {
            if (defaultDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("��ǰ�浵��Ϣ�ѱ�ɾ��");
                return null;
            }
            return GetInfo(defaultDataIndex);
        }

        /// <summary>
        /// �ֶ���ȡ���Ϊi�Ĵ浵
        /// </summary>
        /// <param name="i">�浵���</param>
        internal bool LoadData(int i)
        {
            string dataPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{i:000}.save");
            string dataMetaPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{i:000}.save.meta");
            if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(dataPath))
            {
                FrameworkManager.Debugger.LogError("�浵������");
                return false;
            }
            try
            {
                string js = File.ReadAllText(dataPath, System.Text.Encoding.UTF8);
                if(playerData == null)
                    playerData = ScriptableObject.CreateInstance<PlayerData>();
                JsonUtility.FromJsonOverwrite(js, playerData);
            }
            catch
            {
                FrameworkManager.Debugger.LogError("�浵��");
                File.Move(dataPath, Path.Combine(Application.dataPath, "SaveData", $"CorruptedSaveData{i:000}.save"));
                File.Delete(dataPath);
                if (File.Exists(dataMetaPath))
                {
                    File.Move(dataMetaPath, Path.Combine(Application.dataPath, "SaveData", $"CorruptedSaveData{i:000}.save.meta"));
                    File.Delete(dataMetaPath);
                }
                return false;
            }
            SetDefaultDataIndex(i);// ��ȡ�浵ʱ���˱�ű�Ϊ��ȡ�Ĵ浵���
            SetCurrentLoadedDataIndex(i);
            if (settings.AutoSave) StartAutoSaveTimer();
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            return true;
        }
        /// <summary>
        /// ��ȡ���Ϊi�Ĵ浵��Ϣ
        /// </summary>
        /// <returns>���Ϊi�Ĵ浵��Ϣ</returns>
        internal PlayerDataInfo LoadDataInfo(int i)
        {
            return GetInfo(i);
        }
        /// <summary>
        /// ж�ش浵�����˳�����Ϸʱ����
        /// </summary>
        internal bool UnloadData()
        {
            if(currentLoadedDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("�浵δ����");
                return false;
            }
            else
            {
                SaveData(autoSaveInfo);
                playerData = null;
                SetCurrentLoadedDataIndex(-1);
                StopAutoSaveTimer();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnUnloadData);
                return true;
            }
        }

        /// <summary>
        /// ɾ���浵(����ɾ����ǰ�������еĴ浵)
        /// </summary>
        /// <param name="i">��ɾ���Ĵ浵���</param>
        internal bool DeleteData(int i)
        {
            if(i == currentLoadedDataIndex)
            {
                FrameworkManager.Debugger.LogError("��ֹɾ����ǰ�������еĴ浵");
                return false;
            }
            string dataPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{i:000}.save");
            string dataMetaPath = Path.Combine(Application.dataPath, "SaveData", $"SaveData{i:000}.save.meta");
            string infoPath = Path.Combine(Application.dataPath, "SaveData", $"SaveDataInfo{i:000}.save");
            string infoMetaPath = Path.Combine(Application.dataPath, "SaveData", $"SaveDataInfo{i:000}.save.meta");
            if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(dataPath))
            {
                FrameworkManager.Debugger.LogWarning("��ɾ���Ĵ浵������");
                if (File.Exists(infoPath))
                {
                    File.Delete(infoPath);
                    if(File.Exists(infoMetaPath))
                        File.Delete(infoMetaPath);
                    infoDic.Remove(i);
                }
                return false;
            }
            else
            {
                File.Delete(dataPath);
                File.Delete(infoPath);
                if(File.Exists(dataMetaPath))
                    File.Delete(dataMetaPath);
                if(File.Exists(infoMetaPath))
                    File.Delete(infoMetaPath);
                infoDic.Remove(i);
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnDeleteData);
                if (i == defaultDataIndex)
                {
                    SetDefaultDataIndex(-1);
                    FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnDeleteCurrentData);
                }
                return true;
            }


        }
        /// <summary>
        /// ��ȡȫ���浵��Ϣ
        /// </summary>
        /// <returns></returns>
        internal List<PlayerDataInfo> GetDataInfos()
        {
            if(infoDic == null)
            {
                FrameworkManager.Debugger.LogError("Info�ֵ���δ��ʼ��");
                return null;
            }
            else
            {
                return infoDic.Values.ToList();
            }
        }

        #endregion

        #region ������������
        /// <summary>
        /// ������������
        /// </summary>
        private void SaveSetting()
        {
            string json = JsonUtility.ToJson(gameSettings, true);
            PlayerPrefs.SetString("Settings", json);
            PlayerPrefs.Save();
        }
        /// <summary>
        /// ��ȡ��������
        /// </summary>
        private void LoadSetting()
        {
            string json = PlayerPrefs.GetString("Settings", String.Empty);
            if (json.Equals(string.Empty))
            {
                gameSettings = ScriptableObject.CreateInstance<GameSettings>();
            }
            else
            {
                gameSettings = ScriptableObject.CreateInstance<GameSettings>();
                JsonUtility.FromJsonOverwrite(json, gameSettings);
            }
            
        }
        #endregion

        #region ���ô浵ע��

        /// <summary>
        /// ���ô浵ע��Ϊi��ע��
        /// �浵ע��Ϊ��浵һͬ�������Ϣ
        /// </summary>
        /// <param name="i">ע�ͱ��</param>
        internal void SetSaveInfo(int i)
        {
            if (saveInfoList == null)
            {
                FrameworkManager.Debugger.LogError("Info List is null");
            }
            else if (i < 0 || i >= saveInfoList.Count)
            {
                FrameworkManager.Debugger.LogError("the index of info List is out of range.");
            }
            else
            {
               autoSaveInfo = saveInfoList[i];
            }

        }

        /// <summary>
        /// ���ô浵ע������
        /// </summary>
        /// <param name="info">ע������</param>
        internal void SetSaveInfo(string info)
        {
            if (string.IsNullOrEmpty(info))
            {
                FrameworkManager.Debugger.LogError("Info can not be null or empty");
            }
            else
            {
                autoSaveInfo = info;
            }


        }

        #endregion


    }
}
