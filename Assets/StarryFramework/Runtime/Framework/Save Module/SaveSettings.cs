using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class SaveSettings :IManagerSettings
    {
        [Tooltip("�����Զ��洢")]
        [SerializeField]
        internal bool AutoSave = true;

        [Tooltip("�Զ��浵ʱ����(��)")]
        [Min(10)]
        [SerializeField]
        internal float AutoSaveDataInterval = 600f;

        [Tooltip("�浵ע���б�, ��һ��ΪĬ��ע��")]
        [SerializeField]
        [Multiline]
        internal List<string> SaveInfoList = new List<string>();
    }
}
