using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace StarryFramework
{
    public class UIManager :IManager, IConfigurableManager
    {
        private UISettings settings;
        private bool isInitialized;
        private int serial;
        private int cacheCapacity;
        internal readonly Dictionary<string, UIGroup> uiGroupsDic = new();
        internal readonly LinkedList<UIForm> uiFormsCacheList = new();

        void IManager.Awake() { }
        void IManager.Init()
        {
            ApplySettings(true);
            isInitialized = true;
        }
        void IManager.Update()
        {
            Update();
        }
        void IManager.ShutDown()
        {
            isInitialized = false;
            ShutDown();
        }
        void IConfigurableManager.SetSettings(IManagerSettings settings)
        {
            this.settings = settings as UISettings;
            if (isInitialized)
            {
                ApplySettings(false);
            }
        }

        private void ApplySettings(bool isInit)
        {
            if (settings == null)
            {
                FrameworkManager.Debugger.LogError("UISettings is null.");
                return;
            }

            cacheCapacity = Mathf.Max(0, settings.cacheCapacity);

            if (isInit)
            {
                serial = settings.startOfSerialID;
            }
            else
            {
                // Avoid rolling back IDs at runtime to prevent duplicate serial IDs.
                serial = Mathf.Max(serial, settings.startOfSerialID);
                TrimCacheToCapacity();
            }
        }

        private void TrimCacheToCapacity()
        {
            while (uiFormsCacheList.Count > cacheCapacity)
            {
                UIForm uiFormToRelease = uiFormsCacheList.Last.Value;
                uiFormsCacheList.RemoveLast();
                uiFormToRelease.OnRelease();
            }
        }
        
        #region UIGroup

        #region Has, Get

        internal bool HasUIGroup(string uiGroupName)
        {
            return uiGroupsDic.ContainsKey(uiGroupName);
        }
        internal UIGroup GetUIGroup(string uiGroupName)
        {
            return uiGroupsDic.GetValueOrDefault(uiGroupName);
        }

        internal UIGroup[] GetAllUIGroups()
        {
            return uiGroupsDic.Values.ToArray();
        }

        #endregion
        
        #region Add, Remove

        internal void AddUIGroup(string uiGroupName)
        {
            if (uiGroupsDic.ContainsKey(uiGroupName))
            {
                FrameworkManager.Debugger.Log($"UI Group {uiGroupName} already exists");
                return;
            }
            uiGroupsDic.Add(uiGroupName, new UIGroup(uiGroupName));
        }
        internal void RemoveUIGroup(string uiGroupName)
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

        internal bool HasUIForm(string uiFormName)
        {
            foreach (var uiGroup in uiGroupsDic.Values)
            {
                if(uiGroup.HasUIForm(uiFormName))
                    return true;
            }
            return false;
        }
        
        internal UIForm GetUIForm(string uiFormName)
        {
            foreach (var uiGroup in uiGroupsDic.Values)
            {
                if(uiGroup.HasUIForm(uiFormName))
                    return uiGroup.GetUIForm(uiFormName);
            }
            FrameworkManager.Debugger.Log($"UI Form {uiFormName} doesn't exist");
            return null;
        }

        #endregion
        
        #region Open, Close, Refocus
        
        internal AsyncOperationHandle<UIForm> OpenUIForm(string uiFormName, string uiGroupName, bool pauseCoveredUIForm)
        {
            if (string.IsNullOrEmpty(uiFormName))
            {
                var errorString = $"UI Form name {uiFormName} can't be null or empty";
                FrameworkManager.Debugger.LogError(errorString);
                return Addressables.ResourceManager.CreateCompletedOperation<UIForm>(null, errorString);
            }
            if (string.IsNullOrEmpty(uiGroupName))
            {
                var errorString = $"UI Group name {uiGroupName} can't be null or empty";
                FrameworkManager.Debugger.LogError(errorString);
                return Addressables.ResourceManager.CreateCompletedOperation<UIForm>(null, errorString);
            }
            UIGroup uiGroup = GetUIGroup(uiGroupName);
            if (uiGroup == null)
            {
                var errorString = $"UI Group {uiGroupName} doesn't exist";
                FrameworkManager.Debugger.LogError(errorString);
                return Addressables.ResourceManager.CreateCompletedOperation<UIForm>(null, errorString);
            }
            
            //Load form from cache
            if (TryGetUIFormFromCache(uiFormName, out UIForm uiForm))
            {
                // uiForm.UIObject.SetActive(true);
                //Parent?
                uiGroup.AddAndOpenUIForm(uiForm);
                uiGroup.Refresh();
                return Addressables.ResourceManager.CreateCompletedOperation(uiForm, null);
            }
            //Load form from disk
            
            var objectHandle = Addressables.LoadAssetAsync<GameObject>(uiFormName);
            var formHandle = Addressables.ResourceManager.CreateChainOperation(objectHandle,
            (operationHandle) =>
            {
                if (operationHandle.Status != AsyncOperationStatus.Succeeded)
                    return Addressables.ResourceManager.CreateCompletedOperation<UIForm>(null, operationHandle.OperationException.Message);
                GameObject uiPrefab = operationHandle.Result;
                GameObject uiObject = Object.Instantiate(uiPrefab);
                //Parent?
                UIFormLogic uiFormLogic = uiObject.GetComponent<UIFormLogic>();
                UIForm newForm = new UIForm();
                AddUIFormInCache(newForm);
                newForm.OnInit(serial++, uiFormName, uiGroup, pauseCoveredUIForm, uiFormLogic, uiPrefab/*, uiObject*/);
                uiGroup.AddAndOpenUIForm(newForm);
                uiGroup.Refresh();
                return Addressables.ResourceManager.CreateCompletedOperation(newForm, null);
            });
            return formHandle;
        }
        internal void CloseUIForm(string uiFormName)
        {
            UIForm uiForm = GetUIForm(uiFormName);
            if(uiForm != null)
                CloseUIForm(uiForm);
        }
        internal void CloseUIForm(UIForm uiForm)
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

            uiGroup.RemoveAndCloseUIForm(uiForm);
            uiGroup.Refresh();
        }
        internal void RefocusUIForm(string uiFormName)
        {
            UIForm uiForm = GetUIForm(uiFormName);
            if(uiForm != null)
                RefocusUIForm(uiForm);
        }
        internal void RefocusUIForm(UIForm uiForm)
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
        internal void CloseAndReleaseAllForms()
        {
            foreach (var group in uiGroupsDic.Values)
            {
                group.RemoveAndCloseAllUIForms(false);
            }
            foreach (var form in uiFormsCacheList)
            {
                form.OnRelease();
            }
            uiFormsCacheList.Clear();
        }
        
        #endregion
        
        #endregion
        
        #region UIForm Cache

        private void AddUIFormInCache(UIForm uiForm)
        {
            // uiForm.UIObject.SetActive(false);
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


