using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework.Test
{
    [Serializable]
    public class DemoPlayerData
    {
        public string PlayerName = "New Player";
        public int Level = 1;
        public bool HasSeenIntro;
        public List<string> UnlockedAbilities = new();
    }

    [Serializable]
    public class DemoGameSettings
    {
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public bool FullScreen = true;
        public int ResolutionIndex;
    }

    [CreateAssetMenu(menuName = "StarryFramework/Save/Custom Save Data Provider (Demo)", fileName = "DemoSaveDataProvider")]
    public class DemoSaveDataProvider : SaveDataProviderAsset
    {
        public override Type PlayerDataType => typeof(DemoPlayerData);
        public override Type GameSettingsType => typeof(DemoGameSettings);

        /// <summary>
        /// 创建默认玩家存档数据。
        /// </summary>
        public override object CreateDefaultPlayerData()
        {
            return new DemoPlayerData();
        }

        /// <summary>
        /// 创建默认游戏设置数据。
        /// </summary>
        public override object CreateDefaultGameSettings()
        {
            return new DemoGameSettings();
        }
    }
}
