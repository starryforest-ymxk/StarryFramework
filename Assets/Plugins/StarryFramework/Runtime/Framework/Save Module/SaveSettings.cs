using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [Serializable]
    public class SaveSettings : IManagerSettings
    {
        [Tooltip("Enable automatic save.")]
        [SerializeField]
        internal bool AutoSave = true;

        [Tooltip("Automatic save interval in seconds.")]
        [Min(10)]
        [SerializeField]
        internal float AutoSaveDataInterval = 600f;

        [Tooltip("Editor only. Save directory path relative to Assets. Leave empty to use Assets/SaveData.")]
        [SerializeField]
        internal string EditorSaveDataDirectoryPath = "Assets/SaveData";

        [Tooltip("Save note templates. The first entry is used as the default note.")]
        [SerializeField]
        [Multiline]
        internal List<string> SaveInfoList = new List<string>();

    }
}
