using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{

    [Serializable]
    internal class AudioSettings : IManagerSettings
    {
        [BankRef]
        [SerializeField]
        [Tooltip("全局音频库，库里的音频资源会在开启游戏时加载")]
        internal List<string> globalBanks = new List<string>();

        [SerializeField]
        [Min(10)]
        [Tooltip("自动清除非使用状态下的非标签音频的事件间隔")]
        internal float clearUnusedAudioInterval = 120f;

        [SerializeField]
        [Tooltip("不同场景的音频设置")]
        internal List<SceneAudioSettings> sceneAudioSettings = new List<SceneAudioSettings>();
    }

    [Serializable]
    internal class SceneAudioSettings
    {
        [SerializeField]
        [Tooltip("场景")]
        [SceneIndex]
        internal int scene;
        [SerializeField]
        [Tooltip("自动播放BGM（默认播放第一首）")]
        internal bool autoPlayBGM;
        [SerializeField]
        [Tooltip("场景的BGM列表")]
        internal List<EventReference> BGMList = new List<EventReference>();
        [SerializeField]
        [Tooltip("随场景加载一起预先加载的音频资源")]
        internal List<EventReference> preloadedAudios = new List<EventReference>();
        [SerializeField]
        [Tooltip("场景加载好以后除BGM以外自动播放的音频列表")]
        internal List<AutoPlayAudio> autoPlayAudios = new List<AutoPlayAudio>();
    }


    [Serializable]
    internal class AutoPlayAudio
    {
        [SerializeField]
        [Tooltip("自动播放的音频")]
        internal EventReference eventReference;

        [SerializeField]
        [Tooltip("音频标签，如果为空则没有标签")]
        internal string tag = "";
    }

}

