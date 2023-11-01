using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Audio;

/// <summary>
/// BGM管理器，Mono，挂载在GameManager上
/// 作用是在切换场景时自动播放该场景的默认BGM
/// 外部接口１.可修改某场景的默认BGM　２.改变该管理器的生效状态
/// </summary>
public class BGMMgr : MonoSingleton<BGMMgr>
{
    
    /// <summary>
    /// 用List设置起始时各场景BGM播放方式，运行时则会转存到字典中
    /// </summary>
    [Serializable]
    private struct BGMGroup
    {
        public string BGMName;
        public List<string> SceneName;
    }
    [SerializeField] private bool threeDMode;
    [SerializeField] private Transform bgmObj;
    [SerializeField] private Transform ambObj;
    [SerializeField] private Transform soundObj;
    [SerializeField] private List<BGMGroup> BGMList = new List<BGMGroup>();
    private Dictionary<string, List<string>> BGMDic;
    private Dictionary<string, AudioMixerSnapshot> snapDic = new Dictionary<string, AudioMixerSnapshot>();

    private string currentBGMName = "";
    private bool isEnabled = true;

    public Dictionary<string, AudioMixerSnapshot> SnapDic => snapDic;
    public Transform BgmObj => bgmObj;
    public Transform AmbObj => ambObj;
    public Transform SoundObj => soundObj;
    #region 事件注册与字典转存
    protected override void Awake()
    {
        base.Awake();
        if (threeDMode)
        {
            foreach (var source in this.GetComponentsInChildren<AudioSource>())
            {
                source.spatialBlend = 1f;
            }
        }

        AudioMixerSnapshot[] snaps = ResMgr.GetInstance().LoadAllRes<AudioMixerSnapshot>("Music/AudioMixer");
        foreach (var snap in snaps)
        {
            snapDic.Add(snap.name, snap);
        }
    }
    private void Start()
    {
        BGMDic = SaveMgr.GetInstance().GetBGMDic();
        if (BGMDic == null)
        {
            BGMDic = new Dictionary<string, List<string>>();
            foreach (var bgmGroup in BGMList)
            {
                BGMDic.Add(bgmGroup.BGMName, bgmGroup.SceneName);
            }
            SaveMgr.GetInstance().SaveBGMDic(BGMDic);
        }
        BGMList.Clear();
        BGMList = null;
        UpdateBGM();
    }
    private void OnEnable()
    {
        EventMgr.GetInstance().AddEventListener(EventDic.AfterChangeScene, OnChangeScene);
    }
    private void OnDisable()
    {
        EventMgr.GetInstance().DeleteEventListener(EventDic.AfterChangeScene, OnChangeScene);
    }
    private void OnChangeScene()
    {
        if (isEnabled) UpdateBGM();
    }
    #endregion
    #region 主要功能
    /// <summary>
    /// 依据当前场景的名称播放默认BGM，如果默认BGM与上个场景相同则不重新播放
    /// </summary>
    private void UpdateBGM()
    {
        string name = SceneMgr.GetInstance().GetCurrentSceneName();
        string bgmName = string.Empty;
        foreach (var bgm in BGMDic.Keys)
        {
            if (BGMDic[bgm].Exists(x => x == name))
            {
                bgmName = bgm;
                break;
            }
        }
        if (bgmName.Equals(string.Empty))
        {
            Debug.LogWarning("未设定BGM的场景，检查是否需要设置");
            return;
        }
        else
        {
            if (currentBGMName == bgmName) return;
            currentBGMName = bgmName;
            AudioMgr.GetInstance().PlayBGM(bgmName);
        }
    }
    #endregion
    #region 外部接口
    /// <summary>
    /// 修改进入某个场景时的默认BGM
    /// </summary>
    /// <param Name="sceneName">场景名</param>
    /// <param Name="NewBGMName">新的BGM名称</param>
    /// <param Name="effectiveImdiately">是否立即改变BGM</param>
    public void ChangeDefaultBGM(string sceneName, string NewBGMName, bool effectiveImdiately = true)
    {
        foreach (var scenes in BGMDic.Values)
        {
            if (scenes.Contains(sceneName))
            {
                scenes.Remove(sceneName);
                break;
            }
        }
        if (BGMDic.ContainsKey(NewBGMName)) BGMDic[NewBGMName].Add(sceneName);
        else BGMDic.Add(NewBGMName, new List<string>() { sceneName });
        SaveMgr.GetInstance().SaveBGMDic(BGMDic);
        if (effectiveImdiately) UpdateBGM();
    }
    /// <summary>
    /// 是否在改变场景时播放默认BGM
    /// </summary>
    /// <param Name="enable"></param>
    public void SetEnable(bool enable)
    {
        isEnabled = enable;
    }
    #endregion
}
