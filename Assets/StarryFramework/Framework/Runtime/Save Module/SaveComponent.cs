using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class SaveComponent: BaseComponent
    {
        private SaveManager _manager = null;
        private SaveManager manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = FrameworkManager.GetManager<SaveManager>();
                }
                return _manager;
            }
        }

        [SerializeField] private SaveSettings settings;

        private UnityAction OnLeaveMainGame;


        public int DefaultDataIndex => manager.DefaultDataIndex;
        public int CurrentLoadedDataIndex => manager.CurrentLoadedDataIndex;
        public float AutoSaveDataInterval => manager.AutoSaveDataInterval;
        public float LastAutoSaveTime => manager.LastAutoSaveTime;
        public bool AutoSave => manager.AutoSave;
        public string AutoSaveInfo => manager.AutoSaveInfo;
        public List<string> SaveInfoList => manager.SaveInfoList;
        public Dictionary<int, PlayerDataInfo> DataInfoDic => manager.infoDic;

        public GameSettings GameSettings
        {
            get
            {
                if (manager.GameSettings == null)
                {
                    FrameworkManager.Debugger.LogError("游戏设置数据错误/游戏设置数据尚未加载");
                    return null;
                }
                else
                    return manager.GameSettings;
            }
        }
        public PlayerData PlayerData
        { 
            get 
            { 
                if(manager.PlayerData == null)
                {
                    FrameworkManager.Debugger.LogError("存档数据尚未加载");
                    return null;
                }
                else
                    return manager.PlayerData; 
            } 
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(EditorApplication.isPlaying && _manager != null )
                (_manager as IManager).SetSettings(settings);
        }
#endif


        protected override void Awake()
        {
            base.Awake();
            if (_manager == null)
            {
                _manager = FrameworkManager.GetManager<SaveManager>();
            }

            OnLeaveMainGame = new UnityAction(() => { UnloadData();});
            
            (_manager as IManager).SetSettings(settings);
        }
        private void Start()
        {
            FrameworkManager.EventManager.AddEventListener(FrameworkEvent.OnLeaveMainGame, OnLeaveMainGame);
        }
        internal override void Shutdown()
        {
            FrameworkManager.EventManager?.RemoveEventListener(FrameworkEvent.OnLeaveMainGame, OnLeaveMainGame);
        }


        #region 设置存档注释

        /// <summary>
        /// 设置存档注释为i号注释
        /// 存档注释为与存档一同保存的信息
        /// </summary>
        /// <param Name="i"></param>
        public void SetSaveInfo(int i)
        {
            manager.SetSaveInfo(i);
        }

        /// <summary>
        /// 设置存档注释为i号注释
        /// 存档注释为与存档一同保存的信息
        /// </summary>
        /// <param Name="info"></param>
        public void SetSaveInfo(string info)
        {
            manager.SetSaveInfo(info);
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
            manager.CreateNewData(isNewGame, note);
        }
        /// <summary>
        /// 储存存档,快速存档和自动存档
        /// </summary>
        public void SaveData(string note = "")
        {
            manager.SaveData(note);
        }
        /// <summary>
        /// 储存存档到编号i，手动选择
        /// </summary>
        /// <param Name="i">储存存档的编号</param>
        public void SaveData(int i, string note = "")
        {
            manager.SaveData(i, note);

        }
        /// <summary>
        /// 自动读取存档
        /// </summary>
        public bool LoadData()
        {
            return manager.LoadData();
        }
        /// <summary>
        /// 读取存档信息
        /// </summary>
        public PlayerDataInfo LoadDataInfo()
        {
            return manager.LoadDataInfo();
        }
        /// <summary>
        /// 手动读取编号为i的存档
        /// </summary>
        /// <param Name="i">存档编号</param>
        public bool LoadData(int i)
        {
            return manager.LoadData(i);
        }
        /// <summary>
        /// 获取编号为i的存档信息
        /// </summary>
        /// <returns>编号为i的存档信息</returns>
        public PlayerDataInfo LoadDataInfo(int i)
        {
            return manager.LoadDataInfo(i);
        }
        /// <summary>
        /// 卸载当前已加载的存档
        /// </summary>
        /// <returns></returns>
        public bool UnloadData()
        {
            return manager.UnloadData();
        }
        /// <summary>
        /// 删除存档
        /// </summary>
        /// <param Name="i">删除的存档编号</param>
        public bool DeleteData(int i)
        {
            return manager.DeleteData(i);
        }
        /// <summary>
        /// 获取全部存档信息
        /// </summary>
        /// <returns></returns>
        public List<PlayerDataInfo> GetDataInfos()
        {
            return manager.GetDataInfos();
        }


        #endregion

        #region 自动存档计时器启停

        /// <summary>
        /// 启动自动存档
        /// </summary>
        public void StartAutoSaveTimer()
        {
            manager.StartAutoSaveTimer();
        }

        /// <summary>
        /// 关闭自动存档
        /// </summary>
        public void StopAutoSaveTimer()
        {
            manager.StopAutoSaveTimer();
        }

        #endregion

    }
}
