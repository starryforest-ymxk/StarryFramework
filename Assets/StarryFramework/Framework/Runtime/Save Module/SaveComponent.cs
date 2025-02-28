using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class SaveComponent: BaseComponent
    {
        private SaveManager _manager;
        private SaveManager Manager => _manager ??= FrameworkManager.GetManager<SaveManager>();

        [SerializeField] private SaveSettings settings;

        private UnityAction onLeaveMainGame;


        public int DefaultDataIndex => Manager.DefaultDataIndex;
        public int CurrentLoadedDataIndex => Manager.CurrentLoadedDataIndex;
        public float AutoSaveDataInterval => Manager.AutoSaveDataInterval;
        public float LastAutoSaveTime => Manager.LastAutoSaveTime;
        public bool AutoSave => Manager.AutoSave;
        public string AutoSaveInfo => Manager.AutoSaveInfo;
        public List<string> SaveInfoList => Manager.SaveInfoList;
        public Dictionary<int, PlayerDataInfo> DataInfoDic => Manager.infoDic;

        public GameSettings GameSettings
        {
            get
            {
                if (Manager.GameSettings == null)
                {
                    FrameworkManager.Debugger.LogError("��Ϸ�������ݴ���/��Ϸ����������δ����");
                    return null;
                }

                return Manager.GameSettings;
            }
        }
        public PlayerData PlayerData
        { 
            get
            {
                if(Manager.PlayerData == null)
                {
                    FrameworkManager.Debugger.LogError("�浵������δ����");
                    return null;
                }

                return Manager.PlayerData;
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
            _manager ??= FrameworkManager.GetManager<SaveManager>();
            (_manager as IManager).SetSettings(settings);
            
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


        #region ���ô浵ע��

        /// <summary>
        /// ���ô浵ע��Ϊi��ע��
        /// �浵ע��Ϊ��浵һͬ�������Ϣ
        /// </summary>
        /// <param Name="i"></param>
        public void SetSaveInfo(int i)
        {
            Manager.SetSaveInfo(i);
        }

        /// <summary>
        /// ���ô浵ע��Ϊi��ע��
        /// �浵ע��Ϊ��浵һͬ�������Ϣ
        /// </summary>
        /// <param Name="info"></param>
        public void SetSaveInfo(string info)
        {
            Manager.SetSaveInfo(info);
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
            Manager.CreateNewData(isNewGame, note);
        }
        /// <summary>
        /// ����浵,���ٴ浵���Զ��浵
        /// </summary>
        public void SaveData(string note = "")
        {
            Manager.SaveData(note);
        }
        /// <summary>
        /// ����浵�����i���ֶ�ѡ��
        /// </summary>
        /// <param Name="i">����浵�ı��</param>
        public void SaveData(int i, string note = "")
        {
            Manager.SaveData(i, note);

        }
        /// <summary>
        /// �Զ���ȡ�浵
        /// </summary>
        public bool LoadData()
        {
            return Manager.LoadData();
        }
        /// <summary>
        /// ��ȡ�浵��Ϣ
        /// </summary>
        public PlayerDataInfo LoadDataInfo()
        {
            return Manager.LoadDataInfo();
        }
        /// <summary>
        /// �ֶ���ȡ���Ϊi�Ĵ浵
        /// </summary>
        /// <param Name="i">�浵���</param>
        public bool LoadData(int i)
        {
            return Manager.LoadData(i);
        }
        /// <summary>
        /// ��ȡ���Ϊi�Ĵ浵��Ϣ
        /// </summary>
        /// <returns>���Ϊi�Ĵ浵��Ϣ</returns>
        public PlayerDataInfo LoadDataInfo(int i)
        {
            return Manager.LoadDataInfo(i);
        }
        /// <summary>
        /// ж�ص�ǰ�Ѽ��صĴ浵
        /// </summary>
        /// <returns></returns>
        public bool UnloadData()
        {
            return Manager.UnloadData();
        }
        /// <summary>
        /// ɾ���浵
        /// </summary>
        /// <param Name="i">ɾ���Ĵ浵���</param>
        public bool DeleteData(int i)
        {
            return Manager.DeleteData(i);
        }
        /// <summary>
        /// ��ȡȫ���浵��Ϣ
        /// </summary>
        /// <returns></returns>
        public List<PlayerDataInfo> GetDataInfos()
        {
            return Manager.GetDataInfos();
        }


        #endregion

        #region �Զ��浵��ʱ����ͣ

        /// <summary>
        /// �����Զ��浵
        /// </summary>
        public void StartAutoSaveTimer()
        {
            Manager.StartAutoSaveTimer();
        }

        /// <summary>
        /// �ر��Զ��浵
        /// </summary>
        public void StopAutoSaveTimer()
        {
            Manager.StopAutoSaveTimer();
        }

        #endregion

    }
}
