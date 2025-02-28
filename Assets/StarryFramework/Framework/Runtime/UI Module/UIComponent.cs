using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class UIComponent : BaseComponent
    {
        private UIManager _manager;
        private UIManager Manager => _manager ??= FrameworkManager.GetManager<UIManager>();
        
        [SerializeField] private UISettings settings;
        
        public Dictionary<string, UIGroup> UIGroupsDic => Manager.uiGroupsDic;
        public LinkedList<UIForm> UIFormsCacheList => Manager.uiFormsCacheList;
        
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
            return Manager.HasUIGroup(uiGroupName);
        }
        
        public UIGroup GetUIGroup(string uiGroupName)
        {
            return Manager.GetUIGroup(uiGroupName);
        }

        public UIGroup[] GetAllUIGroups()
        {
            return Manager.GetAllUIGroups();
        }
        
        public void AddUIGroup(string uiGroupName)
        {
            Manager.AddUIGroup(uiGroupName);
        }

        public void RemoveUIGroup(string uiGroupName)
        {
            Manager.RemoveUIGroup(uiGroupName);
        }
        
        
        #endregion
        
        #region UIForm

        public bool HasUIForm(string uiFormAssetName)
        {
            return Manager.HasUIForm(uiFormAssetName);
        }
        
        public UIForm GetUIForm(string uiFormAssetName)
        {
            return Manager.GetUIForm(uiFormAssetName);
        }

        public AsyncOperationHandle<UIForm> OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return Manager.OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm);
        }

        public void CloseUIForm(string uiFormAssetName)
        {
            Manager.CloseUIForm(uiFormAssetName);
        }
        
        public void CloseUIForm(UIForm uiForm)
        {
            Manager.CloseUIForm(uiForm);
        }

        public void RefocusUIForm(string uiFormAssetName)
        {
            Manager.RefocusUIForm(uiFormAssetName);
        }

        public void RefocusUIForm(UIForm uiForm)
        {
            Manager.RefocusUIForm(uiForm);
        }
        
        #endregion
        
        public void CloseAndReleaseAllForms()
        {
            Manager.CloseAndReleaseAllForms();
        }
        
    }
}

