using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public static class FrameworkEvent
    {
        public const string BeforeChangeScene = "BeforeChangeScene";
        public const string AfterChangeScene = "AfterChangeScene";
        public const string BeforeLoadScene = "BeforeLoadScene";
        public const string AfterLoadScene = "AfterLoadScene";
        public const string BeforeUnloadScene = "BeforeUnLoadScene";
        public const string AfterUnloadScene = "AfterUnLoadScene";

        public const string InActiveCurrentScene = "InActiveCurrentScene";
        public const string OnNewActiveScene = "OnNewActiveScene";

        public const string BeforeLoadAsset = "BeforeLoadAsset";
        public const string AfterLoadAsset = "AfterLoadAsset";

        public const string OnSaveData = "OnSaveData";
        public const string OnLoadData = "OnLoadData";
        public const string OnUnloadData = "OnUnloadData";
        public const string OnDeleteData = "OnDeleteData";
        public const string OnDeleteCurrentData = "OnDeleteCurrentData";


        public const string OnEnterMainGame = "OnEnterMainGame";
        public const string OnLeaveMainGame = "OnLeaveMainGame";


    }
}
