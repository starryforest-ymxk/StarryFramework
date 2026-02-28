using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace StarryFramework
{
    [Serializable]
    public sealed class PlayerData
    {
        #region 角色
        public int test = 0;
        #endregion
        #region 事件
        public bool event1;
        public bool event2;
        public bool event3;
        public bool event4;
        #endregion
        #region 道具
        public List<string> inventoryList = new(new[] { "test1", "test2" });
        public string[] inventoryArrow = new[] { "test1", "test2" };
        #endregion
        #region 成就

        #endregion
        #region 记录

        #endregion
        #region 其他

        public CustomData customData = new()
        {
            experience = 0f,
            inventory = new List<string>(new[] { "test1", "test2" }),
            achievements = new Dictionary<string, int>(new KeyValuePair<string, int>[] { new("test3", 3) })
        };

        #endregion
    }

    [Serializable]
    public sealed class CustomData
    {
        public float experience = 0f;
        public List<string> inventory = new();
        public Dictionary<string, int> achievements = new(); // 会显示为只读
    }
}
