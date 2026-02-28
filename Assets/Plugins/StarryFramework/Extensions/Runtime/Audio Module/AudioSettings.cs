using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework.Extentions
{
    [Serializable]
    internal class AudioSettings : IManagerSettings
    {
        [BankRef]
        [SerializeField]
        [Tooltip("Global banks preloaded at framework startup.")]
        internal List<string> globalBanks = new List<string>();

        [SerializeField]
        [Min(10)]
        [Tooltip("Automatically clear unused unnamed audio interval (seconds).")]
        internal float clearUnusedAudioInterval = 120f;

        [SerializeField]
        [Tooltip("Per-scene audio settings.")]
        internal List<SceneAudioSettings> sceneAudioSettings = new List<SceneAudioSettings>();
    }

    [Serializable]
    internal class SceneAudioSettings
    {
        [SerializeField]
        [Tooltip("Scene build index.")]
        [SceneIndex]
        internal int scene;

        [SerializeField]
        [Tooltip("Auto play the first BGM item when entering the scene.")]
        internal bool autoPlayBGM;

        [SerializeField]
        [Tooltip("BGM candidates for the scene.")]
        internal List<EventReference> BGMList = new List<EventReference>();

        [SerializeField]
        [Tooltip("Audio events to preload when entering the scene.")]
        internal List<EventReference> preloadedAudios = new List<EventReference>();

        [SerializeField]
        [Tooltip("Audio events to auto-play after scene load (excluding BGM).")]
        internal List<AutoPlayAudio> autoPlayAudios = new List<AutoPlayAudio>();
    }

    [Serializable]
    internal class AutoPlayAudio
    {
        [SerializeField]
        [Tooltip("Audio event to auto-play.")]
        internal EventReference eventReference;

        [SerializeField]
        [Tooltip("Optional audio tag; empty means untagged.")]
        internal string tag = "";
    }
}
