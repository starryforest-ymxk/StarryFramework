using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    /// <summary>
    /// UI窗体信息类，用于记录UI窗体在UI组中的状态
    /// </summary>
    public class UIFormInfo
    {
        private UIForm uiForm;
        private bool paused;
        private bool covered;
    
        /// <summary>
        /// UI窗体对象
        /// </summary>
        public UIForm UIForm => uiForm;
        
        /// <summary>
        /// 是否处于暂停状态
        /// </summary>
        public bool Paused {get => paused; set => paused = value; }
        
        /// <summary>
        /// 是否被其他窗体覆盖
        /// </summary>
        public bool Covered {get => covered; set => covered = value; }
        
#if UNITY_EDITOR

        private bool _foldout = false;
        public bool Foldout { get => _foldout; set => _foldout = value; }
#endif

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="uiForm">UI窗体对象</param>
        public UIFormInfo(UIForm uiForm)
        {
            this.uiForm = uiForm;
        }
    }
}

