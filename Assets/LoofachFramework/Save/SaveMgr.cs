using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
/// <summary>
/// 存档管理器，主要数据储存与读取
/// 各对象在start获取数据
/// 自动储存在自动储存时间和退出游戏的时候调用
/// </summary>
public sealed class SaveMgr : MonoSingleton<SaveMgr>
{
    /// <summary>
    /// 定时存档为了防止每次自动存档集中修改data导致卡顿，在每次更改数据都修改data
    /// 事件存档为了优化平时的流畅度，在存档时集中修改data;
    /// 不管什么模式，在事件存档时都进行BeforeEventSave事件；
    /// 所有数据都要注册一次BeforeEventSave事件，重要数据还要在直接修改的函数中添加储存模式判断（如果是定时储存则要在修改时修改Data）
    /// 1. 重要信息：在事件储存时利用事件修改data并储存，时间储存状态时即时修改data
    /// 2. 次要信息：在事件储存时利用事件修改data并储存，任何状态下的修改都不直接修改data
    /// </summary>    
    [Tooltip("自动储存间隔")] [SerializeField] private float autoSaveTime = 60 * 15;
    [Tooltip("自动储存方式")] [SerializeField] private Enums.SaveM saveMode;
    private float lastAutoSaveTime = 0;             //上次自动储存时间
    private bool isInMainGame = false;              //是否在主游戏内
    private int currentSaveIndex = -1;              //当前手动存档档位
    private bool isNewData = true;                  //当前存档是否使用全新数据(如果新数据则游戏内不读取data)
    private PlayerData data = new PlayerData();
    private Settings setting = new Settings();
    public int CurrentSaveIndex => currentSaveIndex;

    public bool IsNewData => isNewData;
    public Enums.SaveM SaveMode => saveMode;

    protected override void Awake()
    {
        base.Awake();                
        //LoadSetting();

        //中途关闭游戏进行自动存档
        //即便当前不是自动存档也储存在自动存档，因为玩家可能是主动舍弃当前当前存档未保存的内容
        Application.quitting += () =>
        {
            if (isInMainGame)
            {
                EventMgr.GetInstance().InvokeEvent(EventDic.BeforeEventSave);
                AutoSaveData();
            }
        };
    }    
    private void Update()
    {
        if (SaveMode == Enums.SaveM.timedSave && isInMainGame)
        {
            if (Time.time - lastAutoSaveTime > autoSaveTime) AutoSaveData();
        }
    }
    #region 事件
    private void OnEnable()
    {
        EventMgr.GetInstance().AddEventListener(EventDic.OnEnterMainGame, OnStartMainGame);
        EventMgr.GetInstance().AddEventListener(EventDic.OnLeaveMainGame, OnEndMainGame);
    }
    private void OnDisable()
    {
        EventMgr.GetInstance().DeleteEventListener(EventDic.OnEnterMainGame, OnStartMainGame);
        EventMgr.GetInstance().DeleteEventListener(EventDic.OnLeaveMainGame, OnEndMainGame);
    }
    private void OnStartMainGame()
    {
        isInMainGame = true;
        lastAutoSaveTime = Time.time;
        if (isNewData) data = new PlayerData();
    }
    private void OnEndMainGame()
    {
        isInMainGame = false;
        EventMgr.GetInstance().InvokeEvent(EventDic.BeforeEventSave);
        if (currentSaveIndex >= 0) SaveData();
        else AutoSaveData();
    }
    private void OnStartNewGame()
    {
        isNewData = true;
    }
    #endregion
    #region 数据存储
    #region 读存存档
    /// <summary>
    /// 储存自动存档
    /// </summary>
    public void AutoSaveData()
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
        string dataPath = Path.Combine(Application.dataPath, "SaveData", "AutoSaveData.loofah");
        string infoPath = Path.Combine(Application.dataPath, "SaveData", "AutoSaveDataInfo.loofah");
        string dataJs = JsonConvert.SerializeObject(data);
        string infoJs = JsonConvert.SerializeObject(new PlayerDataInfo(""));
        File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
        File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
    }
    /// <summary>
    /// 储存存档到编号i
    /// </summary>
    /// <param Name="i">储存存档的编号</param>
    public void SaveData(int i, string note = "")
    {
        if (i >= 1000 || i < 0)
        {
            Debug.LogError("存档序号不合法(0-999)");
            return;
        }
        currentSaveIndex = i;
        Directory.CreateDirectory(Path.Combine(Application.dataPath, "SaveData"));
        string dataPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.loofah", i));
        string infoPath = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveDataInfo{0:000}.loofah", i));
        string dataJs = JsonConvert.SerializeObject(data);
        string infoJs = JsonConvert.SerializeObject(new PlayerDataInfo(note));
        File.WriteAllText(dataPath, dataJs, System.Text.Encoding.UTF8);
        File.WriteAllText(infoPath, infoJs, System.Text.Encoding.UTF8);
    }
    public void SaveData(string note = "")
    {
        SaveData(currentSaveIndex, note);
    }
    /// <summary>
    /// 读取自动存档
    /// </summary>
    private void AutoLoadData()
    {
        string path = Path.Combine(Application.dataPath, "SaveData", "AutoSaveData.loofah");
        if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(path)) return;
        try
        {
            string js = File.ReadAllText(path, System.Text.Encoding.UTF8);
            data = JsonConvert.DeserializeObject<PlayerData>(js);
            isNewData = false;
        }
        catch
        {
            Debug.LogError("存档损坏");
            File.Move(path, Path.Combine(Application.dataPath, "SaveData", "CorruptedAutoSaveData.loofah"));
            File.Delete(path);
        }
    }
    /// <summary>
    /// 获取自动保存的存档信息(用于读取界面展示)
    /// </summary>
    /// <returns>自动保存的存档信息</returns>
    public PlayerDataInfo AutoLoadDataInfo()
    {
        string path = Path.Combine(Application.dataPath, "SaveData", "AutoSaveDataInfo.loofah");
        if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(path)) return null;
        try
        {
            string js = File.ReadAllText(path, System.Text.Encoding.UTF8);
            return JsonConvert.DeserializeObject<PlayerDataInfo>(js);
        }
        catch
        {
            Debug.LogError("存档信息损坏");
            File.Move(path, Path.Combine(Application.dataPath, "SaveData", "CorruptedAutoSaveDataInfo.loofah"));
            File.Delete(path);
            return null;
        }
    }
    /// <summary>
    /// 读取第i号存档
    /// </summary>
    /// <param Name="i">存档编号</param>
    private void LoadData(int i)
    {
        string path = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveData{0:000}.loofah", i));
        if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(path)) return;
        try
        {
            currentSaveIndex = i;
            string js = File.ReadAllText(path, System.Text.Encoding.UTF8);
            data = JsonConvert.DeserializeObject<PlayerData>(js);
            isNewData = false;
        }
        catch
        {
            Debug.LogError("存档损坏");
            File.Move(path, Path.Combine(Application.dataPath, "SaveData", string.Format("CorruptedSaveData{0:000}.loofah", i)));
            File.Delete(path);
        }
    }
    /// <summary>
    /// 获取编号为i的存档信息
    /// </summary>
    /// <returns>编号为i的存档信息</returns>
    public PlayerDataInfo LoadDataInfo(int i)
    {
        string path = Path.Combine(Application.dataPath, "SaveData", string.Format("SaveDataInfo{0:000}.loofah", i));
        if (!Directory.Exists(Path.Combine(Application.dataPath, "SaveData")) || !File.Exists(path)) return null;
        try
        {
            string js = File.ReadAllText(path, System.Text.Encoding.UTF8);
            return JsonConvert.DeserializeObject<PlayerDataInfo>(js);
        }
        catch
        {
            Debug.LogError("存档信息损坏");
            File.Move(path, Path.Combine(Application.dataPath, "SaveData", string.Format("CorruptedSaveDataInfo{0:000}.loofah", i)));
            File.Delete(path);
            return null;
        }
    }
    #endregion
    #region 读存设定信息
    /// <summary>
    /// 保存设置数据
    /// </summary>
    public void SaveSetting()
    {
        Settings tmp = new Settings();
        #region 填充数据
        tmp.bgmVolume = AudioMgr.GetInstance().VolumeBGM;
        tmp.soundVolume = AudioMgr.GetInstance().VolumeSound;
        #endregion
        setting = tmp;
        string json = JsonConvert.SerializeObject(tmp);
        PlayerPrefs.SetString("Settings", json);
        PlayerPrefs.Save();
    }
    /// <summary>
    /// 读取设置数据
    /// </summary>
    private void LoadSetting()
    {
        string json = PlayerPrefs.GetString("Settings");
        if (json.Equals(string.Empty)) return;
        setting = JsonConvert.DeserializeObject<Settings>(json);
        #region 填充数据
        AudioMgr.GetInstance().ChangeBGMValue(setting.bgmVolume);
        AudioMgr.GetInstance().ChangeSoundVolume(setting.soundVolume);
        #endregion
    }
    #endregion
    #region 存档更新
    //更新存档信息时只对关键信息自动存档，手动存档则都需要手动调用。两者共用一个data对象

    public void SaveBGMDic(Dictionary<string, List<string>> dict)
    {
        data.bgmDic = dict;
    }    
    #endregion
    #region 获取存档数据
    public Dictionary<string, List<string>> GetBGMDic() => data.bgmDic;
    public Triggers GetTriggers() => data.triggers;    
    #endregion
    #endregion
}
