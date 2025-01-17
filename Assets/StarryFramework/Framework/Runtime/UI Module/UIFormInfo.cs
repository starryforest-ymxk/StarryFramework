using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public class UIFormInfo
    {
        private UIForm uiForm;
        private bool paused;
        private bool covered;
    
        public UIForm UIForm => uiForm;
        public bool Paused {get => paused; set => paused = value; }
        public bool Covered {get => covered; set => covered = value; }
        
#if UNITY_EDITOR

        private bool _foldout = false;
        public bool Foldout { get => _foldout; set => _foldout = value; }
#endif

        public UIFormInfo(UIForm uiForm)
        {
            this.uiForm = uiForm;
        }
    }
}

