using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace StarryFramework
{
    internal class SaveManager : IManager, IConfigurableManager
    {

        private SaveSettings settings;
        private bool isInitialized;
        private const string SaveDataFolderName = "SaveData";

        // 当前游戏默认加载的存档编号，用于自动存档以及快速存档
        // 只在游戏开始前置初值为-1，在点击继续游戏按钮才会有效
        private int defaultDataIndex = -1;
        // 当前游戏已加载存档编号
        private int currentLoadedDataIndex = -1;

        private float autoSaveDataInterval;

        private float lastAutoSaveTime;

        // 启停自动存档标志位
        private bool startAutoSave;

        private string autoSaveInfo = "";

        private List<string> saveInfoList = new();

        private PlayerData playerData;

        private GameSettings gameSettings;

        private static readonly JsonSerializerSettings deserializeSettings = new JsonSerializerSettings
        {
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };



        internal int DefaultDataIndex => defaultDataIndex; 
        internal int CurrentLoadedDataIndex => currentLoadedDataIndex;
        internal float AutoSaveDataInterval =>autoSaveDataInterval;
        internal float LastAutoSaveTime => lastAutoSaveTime;
        internal bool AutoSave => startAutoSave;
        internal string AutoSaveInfo => autoSaveInfo;
        internal List<string> SaveInfoList => saveInfoList;
        internal PlayerData PlayerData => playerData;
        internal GameSettings GameSettings => gameSettings;
        internal bool PlayerDataLoaded => playerData != null;
        internal bool GameSettingsLoaded => gameSettings != null;


        // 目前全部的存档信息字典

        internal Dictionary<int, PlayerDataInfo> infoDic = new Dictionary<int, PlayerDataInfo>();

        internal static string GetSaveDataDirectoryPath()
        {
            // Save files are stored under Application.persistentDataPath only.
            return Path.Combine(Application.persistentDataPath, SaveDataFolderName);
        }

        private static string GetDataFilePath(int index)
        {
            return Path.Combine(GetSaveDataDirectoryPath(), $"SaveData{index:000}.save");
        }

        private static string GetDataMetaFilePath(int index)
        {
            return GetDataFilePath(index) + ".meta";
        }

        private static string GetInfoFilePath(int index)
        {
            return Path.Combine(GetSaveDataDirectoryPath(), $"SaveDataInfo{index:000}.save");
        }

        private static string GetInfoMetaFilePath(int index)
        {
            return GetInfoFilePath(index) + ".meta";
        }

        private static string GetCorruptedFilePath(string fileName)
        {
            return Path.Combine(GetSaveDataDirectoryPath(), $"Corrupted{fileName}");
        }

        private static string GetUniqueFilePath(string preferredPath)
        {
            if (!File.Exists(preferredPath))
            {
                return preferredPath;
            }

            string directory = Path.GetDirectoryName(preferredPath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(preferredPath);
            string extension = Path.GetExtension(preferredPath);
            int suffix = 1;

            string candidate;
            do
            {
                candidate = Path.Combine(directory ?? GetSaveDataDirectoryPath(), $"{fileNameWithoutExtension}_{suffix}{extension}");
                suffix++;
            } while (File.Exists(candidate));

            return candidate;
        }

        private static void MoveExistingFileToCorrupted(string sourceFilePath)
        {
            if (!File.Exists(sourceFilePath)) return;

            Directory.CreateDirectory(GetSaveDataDirectoryPath());
            string targetFilePath = GetUniqueFilePath(GetCorruptedFilePath(Path.GetFileName(sourceFilePath)));
            File.Move(sourceFilePath, targetFilePath);
        }


        void IManager.Awake()
        {
            InitInfoDic();
            LoadSetting();
            InitCurrentDataIndex();
        }
        void IManager.Init()
        {
            ApplySettings();
            isInitialized = true;
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
            isInitialized = false;
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

        void IConfigurableManager.SetSettings(IManagerSettings settings)
        {
            this.settings = settings as SaveSettings;
            if (isInitialized)
            {
                ApplySettings();
            }
        }

        #region 内部方法

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

        private void ApplySettings()
        {
            if (settings == null)
            {
                FrameworkManager.Debugger.LogError("SaveSettings is null.");
                return;
            }

            string oldDefaultInfo = saveInfoList != null && saveInfoList.Count > 0 ? saveInfoList[0] : "";
            string oldAutoSaveInfo = autoSaveInfo;

            saveInfoList ??= new List<string>();
            saveInfoList.Clear();

            if (settings.SaveInfoList == null)
            {
                FrameworkManager.Debugger.LogError("Info List can not be null.");
            }
            else
            {
                settings.SaveInfoList.ForEach(i => saveInfoList.Add(i));
            }

            string newDefaultInfo = saveInfoList.Count > 0 ? saveInfoList[0] : "";
            if (string.IsNullOrEmpty(oldAutoSaveInfo) || oldAutoSaveInfo == oldDefaultInfo)
            {
                autoSaveInfo = newDefaultInfo;
            }

            autoSaveDataInterval = settings.AutoSaveDataInterval;

            if (!settings.AutoSave)
            {
                StopAutoSaveTimer();
            }
            else if (currentLoadedDataIndex != -1 && !startAutoSave)
            {
                StartAutoSaveTimer();
            }
        }

        #region 存档信息管理

        private void InitInfoDic()
        {
            string path = GetSaveDataDirectoryPath();

            if (!Directory.Exists(path)) return;

            foreach (string filePath in Directory.EnumerateFiles(path, "SaveDataInfo*.save"))
            {
                try
                {
                    string js = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                    PlayerDataInfo info = JsonConvert.DeserializeObject<PlayerDataInfo>(js, deserializeSettings);
                    infoDic.Add(info.index, info);
                }
                catch
                {
                    FrameworkManager.Debugger.LogWarning("存档信息损坏");
                    MoveExistingFileToCorrupted(filePath);
                    string metaFilePath = filePath + ".meta";
                    if (File.Exists(metaFilePath))
                    {
                        MoveExistingFileToCorrupted(metaFilePath);
                    }
                }
            }

        }

        private PlayerDataInfo UpdateInfo(int index, string note)
        {
            if (infoDic == null)
            {
                FrameworkManager.Debugger.LogError("Info字典尚未初始化");
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
                FrameworkManager.Debugger.LogError("Info字典尚未初始化");
                return null;
            }
            if (infoDic.TryGetValue(index, out var info))
            {
                return info;
            }
            else
            {
                FrameworkManager.Debugger.LogError("存档信息不存在");
                return null;
            }
        }

        #endregion

        #region 游戏存档编号管理

        /// <summary>
        /// [设置当前已加载的游戏编号]
        /// 只在如下情况下会发生改变：
        /// 1 开启新游戏时，变量被设置为新游戏的存档编号
        /// 2 加载存档时，变量被设置为已加载的存档编号
        /// 3 卸载当前存档时，变量被重置为-1
        /// </summary>
        private void SetCurrentLoadedDataIndex(int index)
        {
            if (index < -1 || index >= 1000)
            {
                FrameworkManager.Debugger.LogError("存档编号超出范围");
            }
            else
                currentLoadedDataIndex = index;
        }

        /// <summary>
        /// [设置默认加载的游戏编号]
        /// 只在如下情况下会发生改变：
        /// 1 启动游戏时，系统读取上次的游戏编号
        /// 2 开启新游戏时，变量被设置为新游戏的存档编号
        /// 3 手动加载存档时，变量被设置为加载的存档编号
        /// 4 删除默认存档时，变量被重置为-1
        /// </summary>
        private void SetDefaultDataIndex(int index)
        {
            if(index < -1 || index >= 1000)
            {
                FrameworkManager.Debugger.LogError("存档编号超出范围");
            }
            else
                defaultDataIndex= index;
        }
        private void InitCurrentDataIndex()
        {
            // 启动游戏时，系统读取上次的游戏编号
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
            FrameworkManager.Debugger.LogError("存档编号已满");
            return -1;
        }

        #endregion

        #endregion

        #region 自动存档计时启停
        internal void StartAutoSaveTimer()
        {
            if(currentLoadedDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("存档未加载");
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
        /// <param name="isNewGame">是否是新游戏</param>
        /// <param name="note">存档信息</param>
        internal void CreateNewData(bool isNewGame, string note = "")
        {

            int newIndex = GetNewSaveIndex();
            if(newIndex==-1)
            {
                FrameworkManager.Debugger.LogError("创建新存档失败");
                return;
            }
            if(isNewGame)
            {
                playerData = new PlayerData();
                SetDefaultDataIndex(newIndex);
                SetCurrentLoadedDataIndex(newIndex);
                autoSaveInfo = saveInfoList.Count > 0 ? saveInfoList[0] : "";
            }

            Directory.CreateDirectory(GetSaveDataDirectoryPath());
            var dataPath = GetDataFilePath(newIndex);
            var infoPath = GetInfoFilePath(newIndex);
            var dataJs = JsonConvert.SerializeObject(playerData, Formatting.Indented);
            var infoJs = JsonConvert.SerializeObject(note != "" ? UpdateInfo(newIndex, note) : UpdateInfo(newIndex, autoSaveInfo), Formatting.Indented);
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
        /// 保存存档，快速存档或自动存档
        /// </summary>
        internal void SaveData(string note = "")
        {
            if (currentLoadedDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("存档尚未加载");
                return;
            }
            Directory.CreateDirectory(GetSaveDataDirectoryPath());
            var dataPath = GetDataFilePath(currentLoadedDataIndex);
            var infoPath = GetInfoFilePath(currentLoadedDataIndex);
            var dataJs = JsonConvert.SerializeObject(playerData, Formatting.Indented);
            var infoJs = JsonConvert.SerializeObject(note != "" ? UpdateInfo(currentLoadedDataIndex, note) : UpdateInfo(currentLoadedDataIndex, autoSaveInfo), Formatting.Indented);
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }

        /// <summary>
        /// 保存存档，编号i，手动选择
        /// </summary>
        /// <param name="i">保存存档的编号</param>
        /// <param name="note">存档注释</param>
        internal void SaveData(int i, string note = "")
        {
            if (i >= 1000 || i < 0)
            {
                FrameworkManager.Debugger.LogError("存档编号不合法(0-999)");
                return;
            }
            if (currentLoadedDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("存档尚未加载");
                return;
            }
            Directory.CreateDirectory(GetSaveDataDirectoryPath());
            var dataPath = GetDataFilePath(i);
            var infoPath = GetInfoFilePath(i);
            var dataJs = JsonConvert.SerializeObject(playerData, Formatting.Indented);
            var infoJs = JsonConvert.SerializeObject(note != "" ? UpdateInfo(i, note) : UpdateInfo(i, autoSaveInfo), Formatting.Indented);
            File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
            File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnSaveData);
        }
        /// <summary>
        /// 读取当前存档
        /// </summary>
        internal bool LoadData()
        {
            if(defaultDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("当前存档已被删除");
                return false;
            }
            string dataPath = GetDataFilePath(defaultDataIndex);
            string dataMetaPath = GetDataMetaFilePath(defaultDataIndex);
            if (!Directory.Exists(GetSaveDataDirectoryPath()) || !File.Exists(dataPath)) return false;
            try
            {
                var js = File.ReadAllText(dataPath, System.Text.Encoding.UTF8);
                playerData = JsonConvert.DeserializeObject<PlayerData>(js, deserializeSettings);
            }
            catch
            {
                FrameworkManager.Debugger.LogError("存档损坏");
                MoveExistingFileToCorrupted(dataPath);
                if (File.Exists(dataMetaPath))
                {
                    MoveExistingFileToCorrupted(dataMetaPath);
                }
                return false;
            }
            SetCurrentLoadedDataIndex(defaultDataIndex);
            if (settings.AutoSave) StartAutoSaveTimer();
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            return true;
        }
        /// <summary>
        /// 读取当前存档信息
        /// </summary>
        internal PlayerDataInfo LoadDataInfo()
        {
            if (defaultDataIndex == -1)
            {
                FrameworkManager.Debugger.LogError("当前存档信息已被删除");
                return null;
            }
            return GetInfo(defaultDataIndex);
        }

        /// <summary>
        /// 手动读取，编号为i的存档
        /// </summary>
        /// <param name="i">存档编号</param>
        internal bool LoadData(int i)
        {
            string dataPath = GetDataFilePath(i);
            string dataMetaPath = GetDataMetaFilePath(i);
            if (!Directory.Exists(GetSaveDataDirectoryPath()) || !File.Exists(dataPath))
            {
                FrameworkManager.Debugger.LogError("存档不存在");
                return false;
            }
            try
            {
                string js = File.ReadAllText(dataPath, System.Text.Encoding.UTF8);
                playerData = JsonConvert.DeserializeObject<PlayerData>(js, deserializeSettings);
            }
            catch
            {
                FrameworkManager.Debugger.LogError("存档损坏");
                MoveExistingFileToCorrupted(dataPath);
                if (File.Exists(dataMetaPath))
                {
                    MoveExistingFileToCorrupted(dataMetaPath);
                }
                return false;
            }
            SetDefaultDataIndex(i);
            SetCurrentLoadedDataIndex(i);
            if (settings.AutoSave) StartAutoSaveTimer();
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.OnLoadData);
            return true;
        }
        /// <summary>
        /// 读取编号为i的存档信息
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
                FrameworkManager.Debugger.LogError("存档未加载");
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
        /// 删除存档(禁止删除当前正在运行的存档)
        /// </summary>
        /// <param name="i">被删除的存档编号</param>
        internal bool DeleteData(int i)
        {
            if(i == currentLoadedDataIndex)
            {
                FrameworkManager.Debugger.LogError("禁止删除当前正在运行的存档");
                return false;
            }
            string dataPath = GetDataFilePath(i);
            string dataMetaPath = GetDataMetaFilePath(i);
            string infoPath = GetInfoFilePath(i);
            string infoMetaPath = GetInfoMetaFilePath(i);
            if (!Directory.Exists(GetSaveDataDirectoryPath()) || !File.Exists(dataPath))
            {
                FrameworkManager.Debugger.LogWarning("被删除的存档不存在");
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
        /// 获取全部存档信息
        /// </summary>
        /// <returns></returns>
        internal List<PlayerDataInfo> GetDataInfos()
        {
            if(infoDic == null)
            {
                FrameworkManager.Debugger.LogError("Info字典尚未初始化");
                return null;
            }
            else
            {
                return infoDic.Values.ToList();
            }
        }

        #endregion

        #region 游戏设置管理
        /// <summary>
        /// 保存游戏设置
        /// </summary>
        private void SaveSetting()
        {
            string json = JsonConvert.SerializeObject(gameSettings, Formatting.Indented);
            PlayerPrefs.SetString("Settings", json);
            PlayerPrefs.Save();
        }
        /// <summary>
        /// 读取游戏设置
        /// </summary>
        private void LoadSetting()
        {
            string json = PlayerPrefs.GetString("Settings", string.Empty);
            if (json.Equals(string.Empty))
            {
                gameSettings = new GameSettings();
            }
            else
            {
                gameSettings = JsonConvert.DeserializeObject<GameSettings>(json, deserializeSettings);
            }
            
        }
        #endregion

        #region 设置存档注释

        /// <summary>
        /// 设置存档注释为i号注释
        /// 存档注释为与存档一同保存的信息
        /// </summary>
        /// <param name="i">注释编号</param>
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
        /// 设置存档注释字符串
        /// </summary>
        /// <param name="info">注释字符串</param>
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
