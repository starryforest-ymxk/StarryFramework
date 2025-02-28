using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework.Extentions
{

    [Serializable]
    internal class AudioSettings : IManagerSettings
    {
        [BankRef]
        [SerializeField]
        [Tooltip("ȫ����Ƶ�⣬�������Ƶ��Դ���ڿ�����Ϸʱ����")]
        internal List<string> globalBanks = new List<string>();

        [SerializeField]
        [Min(10)]
        [Tooltip("�Զ������ʹ��״̬�µķǱ�ǩ��Ƶ���¼����")]
        internal float clearUnusedAudioInterval = 120f;

        [SerializeField]
        [Tooltip("��ͬ��������Ƶ����")]
        internal List<SceneAudioSettings> sceneAudioSettings = new List<SceneAudioSettings>();
    }

    [Serializable]
    internal class SceneAudioSettings
    {
        [SerializeField]
        [Tooltip("����")]
        [SceneIndex]
        internal int scene;
        [SerializeField]
        [Tooltip("�Զ�����BGM��Ĭ�ϲ��ŵ�һ�ף�")]
        internal bool autoPlayBGM;
        [SerializeField]
        [Tooltip("������BGM�б�")]
        internal List<EventReference> BGMList = new List<EventReference>();
        [SerializeField]
        [Tooltip("�泡������һ��Ԥ�ȼ��ص���Ƶ��Դ")]
        internal List<EventReference> preloadedAudios = new List<EventReference>();
        [SerializeField]
        [Tooltip("�������غ��Ժ��BGM�����Զ����ŵ���Ƶ�б�")]
        internal List<AutoPlayAudio> autoPlayAudios = new List<AutoPlayAudio>();
    }


    [Serializable]
    internal class AutoPlayAudio
    {
        [SerializeField]
        [Tooltip("�Զ����ŵ���Ƶ")]
        internal EventReference eventReference;

        [SerializeField]
        [Tooltip("��Ƶ��ǩ�����Ϊ����û�б�ǩ")]
        internal string tag = "";
    }

}

