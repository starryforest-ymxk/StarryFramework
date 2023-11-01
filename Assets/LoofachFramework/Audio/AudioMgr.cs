using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Audio;
/// <summary>
/// 音频管理器，非Mono
/// 作用是播放BGM和3D/2D音效
/// </summary>
public sealed class AudioMgr : Singleton<AudioMgr>
{
    [Header("音量")]
    private float volumeBGM = 1f;
    private float volumeSound = 1f;
    public float VolumeBGM => volumeBGM;
    public float VolumeSound => volumeSound;
    [Header("音效单位")]
    private List<AudioSource> soundList = new List<AudioSource>();
    [Header("事件音乐状态量")]
    //正在执行的音乐事件协程
    private Coroutine currentEvent;
    //是否在音乐事件中
    private bool isInEvent = false;
    //在音乐事件中缓存下一个BGMsnap
    private string nextSnap = "";

    #region 更新
    public AudioMgr()
    {
        MonoMgr.GetInstance().AddUpdateListener(MyUpdate);
    }
    /// <summary>
    /// MonoUpdate事件，用于及时清除完成播放的音效物体
    /// </summary>
    public void MyUpdate()
    {
        for (int i = soundList.Count - 1; i >= 0; i--)
        {
            if (soundList == null || soundList[i] == null) continue;
            if (!soundList[i].isPlaying)
            {
                GameObject.Destroy(soundList[i]);
                soundList.RemoveAt(i);
            }
        }
    }
    #endregion
    #region AudioMixerSnap
    /// <summary>
    /// 进入任意一个Resources/Music/AudioMixer/路径下的Snap;若当前处于音乐事件中且待切换Snap是BGMsnap，则缓存下一个这个snap
    /// </summary>
    /// <param Name="snapName">Snap的名称</param>
    /// <param Name="transTime">切换到Snap的过渡时间</param>
    public void SetSnap(string snapName, float transTime = GameConstant.DefaultAudioFaderTime)
    {
        var dic = BGMMgr.GetInstance().SnapDic;
        if (!dic.ContainsKey(snapName))
        {
            Debug.LogError("Error snap Name");
            return;
        }
        if (isInEvent)
        {
            if (snapName.Equals(MixerDic.EVENTIN))
            {
                if (currentEvent != null) MonoMgr.GetInstance().StopCoroutine(currentEvent);
                dic[snapName].TransitionTo(transTime);
            }
            else if (dic[snapName].audioMixer.name.Equals(MixerDic.MIXER_BGMAUDIOMIXER))
            {
                nextSnap = snapName;
            }
            else dic[snapName].TransitionTo(transTime);
        }
        else
        {
            if (dic[snapName].audioMixer.name.Equals(MixerDic.MIXER_BGMAUDIOMIXER)) nextSnap = snapName;
            dic[snapName].TransitionTo(transTime);            
        }
    }
    /// <summary>
    /// 获取正在播放的BGMsnap
    /// </summary>
    /// <returns>正在播放的BGMsnap</returns>
    public AudioMixerSnapshot GetCurrentBGMSnap()
    {
        var dic = BGMMgr.GetInstance().SnapDic;
        foreach (var snap in dic.Values)
        {
            if (snap.audioMixer.name.Equals(MixerDic.MIXER_BGMAUDIOMIXER) && snap)
            {
                if (!snap.name.Equals(MixerDic.EVENTIN)) nextSnap = snap.name;
                return snap;
            }
        }
        Debug.LogError("Find no current BGM snap");
        return null;
    }
    #endregion
    #region BGM方法
    /// <summary>
    /// 循环播放Resources/Music/BGM/路径下的一个BGM
    /// </summary>
    /// <param Name="name">BGM名称或路径(不含拓展名)</param>
    /// /// <param Name="reset">如果尝试播放的音乐名与正在播放的音乐名相等，是否重新开始播放</param>
    public void PlayBGM(string name, bool reset = false)
    {
        AudioSource s = BGMMgr.GetInstance().BgmObj.GetChild(0).GetComponent<AudioSource>();
        string[] namesp = name.Split('/');
        if (!reset && s.clip != null && s.clip.name == namesp[namesp.Length - 1]) return;
        ResMgr.GetInstance().AsyncLoad<AudioClip>("Music/BGM/" + name, (clip) =>
        {
            s.clip = clip;
            s.loop = true;
            s.volume = volumeBGM;
            s.Play();
        });
    }
    /// <summary>
    /// 循环播放Resources/Music/BGM/路径下的一个辅助BGM
    /// </summary>
    /// <param Name="name">辅助BGM名称或路径</param>
    /// <param Name="index">辅助BGM的编号(从1起)</param>
    /// <param Name="reset">如果尝试播放的辅助音乐名与正在播放的辅助音乐名相等，是否重新开始播放</param>
    public void PlayAux(string name, int index, bool reset = false)
    {
        Transform t = BGMMgr.GetInstance().BgmObj;
        if (t.childCount <= index)
        {
            Debug.LogError("Error aux bgm index");
            return;
        }
        AudioSource s = t.GetChild(index).GetComponent<AudioSource>();
        string[] namesp = name.Split('/');
        if (!reset && s.clip != null && s.clip.name == namesp[namesp.Length - 1]) return;
        ResMgr.GetInstance().AsyncLoad<AudioClip>("Music/BGM/" + name, (clip) =>
        {
            s.clip = clip;
            s.loop = true;
            s.volume = volumeBGM;
            s.Play();
        });
    }
    /// <summary>
    /// 停止当前BGM
    /// </summary>
    public void StopBGM()
    {
        foreach (var bgm in BGMMgr.GetInstance().BgmObj.GetComponentsInChildren<AudioSource>())
        {
            bgm.Stop();
        }
    }
    /// <summary>
    /// 改变BGM音量［0,1］
    /// </summary>
    /// <param Name="volume">音量[0,1]</param>
    public void ChangeBGMValue(float volume)
    {
        volumeBGM = volume;
        foreach (var bgm in BGMMgr.GetInstance().BgmObj.GetComponentsInChildren<AudioSource>())
        {
            bgm.volume = VolumeBGM;
        }
        SaveMgr.GetInstance().SaveSetting();
    }
    #endregion
    #region 音效方法
    /// <summary>
    /// 异步播放Resources/Music/Sounds/路径下的音效，对音源完成回调方法
    /// </summary>
    /// <param Name="name">音频的名称或路径</param>
    /// <param Name="isLoop">是否循环</param>
    /// <param Name="callback">音源的回调函数</param>
    public void PlaySound(string name, bool isLoop, UnityAction<AudioSource> callback = null)
    {
        AudioSource source = BGMMgr.GetInstance().SoundObj.gameObject.AddComponent<AudioSource>();
        ResMgr.GetInstance().AsyncLoad<AudioClip>("Music/Sounds/" + name, (clip) =>
        {
            source.clip = clip;
            source.volume = volumeSound;
            source.loop = isLoop;
            source.Play();
            Debug.Log(clip);
            soundList.Add(source);
            if (callback != null)
            { callback(source); }            
        });
    }
    /// <summary>
    /// 异步播放Resources/Music/Sounds/路径下的3D音效,设定播放原和AudioMixer
    /// </summary>
    /// <param Name="name">音频的名称或路径</param>
    /// <param Name="isLoop">是否循环</param>
    /// <param Name="target">音源物体</param>
    /// <param Name="callback">音源的回调函数</param>
    /// <param Name="mixerName">AudioMixer在Resources/Music/AudioMixer下的名称或路径</param>
    public void PlaySound(string name, bool isLoop, GameObject target, UnityAction<AudioSource> callback = null, string mixerName = "")
    {
        AudioSource source = target.AddComponent<AudioSource>();
        if (mixerName == "") loadMusic();
        else
        {
            AudioMixerGroup[] mixers = ResMgr.GetInstance().LoadAllRes<AudioMixerGroup>("Music/AudioMixer");
            foreach (var mixer in mixers)
            {
                if (mixer.name.Equals(mixerName))
                {
                    loadMusic(mixer);
                    return;
                }
            }
        }
        void loadMusic(AudioMixerGroup mixer = null)
        {
            ResMgr.GetInstance().AsyncLoad<AudioClip>("Music/Sounds/" + name, (clip) =>
            {
                source.clip = clip;
                source.volume = volumeSound;
                source.loop = isLoop;
                source.Play();
                soundList.Add(source);
                if (callback != null)
                { callback(source); }
                source.spatialBlend = 1;
                source.outputAudioMixerGroup = mixer;
            });
        }
    }
    /// <summary>
    /// 异步播放Resources/Music/Sounds/路径下的音效。这个音效会遮盖BGM，并在结束后自动切换回正常的BGM
    /// </summary>
    /// <param Name="name">音频的名称或路径</param>
    /// <param Name="beforeTransTime">进入事件音乐的过渡时间</param>
    /// <param Name="afterTransTime">退出事件音乐的过渡时间</param>
    /// <param Name="callback">音源的回调函数</param>
    public void PlayEventSound(string name, float beforeTransTime = GameConstant.DefaultAudioFaderTime, float afterTransTime = GameConstant.DefaultAudioFaderTime, UnityAction<AudioSource> callback = null)
    {
        AudioSource source = BGMMgr.GetInstance().SoundObj.gameObject.AddComponent<AudioSource>();
        AudioMixerGroup[] mixers = ResMgr.GetInstance().LoadAllRes<AudioMixerGroup>("Music/AudioMixer");
        foreach (var mixer in mixers)
        {
            if (mixer.name.Equals(MixerDic.EVENTSOUND))
            {
                ResMgr.GetInstance().AsyncLoad<AudioClip>("Music/Sounds/" + name, (clip) =>
                {
                    source.clip = clip;
                    source.volume = volumeSound;
                    source.loop = false;
                    source.Play();
                    soundList.Add(source);
                    if (callback != null)
                    { callback(source); }
                    source.spatialBlend = 1;
                    source.outputAudioMixerGroup = mixer;
                    SetSnap(MixerDic.EVENTIN, beforeTransTime);
                    currentEvent = MonoMgr.GetInstance().StartCoroutine(transEvent());
                });
                return;
            }
        }
        IEnumerator transEvent()
        {
            isInEvent = true;
            while (source != null && source.isPlaying)
            {
                yield return null;
            }
            isInEvent = false;
            if (nextSnap != "") SetSnap(nextSnap, afterTransTime);
            else SetSnap(MixerDic.AUXNONE, afterTransTime);
        }
    }
    /// <summary>
    /// 停止播放某个音效
    /// </summary>
    /// <param Name="source">音源</param>
    public void StopSound(AudioSource source)
    {
        if (soundList.Contains(source))
        {
            soundList.Remove(source);
            source.Stop();
            GameObject.Destroy(source);
        }
    }
    /// <summary>
    /// 停止播放所有音效
    /// </summary>
    public void StopAllSound()
    {
        foreach (var source in soundList)
        {
            source.Stop();
            soundList.Remove(source);
            GameObject.Destroy(source);
        }
    }
    /// <summary>
    /// 改变所有音效音量[0,1]
    /// </summary>
    /// <param Name="volume">音量[0,1]</param>
    public void ChangeSoundVolume(float volume)
    {
        volumeSound = volume;
        foreach (var item in soundList)
        {
            item.volume = volumeSound;
        }
        SaveMgr.GetInstance().SaveSetting();        
    }
    #endregion    
}
public static class MixerDic
{
    public const string MIXER_AMBIENCEAUIOMIXER = "AmbienceAudioMixer";
    #region
    public const string MUTEAMB = "MuteAmb";
    public const string AMB1IN = "Amb1In";
    public const string AMB2IN = "Amb2In";
    public const string AMB12IN = "Amb12In";
    #endregion

    public const string MIXER_BGMAUDIOMIXER = "BGMAudioMixer";
    #region
    public const string AUXNONE = "AuxNone";
    public const string AUX1IN = "Aux1In";
    public const string AUX2IN = "Aux2In";
    public const string EVENTIN = "EventIn";
    public const string MUTEBGM = "MuteBGM";
    #endregion

    public const string MIXER_SOUNDAUDIOMIXER = "SoundAudioMier";
    #region
    public const string NORMALSOUND = "NormalSound";
    #endregion

    #region spicial group
    public const string EVENTSOUND = "EventSound";
    #endregion
}


