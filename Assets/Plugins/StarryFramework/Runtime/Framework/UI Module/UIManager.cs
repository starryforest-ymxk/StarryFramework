using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private long focusSequence;
        private ReadOnlyDictionary<string, UIGroup> uiGroupsReadOnlyView;
        internal readonly Dictionary<string, UIGroup> uiGroupsDic = new();
        internal readonly LinkedList<UIForm> uiFormsCacheList = new();
        // Tracks in-flight open requests by deduplication scope key (not just asset name).
        private readonly Dictionary<string, AsyncOperationHandle<UIForm>> openingRequests = new();
        private readonly Dictionary<int, UIForm> activeFormsBySerial = new();
        private readonly Dictionary<string, HashSet<int>> activeSerialsByAsset = new();
        private readonly Dictionary<string, Dictionary<string, HashSet<int>>> activeSerialsByAssetAndGroup = new();

        internal IReadOnlyDictionary<string, UIGroup> UIGroupsReadOnlyView
            => uiGroupsReadOnlyView ??= new ReadOnlyDictionary<string, UIGroup>(uiGroupsDic);

        internal UIForm[] GetUIFormsCacheSnapshot()
        {
            return uiFormsCacheList.ToArray();
        }

        internal UIForm[] GetAllActiveUIFormsSnapshot()
        {
            return activeFormsBySerial.Values
                .Where(IsValidActiveUIForm)
                .OrderByDescending(form => form.LastFocusSequence)
                .ThenByDescending(form => form.SerialID)
                .ToArray();
        }

        internal int ActiveFormCount => activeFormsBySerial.Count;
        internal int ActiveAssetKeyCount => activeSerialsByAsset.Count;
        internal int OpeningRequestCount => openingRequests.Count;

        internal string[] GetOpeningRequestKeysSnapshot()
        {
            return openingRequests.Keys
                .OrderBy(key => key, System.StringComparer.Ordinal)
                .ToArray();
        }

        void IManager.Awake() { }
        void IManager.Init()
        {
            focusSequence = 0;
            ClearActiveUIFormIndices();
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
                ReleaseCachedUIForm(uiFormToRelease);
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
            if (!uiGroupsDic.TryGetValue(uiGroupName, out UIGroup uiGroup))
            {
                FrameworkManager.Debugger.LogError($"UI Group {uiGroupName} doesn't exist");
                return;
            }

            UIForm[] activeForms = uiGroup.GetAllUIForms();
            uiGroup.RemoveAndCloseAllUIForms(false);

            foreach (var uiForm in activeForms)
            {
                UnregisterActiveUIForm(uiForm);

                if (cacheCapacity > 0)
                {
                    AddUIFormInCache(uiForm);
                }
                else
                {
                    ReleaseUIForm(uiForm);
                }
            }

            uiGroupsDic.Remove(uiGroupName);
        }

        #endregion
        
        
        
        #endregion
        
        #region UIForm

        #region Has, Get

        internal bool HasUIForm(int serialId)
        {
            return GetUIForm(serialId) != null;
        }

        internal bool HasUIForm(string uiFormName)
        {
            return GetTopUIForm(uiFormName) != null;
        }

        internal bool HasUIForm(string uiFormName, string instanceKey)
        {
            return GetTopUIFormByInstanceKey(uiFormName, instanceKey) != null;
        }

        internal UIForm GetUIForm(int serialId)
        {
            if (activeFormsBySerial.TryGetValue(serialId, out UIForm uiForm) &&
                IsValidActiveUIForm(uiForm))
            {
                return uiForm;
            }

            return null;
        }
        
        internal UIForm GetUIForm(string uiFormName)
        {
            UIForm uiForm = GetTopUIForm(uiFormName);
            if (uiForm != null)
            {
                return uiForm;
            }

            FrameworkManager.Debugger.Log($"UI Form {uiFormName} doesn't exist");
            return null;
        }

        internal UIForm GetUIForm(string uiFormName, string instanceKey)
        {
            UIForm uiForm = GetTopUIFormByInstanceKey(uiFormName, instanceKey);
            if (uiForm != null)
            {
                return uiForm;
            }

            FrameworkManager.Debugger.Log(
                $"UI Form '{uiFormName}' with instanceKey '{NormalizeInstanceKey(instanceKey) ?? "<null>"}' doesn't exist.");
            return null;
        }

        internal UIForm[] GetUIForms(string uiFormName)
        {
            return GetUIFormsCore(uiFormName, null);
        }

        internal UIForm[] GetUIForms(string uiFormName, string uiGroupName)
        {
            return GetUIFormsCore(uiFormName, uiGroupName);
        }

        internal int GetUIFormCount(string uiFormName)
        {
            return GetUIForms(uiFormName).Length;
        }

        internal UIForm GetTopUIForm(string uiFormName)
        {
            UIForm[] forms = GetUIForms(uiFormName);
            return forms.Length > 0 ? forms[0] : null;
        }

        internal UIForm[] GetUIFormsByInstanceKey(string uiFormName, string instanceKey)
        {
            return FilterUIFormsByInstanceKey(GetUIForms(uiFormName), instanceKey);
        }

        internal UIForm GetTopUIFormByInstanceKey(string uiFormName, string instanceKey)
        {
            UIForm[] forms = GetUIFormsByInstanceKey(uiFormName, instanceKey);
            return forms.Length > 0 ? forms[0] : null;
        }

        #endregion
        
        #region Open, Close, Refocus
        
        internal AsyncOperationHandle<UIForm> OpenUIForm(string uiFormName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return OpenUIForm(new OpenUIFormOptions
            {
                AssetName = uiFormName,
                GroupName = uiGroupName,
                PauseCoveredUIForm = pauseCoveredUIForm,
                OpenPolicy = UIOpenPolicy.SingleInstanceGlobal,
                RefocusIfExists = true
            });
        }

        internal AsyncOperationHandle<UIForm> OpenUIForm(OpenUIFormOptions options)
        {
            if (options == null)
            {
                const string errorString = "OpenUIFormOptions can't be null";
                FrameworkManager.Debugger.LogError(errorString);
                return Addressables.ResourceManager.CreateCompletedOperation<UIForm>(null, errorString);
            }

            string uiFormName = options.AssetName;
            string uiGroupName = options.GroupName;
            bool pauseCoveredUIForm = options.PauseCoveredUIForm;
            bool refocusIfExists = options.RefocusIfExists;
            UIOpenPolicy openPolicy = NormalizeOpenPolicy(options.OpenPolicy);
            string instanceKey = NormalizeInstanceKey(options.InstanceKey);

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

            UIForm openedForm = GetMatchedOpenedUIFormForRequest(uiFormName, uiGroupName, openPolicy, out int matchedOpenedCount);
            if (openedForm != null)
            {
                if (matchedOpenedCount > 1)
                {
                    FrameworkManager.Debugger.LogWarning(
                        $"Open request for UI Form '{uiFormName}' with policy '{openPolicy}' matched {matchedOpenedCount} opened instances. " +
                        $"Request-strategy-priority returns Topmost instance '[{openedForm.SerialID}]{openedForm.UIFormAssetName}' and keeps historical instances unchanged.");
                }

                if (openedForm.UIGroup != uiGroup ||
                    openedForm.PauseCoveredUIForm != pauseCoveredUIForm ||
                    !string.Equals(openedForm.InstanceKey, instanceKey, System.StringComparison.Ordinal))
                {
                    FrameworkManager.Debugger.LogWarning(
                        $"UI Form '{uiFormName}' is already opened for policy '{openPolicy}'. Duplicate open request is deduplicated and current instance context (group/pause/instanceKey) is kept.");
                }

                if (refocusIfExists)
                {
                    RefocusUIForm(openedForm);
                }

                return Addressables.ResourceManager.CreateCompletedOperation(openedForm, null);
            }

            string openingRequestKey = BuildOpeningRequestKey(uiFormName, uiGroupName, openPolicy);
            if (!string.IsNullOrEmpty(openingRequestKey) &&
                openingRequests.TryGetValue(openingRequestKey, out AsyncOperationHandle<UIForm> openingHandle))
            {
                FrameworkManager.Debugger.LogWarning(
                    $"UI Form '{uiFormName}' is opening asynchronously for request key '{openingRequestKey}'. Duplicate open request is deduplicated.");
                return openingHandle;
            }
            
            // Load form from cache.
            if (TryTakeUIFormFromCache(uiFormName, out UIForm uiForm))
            {
                int serialIdForThisOpen = serial++;
                if (!uiForm.PrepareForOpenSession(serialIdForThisOpen, uiGroup, pauseCoveredUIForm, instanceKey, openPolicy))
                {
                    var errorString = $"UI Form '{uiFormName}' from cache failed to prepare open session.";
                    FrameworkManager.Debugger.LogError(errorString);
                    ReleaseUIForm(uiForm);
                    return Addressables.ResourceManager.CreateCompletedOperation<UIForm>(null, errorString);
                }

                uiGroup.AddAndOpenUIForm(uiForm);
                RegisterActiveUIForm(uiForm);
                uiGroup.Refresh();
                return Addressables.ResourceManager.CreateCompletedOperation(uiForm, null);
            }
            // Load form from disk.
            
            var objectHandle = Addressables.LoadAssetAsync<GameObject>(uiFormName);
            var formHandle = Addressables.ResourceManager.CreateChainOperation(objectHandle,
            (operationHandle) =>
            {
                if (operationHandle.Status != AsyncOperationStatus.Succeeded)
                    return Addressables.ResourceManager.CreateCompletedOperation<UIForm>(null, operationHandle.OperationException.Message);
                GameObject uiPrefab = operationHandle.Result;
                GameObject uiObject = Object.Instantiate(uiPrefab);
                UIFormLogic uiFormLogic = uiObject.GetComponent<UIFormLogic>();
                UIForm newForm = new UIForm();
                newForm.OnInit(serial++, uiFormName, uiGroup, pauseCoveredUIForm, uiFormLogic, uiPrefab, instanceKey, openPolicy);
                uiGroup.AddAndOpenUIForm(newForm);
                RegisterActiveUIForm(newForm);
                uiGroup.Refresh();
                return Addressables.ResourceManager.CreateCompletedOperation(newForm, null);
            });

            if (!string.IsNullOrEmpty(openingRequestKey))
            {
                openingRequests[openingRequestKey] = formHandle;
                formHandle.Completed += _ =>
                {
                    if (openingRequests.TryGetValue(openingRequestKey, out AsyncOperationHandle<UIForm> trackedHandle) &&
                        trackedHandle.Equals(formHandle))
                    {
                        openingRequests.Remove(openingRequestKey);
                    }
                };
            }
            return formHandle;
        }
        internal void CloseUIForm(string uiFormName)
        {
            UIForm uiForm = GetUIForm(uiFormName);
            if (uiForm != null)
            {
                CloseUIForm(uiForm.SerialID);
            }
        }

        internal void CloseUIForm(int serialId)
        {
            UIForm uiForm = GetUIForm(serialId);
            if (uiForm != null)
            {
                CloseUIForm(uiForm);
                return;
            }

            FrameworkManager.Debugger.Log($"UI Form serial id {serialId} doesn't exist.");
        }

        internal void CloseUIForm(string uiFormName, string instanceKey)
        {
            UIForm uiForm = GetTopUIFormByInstanceKey(uiFormName, instanceKey);
            if (uiForm != null)
            {
                CloseUIForm(uiForm.SerialID);
                return;
            }

            FrameworkManager.Debugger.Log($"UI Form '{uiFormName}' with instanceKey '{NormalizeInstanceKey(instanceKey) ?? "<null>"}' doesn't exist.");
        }

        internal void CloseAllUIForms(string uiFormName)
        {
            UIForm[] forms = GetUIForms(uiFormName);
            foreach (UIForm uiForm in forms)
            {
                CloseUIForm(uiForm.SerialID);
            }
        }

        internal void CloseAllUIFormsInGroup(string uiFormName, string uiGroupName)
        {
            UIForm[] forms = GetUIForms(uiFormName, uiGroupName);
            foreach (UIForm uiForm in forms)
            {
                CloseUIForm(uiForm.SerialID);
            }
        }

        internal void CloseAllUIFormsByInstanceKey(string uiFormName, string instanceKey)
        {
            UIForm[] forms = GetUIFormsByInstanceKey(uiFormName, instanceKey);
            foreach (UIForm uiForm in forms)
            {
                CloseUIForm(uiForm.SerialID);
            }
        }

        internal void CloseUIForm(UIForm uiForm)
        {
            if (uiForm == null)
            {
                FrameworkManager.Debugger.LogError("UI Form can't be null");
                return;
            }
            if (uiForm.ReleaseTag)
            {
                FrameworkManager.Debugger.LogWarning($"UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}' has already been released.");
                return;
            }
            if (!uiForm.IsOpened)
            {
                FrameworkManager.Debugger.LogWarning($"UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}' is not opened.");
                return;
            }

            UIGroup uiGroup = uiForm.UIGroup;
            if (uiGroup == null)
            {
                FrameworkManager.Debugger.LogError("UI group is invalid.");
                return;
            }
            if (!uiGroup.HasUIForm(uiForm.SerialID))
            {
                FrameworkManager.Debugger.LogError($"UI group '{uiGroup.Name}' does not contain UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}'.");
                return;
            }

            uiGroup.RemoveAndCloseUIForm(uiForm);
            uiGroup.Refresh();

            if (!uiForm.IsOpened)
            {
                UnregisterActiveUIForm(uiForm);
            }

            if (cacheCapacity > 0)
            {
                AddUIFormInCache(uiForm);
            }
            else
            {
                ReleaseUIForm(uiForm);
            }
        }
        internal void RefocusUIForm(string uiFormName)
        {
            UIForm uiForm = GetUIForm(uiFormName);
            if (uiForm != null)
            {
                RefocusUIForm(uiForm.SerialID);
            }
        }

        internal void RefocusUIForm(int serialId)
        {
            UIForm uiForm = GetUIForm(serialId);
            if (uiForm != null)
            {
                RefocusUIForm(uiForm);
                return;
            }

            FrameworkManager.Debugger.Log($"UI Form serial id {serialId} doesn't exist.");
        }

        internal void RefocusUIForm(string uiFormName, string instanceKey)
        {
            UIForm uiForm = GetTopUIFormByInstanceKey(uiFormName, instanceKey);
            if (uiForm != null)
            {
                RefocusUIForm(uiForm.SerialID);
                return;
            }

            FrameworkManager.Debugger.Log($"UI Form '{uiFormName}' with instanceKey '{NormalizeInstanceKey(instanceKey) ?? "<null>"}' doesn't exist.");
        }

        internal void RefocusUIForm(UIForm uiForm)
        {
            if (uiForm == null)
            {
                FrameworkManager.Debugger.LogError("UI form is invalid.");
                return;
            }
            if (uiForm.ReleaseTag)
            {
                FrameworkManager.Debugger.LogWarning($"UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}' has already been released.");
                return;
            }
            if (!uiForm.IsOpened)
            {
                FrameworkManager.Debugger.LogWarning($"UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}' is not opened.");
                return;
            }

            UIGroup uiGroup = uiForm.UIGroup;
            if (uiGroup == null)
            {
                FrameworkManager.Debugger.LogError("UI group is invalid.");
                return;
            }
            if (!uiGroup.HasUIForm(uiForm.SerialID))
            {
                FrameworkManager.Debugger.LogError($"UI group '{uiGroup.Name}' does not contain UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}'.");
                return;
            }

            uiGroup.RefocusUIForm(uiForm);
            uiGroup.Refresh();
            uiForm.OnRefocus();
            MarkUIFormFocused(uiForm);
        }
        internal void CloseAndReleaseAllForms()
        {
            CloseAndReleaseAllFormsInGroups(false);
            ReleaseAllCachedForms();
            ClearActiveUIFormIndices();
        }
        
        #endregion
        
        #endregion
        
        #region UIForm Cache

        private void AddUIFormInCache(UIForm uiForm)
        {
            if (uiForm == null)
            {
                FrameworkManager.Debugger.LogError("UI form is null.");
                return;
            }
            if (uiForm.ReleaseTag)
            {
                FrameworkManager.Debugger.LogWarning($"Can not cache released UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}'.");
                return;
            }
            if (uiForm.IsOpened)
            {
                FrameworkManager.Debugger.LogError($"Can not cache opened UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}'.");
                return;
            }
            if (cacheCapacity <= 0)
            {
                ReleaseUIForm(uiForm);
                return;
            }

            if (InCacheUIForm(uiForm))
            {
                uiFormsCacheList.Remove(uiForm);
                uiFormsCacheList.AddFirst(uiForm);
            }
            else
            {
                uiFormsCacheList.AddFirst(uiForm);
            }

            TrimCacheToCapacity();
        }
        private bool InCacheUIForm(UIForm uiForm)
        {
            return uiFormsCacheList.Contains(uiForm);
        }
        private bool TryTakeUIFormFromCache(string uiFormName, out UIForm uiForm)
        {
            uiForm = null;
            LinkedListNode<UIForm> current = uiFormsCacheList.First;
            while (current != null)
            {
                var tempNode = current.Next;
                UIForm form = current.Value;

                if (form == null)
                {
                    uiFormsCacheList.Remove(current);
                    current = tempNode;
                    continue;
                }
                if (form.ReleaseTag)
                {
                    uiFormsCacheList.Remove(current);
                    current = tempNode;
                    continue;
                }
                if (form.IsOpened)
                {
                    FrameworkManager.Debugger.LogError($"Opened UI form '[{form.SerialID}]{form.UIFormAssetName}' should not stay in cache. Removing stale cache entry.");
                    uiFormsCacheList.Remove(current);
                    current = tempNode;
                    continue;
                }

                if (string.Equals(form.UIFormAssetName, uiFormName, System.StringComparison.Ordinal))
                {
                    uiFormsCacheList.Remove(current);
                    uiForm = form;
                    return true;
                }

                current = tempNode;
            }
            return false;
        }

        private void ReleaseUIForm(UIForm uiForm)
        {
            if (uiForm == null || uiForm.ReleaseTag)
            {
                return;
            }

            uiForm.OnRelease();
        }

        private void ReleaseCachedUIForm(UIForm uiForm)
        {
            if (uiForm == null || uiForm.ReleaseTag)
            {
                return;
            }
            if (uiForm.IsOpened)
            {
                FrameworkManager.Debugger.LogError($"Opened UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}' should not be released from cache. Cache entry has been dropped.");
                return;
            }

            uiForm.OnRelease();
        }

        private void ReleaseAllCachedForms()
        {
            foreach (var uiForm in uiFormsCacheList.ToArray())
            {
                ReleaseCachedUIForm(uiForm);
            }

            uiFormsCacheList.Clear();
        }

        private void CloseAndReleaseAllFormsInGroups(bool isShutdown)
        {
            UIGroup[] uiGroups = uiGroupsDic.Values.ToArray();
            foreach (var uiGroup in uiGroups)
            {
                UIForm[] activeForms = uiGroup.GetAllUIForms();
                uiGroup.RemoveAndCloseAllUIForms(isShutdown);

                foreach (var uiForm in activeForms)
                {
                    UnregisterActiveUIForm(uiForm);
                    ReleaseUIForm(uiForm);
                }
            }
        }

        private bool IsValidActiveUIForm(UIForm uiForm)
        {
            return uiForm != null && !uiForm.ReleaseTag && uiForm.IsOpened;
        }

        private static string NormalizeInstanceKey(string instanceKey)
        {
            return string.IsNullOrEmpty(instanceKey) ? null : instanceKey;
        }

        private UIForm[] FilterUIFormsByInstanceKey(UIForm[] uiForms, string instanceKey)
        {
            if (uiForms == null || uiForms.Length == 0)
            {
                return new UIForm[0];
            }

            string normalizedInstanceKey = NormalizeInstanceKey(instanceKey);
            List<UIForm> matchedForms = new List<UIForm>(uiForms.Length);
            foreach (UIForm uiForm in uiForms)
            {
                if (uiForm == null)
                {
                    continue;
                }

                if (string.Equals(uiForm.InstanceKey, normalizedInstanceKey, System.StringComparison.Ordinal))
                {
                    matchedForms.Add(uiForm);
                }
            }

            return matchedForms.ToArray();
        }

        private UIOpenPolicy NormalizeOpenPolicy(UIOpenPolicy openPolicy)
        {
            switch (openPolicy)
            {
                case UIOpenPolicy.SingleInstanceGlobal:
                case UIOpenPolicy.SingleInstancePerGroup:
                case UIOpenPolicy.MultiInstanceGlobal:
                    return openPolicy;
                default:
                    FrameworkManager.Debugger.LogWarning(
                        $"Unknown UI open policy '{openPolicy}'. Fallback to '{UIOpenPolicy.SingleInstanceGlobal}'.");
                    return UIOpenPolicy.SingleInstanceGlobal;
            }
        }

        private string BuildOpeningRequestKey(string uiFormName, string uiGroupName, UIOpenPolicy openPolicy)
        {
            switch (openPolicy)
            {
                case UIOpenPolicy.SingleInstanceGlobal:
                    return $"SingleGlobal|{uiFormName}";
                case UIOpenPolicy.SingleInstancePerGroup:
                    return $"SinglePerGroup|{uiFormName}|{uiGroupName}";
                case UIOpenPolicy.MultiInstanceGlobal:
                    // Multi-instance requests should not be deduplicated while opening.
                    return null;
                default:
                    return $"SingleGlobal|{uiFormName}";
            }
        }

        private UIForm GetMatchedOpenedUIFormForRequest(string uiFormName, string uiGroupName, UIOpenPolicy openPolicy, out int matchedCount)
        {
            UIForm[] matchedForms;
            switch (openPolicy)
            {
                case UIOpenPolicy.SingleInstancePerGroup:
                    matchedForms = GetUIForms(uiFormName, uiGroupName);
                    break;
                case UIOpenPolicy.MultiInstanceGlobal:
                    matchedCount = 0;
                    return null;
                case UIOpenPolicy.SingleInstanceGlobal:
                default:
                    matchedForms = GetUIForms(uiFormName);
                    break;
            }

            matchedCount = matchedForms.Length;
            return matchedCount > 0 ? matchedForms[0] : null;
        }

        private void RegisterActiveUIForm(UIForm uiForm)
        {
            if (!IsValidActiveUIForm(uiForm))
            {
                return;
            }

            activeFormsBySerial[uiForm.SerialID] = uiForm;

            if (!string.IsNullOrEmpty(uiForm.UIFormAssetName))
            {
                if (!activeSerialsByAsset.TryGetValue(uiForm.UIFormAssetName, out HashSet<int> serialsByAsset))
                {
                    serialsByAsset = new HashSet<int>();
                    activeSerialsByAsset.Add(uiForm.UIFormAssetName, serialsByAsset);
                }
                serialsByAsset.Add(uiForm.SerialID);

                string uiGroupName = uiForm.UIGroup?.Name;
                if (!string.IsNullOrEmpty(uiGroupName))
                {
                    if (!activeSerialsByAssetAndGroup.TryGetValue(uiForm.UIFormAssetName, out Dictionary<string, HashSet<int>> serialsByGroup))
                    {
                        serialsByGroup = new Dictionary<string, HashSet<int>>();
                        activeSerialsByAssetAndGroup.Add(uiForm.UIFormAssetName, serialsByGroup);
                    }
                    if (!serialsByGroup.TryGetValue(uiGroupName, out HashSet<int> serials))
                    {
                        serials = new HashSet<int>();
                        serialsByGroup.Add(uiGroupName, serials);
                    }
                    serials.Add(uiForm.SerialID);
                }
            }

            MarkUIFormFocused(uiForm);
        }

        private void UnregisterActiveUIForm(UIForm uiForm)
        {
            if (uiForm == null)
            {
                return;
            }

            activeFormsBySerial.Remove(uiForm.SerialID);

            if (!string.IsNullOrEmpty(uiForm.UIFormAssetName))
            {
                if (activeSerialsByAsset.TryGetValue(uiForm.UIFormAssetName, out HashSet<int> serialsByAsset))
                {
                    serialsByAsset.Remove(uiForm.SerialID);
                    if (serialsByAsset.Count == 0)
                    {
                        activeSerialsByAsset.Remove(uiForm.UIFormAssetName);
                    }
                }

                string uiGroupName = uiForm.UIGroup?.Name;
                if (!string.IsNullOrEmpty(uiGroupName) &&
                    activeSerialsByAssetAndGroup.TryGetValue(uiForm.UIFormAssetName, out Dictionary<string, HashSet<int>> serialsByGroup))
                {
                    if (serialsByGroup.TryGetValue(uiGroupName, out HashSet<int> serials))
                    {
                        serials.Remove(uiForm.SerialID);
                        if (serials.Count == 0)
                        {
                            serialsByGroup.Remove(uiGroupName);
                        }
                    }

                    if (serialsByGroup.Count == 0)
                    {
                        activeSerialsByAssetAndGroup.Remove(uiForm.UIFormAssetName);
                    }
                }
            }
        }

        private void MarkUIFormFocused(UIForm uiForm)
        {
            if (uiForm == null)
            {
                return;
            }

            uiForm.SetLastFocusSequence(++focusSequence);
        }

        private UIForm[] GetUIFormsCore(string uiFormName, string uiGroupName)
        {
            if (string.IsNullOrEmpty(uiFormName))
            {
                return new UIForm[0];
            }

            IEnumerable<int> serialIds = null;
            if (string.IsNullOrEmpty(uiGroupName))
            {
                if (activeSerialsByAsset.TryGetValue(uiFormName, out HashSet<int> serialsByAsset))
                {
                    serialIds = serialsByAsset;
                }
            }
            else if (activeSerialsByAssetAndGroup.TryGetValue(uiFormName, out Dictionary<string, HashSet<int>> serialsByGroup) &&
                     serialsByGroup.TryGetValue(uiGroupName, out HashSet<int> serialsByAssetAndGroup))
            {
                serialIds = serialsByAssetAndGroup;
            }

            if (serialIds == null)
            {
                return new UIForm[0];
            }

            List<UIForm> results = new List<UIForm>();
            foreach (int serialId in serialIds)
            {
                if (!activeFormsBySerial.TryGetValue(serialId, out UIForm uiForm) || !IsValidActiveUIForm(uiForm))
                {
                    continue;
                }
                if (!string.Equals(uiForm.UIFormAssetName, uiFormName, System.StringComparison.Ordinal))
                {
                    continue;
                }
                if (!string.IsNullOrEmpty(uiGroupName) &&
                    (uiForm.UIGroup == null || !string.Equals(uiForm.UIGroup.Name, uiGroupName, System.StringComparison.Ordinal)))
                {
                    continue;
                }

                results.Add(uiForm);
            }

            return results
                .OrderByDescending(form => form.LastFocusSequence)
                .ThenByDescending(form => form.SerialID)
                .ToArray();
        }

        private void ClearActiveUIFormIndices()
        {
            activeFormsBySerial.Clear();
            activeSerialsByAsset.Clear();
            activeSerialsByAssetAndGroup.Clear();
        }
        
        #endregion
        
        #region private lifecycle

        private void Update()
        {
            // Update opened forms.
            UIGroup[] uiGroups = uiGroupsDic.Values.ToArray();
            foreach (var uiGroup in uiGroups)
            {
                uiGroup.Update();
            }
        }
        private void ShutDown()
        {   
            CloseAndReleaseAllFormsInGroups(true);
            uiGroupsDic.Clear();
            ReleaseAllCachedForms();
            ClearActiveUIFormIndices();
            openingRequests.Clear();
        }
        
        
        #endregion
    
    
    }
}


