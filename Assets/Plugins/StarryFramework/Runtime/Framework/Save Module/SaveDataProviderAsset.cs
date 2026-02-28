using System;
using UnityEngine;

namespace StarryFramework
{
    public interface ISaveDataProvider
    {
        Type PlayerDataType { get; }
        Type GameSettingsType { get; }
        object CreateDefaultPlayerData();
        object CreateDefaultGameSettings();
    }

    public abstract class SaveDataProviderAsset : ScriptableObject, ISaveDataProvider
    {
        public abstract Type PlayerDataType { get; }
        public abstract Type GameSettingsType { get; }
        public abstract object CreateDefaultPlayerData();
        public abstract object CreateDefaultGameSettings();
    }
}
