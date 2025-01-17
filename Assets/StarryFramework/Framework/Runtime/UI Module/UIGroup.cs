using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public class UIGroup
    {
        private string name;
        private int formCount;
        private bool pause;
        private readonly LinkedList<UIFormInfo> formInfosList;
        
        public string Name => name;
        public int FormCount => formCount;
        
#if UNITY_EDITOR

        private bool _foldout = false;
        public bool Foldout { get => _foldout; set => _foldout = value; }
        
        public LinkedList<UIFormInfo> FormInfosList => formInfosList;
#endif
        
        public bool Pause
        {
            get => pause;
            set
            {
                if(pause == value) return;
                pause = value;
                Refresh();
            }
        }

        public UIForm CurrentForm => formInfosList.First?.Value.UIForm;
        
        internal UIGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                FrameworkManager.Debugger.LogError("UI group name is invalid.");
            }

            this.name = name;
            pause = false;
            formInfosList = new LinkedList<UIFormInfo>();
            formCount = 0;
        }
        
        internal void Update()
        {
            //显式迭代，避免游戏逻辑导致foreach内部增删节点
            LinkedListNode<UIFormInfo> current = formInfosList.First;
            while (current != null)
            {
                if (current.Value.Paused)
                {
                    break;
                }

                var tempNode = current.Next;
                current.Value.UIForm.OnUpdate();
                current = tempNode;
            }
        }

        #region Has, Get

        public bool HasUIForm(int serialId)
        {
            foreach (UIFormInfo uiFormInfo in formInfosList)
            {
                if (uiFormInfo.UIForm.SerialID == serialId)
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                FrameworkManager.Debugger.LogError("UI form asset name is invalid.");
            }

            foreach (UIFormInfo uiFormInfo in formInfosList)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return true;
                }
            }

            return false;
        }
        
        public UIForm GetUIForm(int serialId)
        {
            foreach (UIFormInfo uiFormInfo in formInfosList)
            {
                if (uiFormInfo.UIForm.SerialID == serialId)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }
        
        public UIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                FrameworkManager.Debugger.LogError("UI form asset name is invalid.");
            }

            foreach (UIFormInfo uiFormInfo in formInfosList)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }
        
        public UIForm[] GetAllUIForms()
        {
            List<UIForm> results = new List<UIForm>();
            foreach (UIFormInfo uiFormInfo in formInfosList)
            {
                results.Add(uiFormInfo.UIForm);
            }

            return results.ToArray();
        }
        
        private UIFormInfo GetUIFormInfo(UIForm uiForm)
        {
            if (uiForm == null)
            {
                FrameworkManager.Debugger.LogError("UI form is null.");
            }

            foreach (UIFormInfo uiFormInfo in formInfosList)
            {
                if (uiFormInfo.UIForm == uiForm)
                {
                    return uiFormInfo;
                }
            }

            return null;
        }

        #endregion
        
        #region Add, Remove, Refocus
        internal void AddUIForm(UIForm uiForm)
        {
            formCount++;
            formInfosList.AddFirst(new UIFormInfo(uiForm));
            DepthRefresh();
        }
        internal void RemoveUIForm(UIForm uiForm)
        {
            UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                FrameworkManager.Debugger.LogError($"Can not find UI form info for serial id '{uiForm.SerialID}', UI form asset name is '{uiForm.UIFormAssetName}'.");
                return;
            }
            
            if (!uiFormInfo.Covered)
            {
                uiFormInfo.Covered = true;
                uiForm.OnCover();
            }
            
            if (!uiFormInfo.Paused)
            {
                uiFormInfo.Paused = true;
                uiForm.OnPause();
            }

            if (formInfosList.Remove(uiFormInfo))
            {
                formCount--;
                DepthRefresh();
                return;
            }
            
            FrameworkManager.Debugger.LogError($"UI group '{name}' not exists specified UI form '[{uiForm.SerialID}]{uiForm.UIFormAssetName}'.");
        }
        internal void RefocusUIForm(UIForm uiForm)
        {
            UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                FrameworkManager.Debugger.LogError($"Can not find UI form info for serial id '{uiForm.SerialID}', UI form asset name is '{uiForm.UIFormAssetName}'.");
                return;
            }

            formInfosList.Remove(uiFormInfo);
            formInfosList.AddFirst(uiFormInfo);
            DepthRefresh();
        }
        
        #endregion
        
        #region Refresh
        private void DepthRefresh()
        {
            int currentDepth = formCount;
            
            LinkedListNode<UIFormInfo> current = formInfosList.First;
            while (current != null)
            {
                var tempNode = current.Next;

                currentDepth--;
                if (current.Value.UIForm != null && current.Value.UIForm.DepthInUIGroup != currentDepth)
                {
                    current.Value.UIForm.OnDepthChanged(formCount, currentDepth);
                }
                
                current = tempNode;
            }
        }
        internal void Refresh()
        {
            bool currentPause = pause;
            bool currentCover = false;
            
            LinkedListNode<UIFormInfo> current = formInfosList.First;
            while (current != null)
            {
                var tempNode = current.Next;
                
                if (currentPause)
                {
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        current.Value.UIForm.OnCover();
                    }
                    
                    if (!current.Value.Paused)
                    {
                        current.Value.Paused = true;
                        current.Value.UIForm.OnPause();
                    }
                }
                else
                {
                    if (current.Value.Paused)
                    {
                        current.Value.Paused = false;
                        current.Value.UIForm.OnResume();
                    }
                    if (current.Value.UIForm.PauseCoveredUIForm)
                    {
                        currentPause = true;
                    }

                    if (currentCover)
                    {
                        if (!current.Value.Covered)
                        {
                            current.Value.Covered = true;
                            current.Value.UIForm.OnCover();
                        }
                    }
                    else
                    {
                        if (current.Value.Covered)
                        {
                            current.Value.Covered = false;
                            current.Value.UIForm.OnReveal();
                        }
                        currentCover = true;
                    }
                }
                
                current = tempNode;
            }
        }
        
        #endregion

        internal void ShutDown()
        {
            foreach (var uiFormInfo in formInfosList)
            {
                uiFormInfo.UIForm.OnClose(true);
                uiFormInfo.UIForm.OnRelease();
            }
            formInfosList.Clear();
        }
        
    }
}

