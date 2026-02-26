using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    /// <summary>
    /// UI组类，用于管理一组UI窗体的层级和状态
    /// </summary>
    public class UIGroup
    {
        private string name;
        private int formCount;
        private bool pause;
        private readonly LinkedList<UIFormInfo> formInfosList;
        
        /// <summary>
        /// UI组名称
        /// </summary>
        public string Name => name;
        
        /// <summary>
        /// UI组中窗体数量
        /// </summary>
        public int FormCount => formCount;
        
#if UNITY_EDITOR

        private bool _foldout = false;
        public bool Foldout { get => _foldout; set => _foldout = value; }
        
        public LinkedList<UIFormInfo> FormInfosList => formInfosList;
#endif
        
        /// <summary>
        /// 是否暂停UI组中的所有窗体
        /// </summary>
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

        /// <summary>
        /// 当前显示的UI窗体（位于最上层）
        /// </summary>
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

        /// <summary>
        /// 检查是否存在指定序列号的UI窗体
        /// </summary>
        /// <param name="serialId">UI窗体序列号</param>
        /// <returns>是否存在该UI窗体</returns>
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
        
        /// <summary>
        /// 检查是否存在指定资源名的UI窗体
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>是否存在该UI窗体</returns>
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
        
        /// <summary>
        /// 通过序列号获取UI窗体
        /// </summary>
        /// <param name="serialId">UI窗体序列号</param>
        /// <returns>UI窗体对象，未找到返回null</returns>
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
        
        /// <summary>
        /// 通过资源名称获取UI窗体
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>UI窗体对象，未找到返回null</returns>
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
        
        /// <summary>
        /// 获取UI组中的所有UI窗体
        /// </summary>
        /// <returns>UI窗体数组</returns>
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
        internal void AddAndOpenUIForm(UIForm uiForm)
        {
            formCount++;
            formInfosList.AddFirst(new UIFormInfo(uiForm));
            DepthRefresh();
            uiForm.OnOpen();
        }
        internal void RemoveAndCloseUIForm(UIForm uiForm)
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
                
                uiForm.OnClose(false);
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
        internal void RemoveAndCloseAllUIForms(bool isShutdown)
        {
            foreach (UIFormInfo uiFormInfo in formInfosList)
            {
                uiFormInfo.UIForm.OnClose(isShutdown);
            }
            formInfosList.Clear();
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
            RemoveAndCloseAllUIForms(true);
        }
        
    }
}

