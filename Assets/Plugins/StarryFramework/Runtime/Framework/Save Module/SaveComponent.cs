using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class SaveComponent: ConfigurableComponent
    {
        private SaveManager _manager;
        private SaveManager Manager => _manager ??= FrameworkManager.GetManager<SaveManager>();

        [SerializeField] private SaveSettings settings = new();

        private UnityAction onLeaveMainGame;


        public int DefaultDataIndex => Manager.DefaultDataIndex;
        public int CurrentLoadedDataIndex => Manager.CurrentLoadedDataIndex;
        public float AutoSaveDataInterval => Manager.AutoSaveDataInterval;
        public float LastAutoSaveTime => Manager.LastAutoSaveTime;
        public bool AutoSave => Manager.AutoSave;
        public string AutoSaveInfo => Manager.AutoSaveInfo;
        public List<string> SaveInfoList => Manager.SaveInfoList;
        public Dictionary<int, PlayerDataInfo> DataInfoDic => Manager.infoDic;
        public bool PlayerDataLoaded => Manager.PlayerDataLoaded;
        public bool GameSettingsLoaded => Manager.GameSettingsLoaded;

        public object PlayerDataObject => Manager.PlayerDataObject;
        public object GameSettingsObject => Manager.GameSettingsObject;

        [Obsolete("兼容入口：将于后续大版本移除。请改用 GetGameSettings<T>() 或 GetGameSettingsObject().", false)]
        public GameSettings GameSettings
        {
            get
            {
                if (Manager.GameSettings == null)
                {
                    FrameworkManager.Debugger.LogError("当前数据提供器未使用内置 GameSettings 类型，请改用 GetGameSettings<T>() 或 GameSettingsObject。");
                    return null;
                }

                return Manager.GameSettings;
            }
        }
        [Obsolete("兼容入口：将于后续大版本移除。请改用 GetPlayerData<T>() 或 GetPlayerDataObject().", false)]
        public PlayerData PlayerData
        { 
            get
            {
                if(Manager.PlayerData == null)
                {
                    FrameworkManager.Debugger.LogError("当前数据提供器未使用内置 PlayerData 类型，请改用 GetPlayerData<T>() 或 PlayerDataObject。");
                    return null;
                }

                return Manager.PlayerData;
            } 
        }

        public T GetPlayerData<T>() where T : class
        {
            T data = Manager.PlayerDataObject as T;
            if (data == null)
            {
                FrameworkManager.Debugger.LogError($"玩家数据类型不匹配或未加载，期望类型: {typeof(T).FullName}");
            }

            return data;
        }

        public T GetGameSettings<T>() where T : class
        {
            T data = Manager.GameSettingsObject as T;
            if (data == null)
            {
                FrameworkManager.Debugger.LogError($"游戏设置类型不匹配或未加载，期望类型: {typeof(T).FullName}");
            }

            return data;
        }

        public object GetPlayerDataObject()
        {
            return Manager.PlayerDataObject;
        }

        public object GetGameSettingsObject()
        {
            return Manager.GameSettingsObject;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            SaveManager.ApplyEditorSaveDataPathOverride(settings);
            HotApplyConfigurableSettingsInPlayMode(_manager, ref settings);
        }
#endif


        protected override void Awake()
        {
            SaveManager.ApplyEditorSaveDataPathOverride(settings);
            base.Awake();
            ResolveAndApplyConfigurableSettings(ref _manager, ref settings, FrameworkManager.GetManager<SaveManager>);
            
            onLeaveMainGame = () => { UnloadData();};
        }
        private void Start()
        {
            FrameworkManager.EventManager.AddEventListener(FrameworkEvent.OnLeaveMainGame, onLeaveMainGame);
        }
        internal override void Shutdown()
        {
            FrameworkManager.EventManager?.RemoveEventListener(FrameworkEvent.OnLeaveMainGame, onLeaveMainGame);
        }


        #region 设置存档注释

        /// <summary>
        /// 设置存档注释为i号注释
        /// 存档注释为与存档一同保存的信息
        /// </summary>
        /// <param Name="i"></param>
        public void SetSaveInfo(int i)
        {
            Manager.SetSaveInfo(i);
        }

        /// <summary>
        /// 设置存档注释字符串
        /// 存档注释为与存档一同保存的信息
        /// </summary>
        /// <param Name="info"></param>
        public void SetSaveInfo(string info)
        {
            Manager.SetSaveInfo(info);
        }

        #endregion

        #region 存档操作
        /// <summary>
        /// 创建新存档
        /// </summary>
        /// <param Name="isNewGame">是否是新游戏</param>
        /// <param Name="note">存档信息</param>
        public void CreateNewData(bool isNewGame, string note = "")
        {
            Manager.CreateNewData(isNewGame, note);
        }
        /// <summary>
        /// 保存存档，快速存档或自动存档
        /// </summary>
        public void SaveData(string note = "")
        {
            Manager.SaveData(note);
        }
        /// <summary>
        /// 保存存档，编号i，手动选择
        /// </summary>
        /// <param Name="i">保存存档的编号</param>
        public void SaveData(int i, string note = "")
        {
            Manager.SaveData(i, note);

        }
        /// <summary>
        /// 自动读取存档
        /// </summary>
        public bool LoadData()
        {
            return Manager.LoadData();
        }
        /// <summary>
        /// 读取存档信息
        /// </summary>
        public PlayerDataInfo LoadDataInfo()
        {
            return Manager.LoadDataInfo();
        }
        /// <summary>
        /// 手动读取，编号为i的存档
        /// </summary>
        /// <param Name="i">存档编号</param>
        public bool LoadData(int i)
        {
            return Manager.LoadData(i);
        }
        /// <summary>
        /// 读取编号为i的存档信息
        /// </summary>
        /// <returns>编号为i的存档信息</returns>
        public PlayerDataInfo LoadDataInfo(int i)
        {
            return Manager.LoadDataInfo(i);
        }
        /// <summary>
        /// 卸载当前已加载的存档
        /// </summary>
        /// <returns></returns>
        public bool UnloadData()
        {
            return Manager.UnloadData();
        }
        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param Name="i">删除的存档编号</param>
        public bool DeleteData(int i)
        {
            return Manager.DeleteData(i);
        }
        /// <summary>
        /// 获取全部存档信息
        /// </summary>
        /// <returns></returns>
        public List<PlayerDataInfo> GetDataInfos()
        {
            return Manager.GetDataInfos();
        }


        #endregion

        #region 自动存档计时启停

        /// <summary>
        /// 开启自动存档
        /// </summary>
        public void StartAutoSaveTimer()
        {
            Manager.StartAutoSaveTimer();
        }

        /// <summary>
        /// 关闭自动存档
        /// </summary>
        public void StopAutoSaveTimer()
        {
            Manager.StopAutoSaveTimer();
        }

        #endregion

    }
    
}
