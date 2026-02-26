using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace StarryFramework
{
    [Serializable]
    public class PlayerDataInfo
    {
        public PlayerDataInfo(int index, string note)
        {
            this.index = index;
            time = DateTime.Now.ToString("G");
            this.note = note;
        }

        public void UpdateDataInfo(string note)
        {
            time = DateTime.Now.ToString("G");
            this.note = note;
        }

        public int index;
        public string time;
        public string note;

    }
}
