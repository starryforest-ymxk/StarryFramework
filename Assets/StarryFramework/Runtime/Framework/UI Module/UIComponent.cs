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
        
        [SerializeField] private UISettings settings = new();
        
        public Dictionary<string, UIGroup> UIGroupsDic => Manager.uiGroupsDic;
        public LinkedList<UIForm> UIFormsCacheList => Manager.uiFormsCacheList;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            settings ??= new UISettings();
            if(EditorApplication.isPlaying && _manager != null)
                (_manager as IConfigurableManager)?.SetSettings(settings);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            settings ??= new UISettings();
            _manager ??= FrameworkManager.GetManager<UIManager>();
            (_manager as IConfigurableManager)?.SetSettings(settings);
        }
        
        
        #region UIGroup

        /// <summary>
        /// 检查是否存在指定名称的UI组
        /// </summary>
        /// <param name="uiGroupName">UI组名称</param>
        /// <returns>是否存在该UI组</returns>
        public bool HasUIGroup(string uiGroupName)
        {
            return Manager.HasUIGroup(uiGroupName);
        }
        
        /// <summary>
        /// 获取指定名称的UI组
        /// </summary>
        /// <param name="uiGroupName">UI组名称</param>
        /// <returns>UI组对象</returns>
        public UIGroup GetUIGroup(string uiGroupName)
        {
            return Manager.GetUIGroup(uiGroupName);
        }

        /// <summary>
        /// 获取所有UI组
        /// </summary>
        /// <returns>所有UI组的数组</returns>
        public UIGroup[] GetAllUIGroups()
        {
            return Manager.GetAllUIGroups();
        }
        
        /// <summary>
        /// 添加UI组
        /// </summary>
        /// <param name="uiGroupName">UI组名称</param>
        public void AddUIGroup(string uiGroupName)
        {
            Manager.AddUIGroup(uiGroupName);
        }

        /// <summary>
        /// 移除UI组
        /// </summary>
        /// <param name="uiGroupName">UI组名称</param>
        public void RemoveUIGroup(string uiGroupName)
        {
            Manager.RemoveUIGroup(uiGroupName);
        }
        
        
        #endregion
        
        #region UIForm

        /// <summary>
        /// 检查是否存在指定资源名的UI窗体
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>是否存在该UI窗体</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            return Manager.HasUIForm(uiFormAssetName);
        }
        
        /// <summary>
        /// 获取指定资源名的UI窗体
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>UI窗体对象</returns>
        public UIForm GetUIForm(string uiFormAssetName)
        {
            return Manager.GetUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 打开UI窗体
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="uiGroupName">UI组名称</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的UI窗体</param>
        /// <returns>异步操作句柄</returns>
        public AsyncOperationHandle<UIForm> OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return Manager.OpenUIForm(uiFormAssetName, uiGroupName, pauseCoveredUIForm);
        }

        /// <summary>
        /// 通过资源名称关闭UI窗体
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        public void CloseUIForm(string uiFormAssetName)
        {
            Manager.CloseUIForm(uiFormAssetName);
        }
        
        /// <summary>
        /// 关闭UI窗体
        /// </summary>
        /// <param name="uiForm">UI窗体对象</param>
        public void CloseUIForm(UIForm uiForm)
        {
            Manager.CloseUIForm(uiForm);
        }

        /// <summary>
        /// 通过资源名称重新聚焦UI窗体
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        public void RefocusUIForm(string uiFormAssetName)
        {
            Manager.RefocusUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 重新聚焦UI窗体
        /// </summary>
        /// <param name="uiForm">UI窗体对象</param>
        public void RefocusUIForm(UIForm uiForm)
        {
            Manager.RefocusUIForm(uiForm);
        }
        
        #endregion
        
        /// <summary>
        /// 关闭并释放所有UI窗体
        /// </summary>
        public void CloseAndReleaseAllForms()
        {
            Manager.CloseAndReleaseAllForms();
        }
        
    }
}

