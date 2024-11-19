using System.Collections.Generic;
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
                    FrameworkManager.Debugger.LogError("��Ϸ�������ݴ���/��Ϸ����������δ����");
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
                    FrameworkManager.Debugger.LogError("�浵������δ����");
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


        #region ���ô浵ע��

        /// <summary>
        /// ���ô浵ע��Ϊi��ע��
        /// �浵ע��Ϊ��浵һͬ�������Ϣ
        /// </summary>
        /// <param Name="i"></param>
        public void SetSaveInfo(int i)
        {
            manager.SetSaveInfo(i);
        }

        /// <summary>
        /// ���ô浵ע��Ϊi��ע��
        /// �浵ע��Ϊ��浵һͬ�������Ϣ
        /// </summary>
        /// <param Name="info"></param>
        public void SetSaveInfo(string info)
        {
            manager.SetSaveInfo(info);
        }

        #endregion

        #region �浵����
        /// <summary>
        /// �����´浵
        /// </summary>
        /// <param Name="isNewGame">�Ƿ�������Ϸ</param>
        /// <param Name="note">�浵��Ϣ</param>
        public void CreateNewData(bool isNewGame, string note = "")
        {
            manager.CreateNewData(isNewGame, note);
        }
        /// <summary>
        /// ����浵,���ٴ浵���Զ��浵
        /// </summary>
        public void SaveData(string note = "")
        {
            manager.SaveData(note);
        }
        /// <summary>
        /// ����浵�����i���ֶ�ѡ��
        /// </summary>
        /// <param Name="i">����浵�ı��</param>
        public void SaveData(int i, string note = "")
        {
            manager.SaveData(i, note);

        }
        /// <summary>
        /// �Զ���ȡ�浵
        /// </summary>
        public bool LoadData()
        {
            return manager.LoadData();
        }
        /// <summary>
        /// ��ȡ�浵��Ϣ
        /// </summary>
        public PlayerDataInfo LoadDataInfo()
        {
            return manager.LoadDataInfo();
        }
        /// <summary>
        /// �ֶ���ȡ���Ϊi�Ĵ浵
        /// </summary>
        /// <param Name="i">�浵���</param>
        public bool LoadData(int i)
        {
            return manager.LoadData(i);
        }
        /// <summary>
        /// ��ȡ���Ϊi�Ĵ浵��Ϣ
        /// </summary>
        /// <returns>���Ϊi�Ĵ浵��Ϣ</returns>
        public PlayerDataInfo LoadDataInfo(int i)
        {
            return manager.LoadDataInfo(i);
        }
        /// <summary>
        /// ж�ص�ǰ�Ѽ��صĴ浵
        /// </summary>
        /// <returns></returns>
        public bool UnloadData()
        {
            return manager.UnloadData();
        }
        /// <summary>
        /// ɾ���浵
        /// </summary>
        /// <param Name="i">ɾ���Ĵ浵���</param>
        public bool DeleteData(int i)
        {
            return manager.DeleteData(i);
        }
        /// <summary>
        /// ��ȡȫ���浵��Ϣ
        /// </summary>
        /// <returns></returns>
        public List<PlayerDataInfo> GetDataInfos()
        {
            return manager.GetDataInfos();
        }


        #endregion

        #region �Զ��浵��ʱ����ͣ

        /// <summary>
        /// �����Զ��浵
        /// </summary>
        public void StartAutoSaveTimer()
        {
            manager.StartAutoSaveTimer();
        }

        /// <summary>
        /// �ر��Զ��浵
        /// </summary>
        public void StopAutoSaveTimer()
        {
            manager.StopAutoSaveTimer();
        }

        #endregion

    }
}
