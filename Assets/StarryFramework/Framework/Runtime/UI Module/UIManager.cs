using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace StarryFramework
{
    public class UIManager :IManager
    {
        private UISettings settings;
        private int serial;
        private int cacheCapacity;
        internal readonly Dictionary<string, UIGroup> uiGroupsDic = new();
        internal readonly LinkedList<UIForm> uiFormsCacheList = new();

        void IManager.Awake() { }

        void IManager.Init()
        {
            serial = settings.startOfSerialID;
            cacheCapacity = settings.cacheCapacity;
        }

        void IManager.Update()
        {
            Update();
        }

        void IManager.ShutDown()
        {
            ShutDown();
        }

        void IManager.SetSettings(IManagerSettings settings)
        {
            this.settings = settings as UISettings;
        }
        
        #region UIGroup

        #region Has, Get

        public bool HasUIGroup(string uiGroupName)
        {
            return uiGroupsDic.ContainsKey(uiGroupName);
        }
        public UIGroup GetUIGroup(string uiGroupName)
        {
            return uiGroupsDic.GetValueOrDefault(uiGroupName);
        }

        public UIGroup[] GetAllUIGroups()
        {
            return uiGroupsDic.Values.ToArray();
        }

        #endregion
        
        #region Add, Remove

        public void AddUIGroup(string uiGroupName)
        {
            if (uiGroupsDic.ContainsKey(uiGroupName))
            {
                FrameworkManager.Debugger.Log($"UI Group {uiGroupName} already exists");
                return;
            }
            uiGroupsDic.Add(uiGroupName, new UIGroup(uiGroupName));
        }

        public void RemoveUIGroup(string uiGroupName)
        {
            if (!uiGroupsDic.ContainsKey(uiGroupName))
            {
                FrameworkManager.Debugger.LogError($"UI Group {uiGroupName} doesn't exist");
                return;
            }
            uiGroupsDic.Remove(uiGroupName);
        }

        #endregion
        
        #endregion
        
        #region UIForm

        #region Has, Get

        public bool HasUIForm(string uiFormName)
        {
            foreach (var uiGroup in uiGroupsDic.Values)
            {
                if(uiGroup.HasUIForm(uiFormName))
                    return true;
            }
            return false;
        }
        
        public UIForm GetUIForm(string uiFormName)
        {
            foreach (var uiGroup in uiGroupsDic.Values)
            {
                if(uiGroup.HasUIForm(uiFormName))
                    return uiGroup.GetUIForm(uiFormName);
            }
            FrameworkManager.Debugger.LogWarning($"UI Form {uiFormName} doesn't exist");
            return null;
        }

        #endregion
        
        #region Open, Close, Refocus
        
        public void OpenUIForm(string uiFormName, string uiGroupName, bool pauseCoveredUIForm)
        {
            if (string.IsNullOrEmpty(uiFormName))
            {
                FrameworkManager.Debugger.LogError($"UI Form name {uiFormName} can't be null or empty");
                return;
            }
            if (string.IsNullOrEmpty(uiGroupName))
            {
                FrameworkManager.Debugger.LogError($"UI Group name {uiGroupName} can't be null or empty");
                return;
            }
            UIGroup uiGroup = GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                FrameworkManager.Debugger.LogError($"UI Group {uiGroupName} doesn't exist");
                return;
            }
            
            //Load form from cache
            if (TryGetUIFormFromCache(uiFormName, out UIForm uiForm))
            {
                uiForm.UIObject.SetActive(true);
                //Parent?
                uiGroup.AddUIForm(uiForm);
                uiForm.OnOpen();
                uiGroup.Refresh();
            }
            //Load form from disk
            else
            {
                Addressables.LoadAssetAsync<GameObject>(uiFormName).Completed+= (handle) =>
                {
                    GameObject uiPrefab = handle.Result;
                    GameObject uiObject = Object.Instantiate(uiPrefab);
                    //Parent?
                    UIFormLogic uiFormLogic = uiObject.GetComponent<UIFormLogic>();
                    UIForm newForm = new UIForm();
                    newForm.OnInit(serial++, uiFormName, uiGroup, pauseCoveredUIForm, uiFormLogic, uiPrefab, uiObject);
                    uiGroup.AddUIForm(newForm);
                    newForm.OnOpen();
                    uiGroup.Refresh();
                };
            }
        }

        public void CloseUIForm(string uiFormName)
        {
            UIForm uiForm = GetUIForm(uiFormName);
            if(uiForm != null)
                CloseUIForm(uiForm);
        }
        
        public void CloseUIForm(UIForm uiForm)
        {
            if (uiForm == null)
            {
                FrameworkManager.Debugger.LogError("UI Form can't be null");
                return;
            }
            UIGroup uiGroup = uiForm.UIGroup;
            if (uiGroup == null)
            {
                FrameworkManager.Debugger.LogError("UI group is invalid.");
                return;
            }

            uiGroup.RemoveUIForm(uiForm);
            uiForm.OnClose(false);
            uiGroup.Refresh();
            RecycleUIForm(uiForm);
        }

        public void RefocusUIForm(string uiFormName)
        {
            UIForm uiForm = GetUIForm(uiFormName);
            if(uiForm != null)
                RefocusUIForm(uiForm);
        }

        public void RefocusUIForm(UIForm uiForm)
        {
            if (uiForm == null)
            {
                FrameworkManager.Debugger.LogError("UI form is invalid.");
                return;
            }

            UIGroup uiGroup = uiForm.UIGroup;
            if (uiGroup == null)
            {
                FrameworkManager.Debugger.LogError("UI group is invalid.");
                return;
            }

            uiGroup.RefocusUIForm(uiForm);
            uiGroup.Refresh();
            uiForm.OnRefocus();
        }
        
        #endregion
        
        #endregion
        
        #region UIForm Cache

        private void RecycleUIForm(UIForm uiForm)
        {
            uiForm.UIObject.SetActive(false);
            if (InCacheUIForm(uiForm))
            {
                uiFormsCacheList.Remove(uiForm);
                uiFormsCacheList.AddFirst(uiForm);
            }
            else
            {
                uiFormsCacheList.AddFirst(uiForm);
                if (uiFormsCacheList.Count > cacheCapacity)
                {
                    UIForm uiFormToRelease = uiFormsCacheList.Last.Value;
                    uiFormsCacheList.RemoveLast();
                    uiFormToRelease.OnRelease();
                }
            }
        }
        private bool InCacheUIForm(UIForm uiForm)
        {
            return uiFormsCacheList.Contains(uiForm);
        }
        private bool TryGetUIFormFromCache(string uiFormName, out UIForm uiForm)
        {
            uiForm = null;
            foreach (var form in uiFormsCacheList)
            {
                if (form.UIFormAssetName.Equals(uiFormName))
                {
                    uiForm = form;
                    return true;
                }
            }
            return false;
        }
        
        #endregion
        
        #region private lifecycle

        private void Update()
        {
            //Update
            UIGroup[] uiGroups = uiGroupsDic.Values.ToArray();
            foreach (var uiGroup in uiGroups)
            {
                uiGroup.Update();
            }
        }

        private void ShutDown()
        {   
            UIGroup[] uiGroups = uiGroupsDic.Values.ToArray();
            foreach (var uiGroup in uiGroups)
            {
                uiGroup.ShutDown();
            }
            uiGroupsDic.Clear();
            
            foreach (var uiForm in uiFormsCacheList.Where(uiForm => !uiForm.ReleaseTag))
            {
                uiForm.OnRelease();
            }
            
            uiFormsCacheList.Clear();
            
        }
        
        
        #endregion
    
    
    }
}


