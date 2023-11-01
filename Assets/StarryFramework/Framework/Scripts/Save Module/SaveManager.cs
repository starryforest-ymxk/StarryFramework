using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using UnityEngine.Events;

namespace StarryFramework
{
    internal class SaveManager : IManager
    {

        //当前游戏默认加载的存档编号，用于自动存档以及快速存档
        //只有游戏开始前其数值不为-1，“继续游戏”按钮才会生效
        private int defaultDataIndex = -1;
        //当前游戏已加载存档编号
        private int currentLoadedDataIndex = -1;

        private float autoSaveDataInterval;

        private float lastAutoSaveTime;

        //启停自动存档标志位
        private bool startAutoSave = false;

        private string autoSaveInfo = "";

        private List<string> saveInfoList = new List<string>();

        private PlayerData playerData = null;

        private GameSettings gameSettings = null;



        internal int DefaultDataIndex => defaultDataIndex; 
        internal int CurrentLoadedDataIndex => currentLoadedDataIndex;
        internal float AutoSaveDataInterval =>autoSaveDataInterval;
        internal float LastAutoSaveTime => lastAutoSaveTime;
        internal bool AutoSave => startAutoSave;
        internal string AutoSaveInfo => autoSaveInfo;
        internal List<string> SaveInfoList => saveInfoList;
        internal PlayerData PlayerData => playerData;
        internal GameSettings GameSettings => gameSettings;


        //目前全部的存档信息词典

        internal Dictionary<int, PlayerDataInfo> infoDic = new Dictionary<int, PlayerDataInfo>();


        void IManager.Awake()
        {
            InitInfoDic();
            LoadSetting();
            InitCurrentDataIndex();
        }
        void IManager.Init()
        {
            SetInfoList(FrameworkManager.setting.SaveSettings.SaveInfoList);
            autoSaveDataInterval = FrameworkManager.setting.SaveSettings.AutoSaveDataInterval;
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

        #region 内部操作

        private void SetInfoList(List<string> infos)
        {
            if (infos == null)
                Debug.LogError("Info List can not be null.");
            else
            {
                infos.ForEach(i => saveInfoList.Add(i));
                if (infos.Count != 0)
                {
                    autoSaveInfo = infos[0];
                }

            }
        }

        #region 存档信息管理

        private void InitInfoDic()
        {
            string path = Path.Combine(Application.dataPath, "SaveData");

            if (!Directory.Exists(path)) return;

            foreach (string filePath in Directory.EnumerateFiles(path, "SaveDataInfo*.save"))
            {
                try
                {
                    string js = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    PlayerDataInfo info = JsonConvert.DeserializeObject<PlayerDataInfo>(js);
                    infoDic.Add(info.index, info);
                }
                catch
                {
                    Debug.LogWarning("存档信息损坏");
                    File.Move(filePath, Path.Combine(Application.dataPath, "SaveData", $"Corrupted{Path.GetFileName(filePath)}"));
                    File.Delete(filePath);
                }
            }

        }

        private PlayerDataInfo UpdateInfo(int index, string note)
        {
            if (infoDic == null)
            {
                Debug.LogError("Info字典尚未初始化");
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
                Debug.LogError("Info字典尚未初始化");
                return null;
            }
            if (infoDic.ContainsKey(index))
            {
                return infoDic[index];
            }
            else
            {
                Debug.LogError("存档信息不存在");
                return null;
            }
        }

        #endregion

        #region 游戏存档编号管理

        /// <summary>
        /// [设置当前已加载的游戏编号]
        /// 只有三种情况下会发生改变：
        /// 1 创建新游戏时，此编号变为新游戏的存档编号
        /// 2 加载存档时，此编号变为已加载的存档编号
        /// 3 卸载当前存档时，此编号重置为-1
        /// </summary>
        /// <param Name="index"></param>
        private void SetCurrentLoadedDataIndex(int index)
        {
            if (index < -1 || index >= 1000)
            {
                Debug.LogError("存档编号超出范围");
            }
            else
                currentLoadedDataIndex = index;
        }

        /// <summary>
        /// [设置默认加载的游戏编号]
        /// 只有四种情况下会发生改变：
        /// 1 打开游戏时，系统读取上次的游戏编号
        /// 2 创建新游戏时，此编号变为新游戏的存档编号
        /// 3 按照序号加载存档时，此编号变为加载的存档编号
        /// 4 删除默认存档时，此编号重置为-1
        /// </summary>
        /// <param Name="index"></param>
        private void SetDefaultDataIndex(int index)
        {
            if(index < -1 || index >= 1000)
            {
                Debug.LogError("存档编号超出范围");
            }
            else
                defaultDataIndex= index;
        }
        private void InitCurrentDataIndex()
        {
            //打开游戏时，系统读取上次的游戏编号
            defaultDataIndex = PlayerPrefs.GetInt("DefaultDataIndex", -1);
        }
        private void SaveCurrentDataIndex()
        {
            PlayerPrefs.SetInt("DefaultDataIndex", defaultDataIndex);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 获取新的存档编号，成功返回0-999，失败返回-1
        /// </summary>
        /// <returns>返回的编号</returns>
        private int GetNewSaveIndex()
        {
            for (int i = 0; i < 1000; i++)
            {
                if (!infoDic.ContainsKey(i))
                {
                    return i;
                }
            }
            Debug.LogError("存档容量已满");
            return -1;
        }

        #endregion

        #endregion

        #region 自动存档计时器启停
        internal void StartAutoSaveTimer()
        {
            if(currentLoadedDataIndex == -1)
            {
                Debug.LogError("存档未加载");
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

        #region 存档操作
        /// <summary>
        /// 创建新存档
        /// </summary>
        /// <param Name="isNewGame">是否是新游戏</param>
        /// <param Name="note">存档信息</param>
        internal void CreateNewData(bool isNewGame, string note = "")
        {

            int newIndex = GetNewSaveIndex();
            if(newIndex==-1)
            {
                Debug.LogError("创建新存档失败");
                return;
            }
            if(isNewGame)
            {
                playerData = new PlayerData();
                SetDefaultDataIndex(newIndex); //创建新游戏时，此编号变为新游戏的存档编号
                SetCurrentLoadedDataIndex(newIndex);
                if (saveInfoList.Count > 0)
                    autoSaveInfo = saveInfoList[0];
                else
                    autoSaveInfo = "";
            }

            Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
            string dataPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.save", newIndex));
            string infoPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveDataInfo{0:000}.save", newIndex));
            string dataJs = JsonConvert.SerializeObject(playerData);
            string infoJs;
            if (note != "")
                infoJs = JsonConvert.SerializeObject(UpdateInfo(newIndex, note));
            else
                infoJs = JsonConvert.SerializeObject(UpdateInfo(newIndex, autoSaveInfo));
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            if (isNewGame)
            {
                if (FrameworkManager.setting.SaveSettings.AutoSave) StartAutoSaveTimer();
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            }
            else
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }
        /// <summary>
        /// 储存存档,快速存档和自动存档
        /// </summary>
        internal void SaveData(string note = "")
        {
            if (currentLoadedDataIndex == -1)
            {
                Debug.LogError("存档尚未加载");
                return;
            }
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
            string dataPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.save", currentLoadedDataIndex));
            string infoPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveDataInfo{0:000}.save", currentLoadedDataIndex));
            string dataJs = JsonConvert.SerializeObject(playerData);
            string infoJs;
            if (note != "")
                infoJs = JsonConvert.SerializeObject(UpdateInfo(currentLoadedDataIndex, note));
            else
                infoJs = JsonConvert.SerializeObject(UpdateInfo(currentLoadedDataIndex, autoSaveInfo));
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }
        /// <summary>
        /// 储存存档到编号i，手动选择
        /// </summary>
        /// <param Name="i">储存存档的编号</param>
        internal void SaveData(int i, string note = "")
        {
            if (i >= 1000 || i < 0)
            {
                Debug.LogError("存档序号不合法(0-999)");
                return;
            }
            if (currentLoadedDataIndex == -1)
            {
                Debug.LogError("存档尚未加载");
                return;
            }
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
            string dataPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.save", i));
            string infoPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveDataInfo{0:000}.save", i));
            string dataJs = JsonConvert.SerializeObject(playerData);
            string infoJs;
            if (note != "")
                infoJs = JsonConvert.SerializeObject(UpdateInfo(i, note));
            else
                infoJs = JsonConvert.SerializeObject(UpdateInfo(i, autoSaveInfo));
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }
        /// <summary>
        /// 读取当前存档
        /// </summary>
        internal bool LoadData()
        {
            if(defaultDataIndex == -1)
            {
                Debug.LogError("当前存档已被删除");
                return false;
            }
            string datapath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.save", defaultDataIndex));
            if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(datapath)) return false;
            try
            {
                string js = File.ReadAllText(datapath, System.Text.Encoding.UTF8);
                playerData = JsonConvert.DeserializeObject<PlayerData>(js);
            }
            catch
            {
                Debug.LogError("存档损坏");
                File.Move(datapath, Path.Combine(Application.dataPath, "SaveData", string.Format("CorruptedSaveData{0:000}.save", defaultDataIndex)));
                File.Delete(datapath);
                return false;
            }
            SetCurrentLoadedDataIndex(defaultDataIndex);
            if (FrameworkManager.setting.SaveSettings.AutoSave) StartAutoSaveTimer();
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            return true;
        }
        /// <summary>
        /// 读取当前存档信息
        /// </summary>
        internal PlayerDataInfo LoadDataInfo()
        {
            if (defaultDataIndex == -1)
            {
                Debug.LogError("当前存档信息已被删除");
                return null;
            }
            return GetInfo(defaultDataIndex);
        }
        /// <summary>
        /// 手动读取编号为i的存档
        /// </summary>
        /// <param Name="i">存档编号</param>
        internal bool LoadData(int i)
        {
            string datapath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.save", i));
            if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(datapath))
            {
                Debug.LogError("存档不存在");
                return false;
            }
            try
            {
                string js = File.ReadAllText(datapath, System.Text.Encoding.UTF8);
                playerData = JsonConvert.DeserializeObject<PlayerData>(js);
            }
            catch
            {
                Debug.LogError("存档损坏");
                File.Move(datapath, Path.Combine(Application.dataPath, "SaveData", string.Format("CorruptedSaveData{0:000}.save", i)));
                File.Delete(datapath);
                return false;
            }
            SetDefaultDataIndex(i);// 读取存档时，此编号变为读取的存档编号
            SetCurrentLoadedDataIndex(i);
            if (FrameworkManager.setting.SaveSettings.AutoSave) StartAutoSaveTimer();
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            return true;
        }
        /// <summary>
        /// 获取编号为i的存档信息
        /// </summary>
        /// <returns>编号为i的存档信息</returns>
        internal PlayerDataInfo LoadDataInfo(int i)
        {
            return GetInfo(i);
        }
        /// <summary>
        /// 卸载存档，在退出主游戏时调用
        /// </summary>
        internal bool UnloadData()
        {
            if(currentLoadedDataIndex == -1)
            {
                Debug.LogError("存档未加载");
                return false;
            }
            else
            {
                SaveData(autoSaveInfo);
                playerData = null;
                SetCurrentLoadedDataIndex(-1);
                StopAutoSaveTimer();
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnUnloadData);
                return true;
            }
        }
        /// <summary>
        /// 删除存档(不可删除当前正在运行的存档)
        /// </summary>
        /// <param Name="i">删除的存档编号</param>
        internal bool DeleteData(int i)
        {
            if(i == currentLoadedDataIndex)
            {
                Debug.LogError("禁止删除当前正在运行的存档");
                return false;
            }
            string datapath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.save", i));
            string infopath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveDataInfo{0:000}.save", i));
            if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(datapath))
            {
                Debug.LogWarning("待删除的存档不存在");
                if (File.Exists(infopath))
                {
                    File.Delete(infopath);
                    infoDic.Remove(i);
                }
                return false;
            }
            else
            {
                File.Delete(datapath);
                File.Delete(infopath);
                infoDic.Remove(i);
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnDeleteData);
                if (i == defaultDataIndex)
                {
                    SetDefaultDataIndex(-1);
                    FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnDeleteCurrentData);
                }
                return true;
            }


        }
        /// <summary>
        /// 获取全部存档信息
        /// </summary>
        /// <returns></returns>
        internal List<PlayerDataInfo> GetDataInfos()
        {
            if(infoDic == null)
            {
                Debug.LogError("Info字典尚未初始化");
                return null;
            }
            else
            {
                return infoDic.Values.ToList();
            }
        }

        #endregion

        #region 读存设置数据
        /// <summary>
        /// 保存设置数据
        /// </summary>
        private void SaveSetting()
        {
            string json = JsonConvert.SerializeObject(gameSettings);
            PlayerPrefs.SetString("Settings", json);
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 读取设置数据
        /// </summary>
        private void LoadSetting()
        {
            string json = PlayerPrefs.GetString("Settings", String.Empty);
            if (json.Equals(string.Empty))
            {
                gameSettings = new GameSettings();
            }
            else
            {
                gameSettings = JsonConvert.DeserializeObject<GameSettings>(json);
            }
        }
        #endregion

        #region 设置存档注释

        /// <summary>
        /// 设置存档注释为i号注释
        /// 存档注释为与存档一同保存的信息
        /// </summary>
        /// <param Name="i"></param>
        internal void SetSaveInfo(int i)
        {
            if (saveInfoList == null)
            {
                Debug.LogError("Info List is null");
            }
            else if (i < 0 || i >= saveInfoList.Count)
            {
                Debug.LogError("the index of info List is out of range.");
            }
            else
            {
               autoSaveInfo = saveInfoList[i];
            }

        }

        /// <summary>
        /// 设置存档注释为i号注释
        /// 存档注释为与存档一同保存的信息
        /// </summary>
        /// <param Name="info"></param>
        internal void SetSaveInfo(string info)
        {
            if (info == null || info == "")
            {
                Debug.LogError("Info can not be null or empty");
            }
            else
            {
                autoSaveInfo = info;
            }


        }

        #endregion


    }
}
