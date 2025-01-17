using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class UIComponent : BaseComponent
    {
        private UIManager _manager = null;
        private UIManager manager => _manager ??= FrameworkManager.GetManager<UIManager>();
        
        [SerializeField] private UISettings settings;
        
        public Dictionary<string, UIGroup> UIGroupsDic => manager.uiGroupsDic;
        public LinkedList<UIForm> UIFormsCacheList => manager.uiFormsCacheList;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if(EditorApplication.isPlaying && _manager != null)
                (_manager as IManager).SetSettings(settings);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<UIManager>();
            (_manager as IManager).SetSettings(settings);
        }
        
        
        #region UIGroup

        public bool HasUIGroup(string uiGroupName)
        {
            return manager.HasUIGroup(uiGroupName);
        }
        
        public UIGroup GetUIGroup(string uiGroupName)
        {
            return manager.GetUIGroup(uiGroupName);
        }

        public UIGroup[] GetAllUIGroups()
        {
            return manager.GetAllUIGroups();
        }
        
        public void AddUIGroup(string uiGroupName)
        {
            manager.AddUIGroup(uiGroupName);
        }

        public void RemoveUIGroup(string uiGroupName)
        {
            manager.RemoveUIGroup(uiGroupName);
        }
        
        
        #endregion
        
        #region UIForm

        public bool HasUIForm(string uiFormName)
        {
            return manager.HasUIForm(uiFormName);
        }
        
        public UIForm GetUIForm(string uiFormName)
        {
            return manager.GetUIForm(uiFormName);
        }

        public void OpenUIForm(string uiFormName, string uiGroupName, bool pauseCoveredUIForm)
        {
            manager.OpenUIForm(uiFormName, uiGroupName, pauseCoveredUIForm);
        }

        public void CloseUIForm(string uiFormName)
        {
            manager.CloseUIForm(uiFormName);
        }
        
        public void CloseUIForm(UIForm uiForm)
        {
            manager.CloseUIForm(uiForm);
        }

        public void RefocusUIForm(string uiFormName)
        {
            manager.RefocusUIForm(uiFormName);
        }

        public void RefocusUIForm(UIForm uiForm)
        {
            manager.RefocusUIForm(uiForm);
        }
        
        #endregion
        
    }
}

