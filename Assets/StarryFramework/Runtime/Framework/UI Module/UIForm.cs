using UnityEngine;

namespace StarryFramework
{
    /// <summary>
    /// UI窗体类，负责管理UI窗体的生命周期和状态
    /// </summary>
    public class UIForm
    {
        private int serialID;
        private string uiFormAssetName;
        private UIGroup uiGroup;
        private int depthInUIGroup;
        private bool pauseCoveredUiForm;
        private UIFormLogic uiFormLogic;
        // private GameObject objectHandle;
        // private GameObject uiObject;
        private bool releaseTag;
        private bool isOpened;
        
        /// <summary>
        /// UI窗体序列号
        /// </summary>
        public int SerialID => serialID;
        
        /// <summary>
        /// UI窗体资源名称
        /// </summary>
        public string UIFormAssetName => uiFormAssetName;
        
        // public GameObject ObjectHandle => objectHandle;
        // public GameObject UIObject => uiObject;
        
        /// <summary>
        /// UI窗体所属的UI组
        /// </summary>
        public UIGroup UIGroup => uiGroup;
        
        /// <summary>
        /// UI窗体在UI组中的深度，0代表位于最底层
        /// </summary>
        public int DepthInUIGroup => depthInUIGroup;
        
        /// <summary>
        /// 是否暂停被该窗体覆盖的其他UI窗体
        /// </summary>
        public bool PauseCoveredUIForm => pauseCoveredUiForm;
        
        /// <summary>
        /// UI窗体逻辑接口
        /// </summary>
        public UIFormLogic UIFormLogic => uiFormLogic;
        
        /// <summary>
        /// 是否已被释放
        /// </summary>
        public bool ReleaseTag => releaseTag;
        
        /// <summary>
        /// 是否已打开
        /// </summary>
        public bool IsOpened => isOpened;
        
#if UNITY_EDITOR
        public bool Foldout { get; set; }
        public bool FoldoutInCache { get; set; }
#endif
        
        
        
        #region 生命周期

        /// <summary>
        /// 初始化UI窗体
        /// </summary>
        public void OnInit(
            int serialId, string assetName, UIGroup group, bool pauseCoveredUIForm, 
            UIFormLogic logic, GameObject handle/*, GameObject @object*/)
        {
            serialID = serialId;
            uiFormAssetName = assetName;
            uiGroup = group;
            pauseCoveredUiForm = pauseCoveredUIForm;
            uiFormLogic = logic;
            // objectHandle = handle;
            // uiObject = @object;
            releaseTag = false;
            if(uiFormLogic != null)
                uiFormLogic.OnInit(handle);
            else
                FrameworkManager.Debugger.LogError("uiFormLogic is null.");
            
        }
        
        /// <summary>
        /// 释放UI窗体资源
        /// </summary>
        public void OnRelease()
        {
            if (releaseTag)
            {
                FrameworkManager.Debugger.LogWarning("uiForm has already been released.");
                return;
            }
            if(uiFormLogic != null)
                uiFormLogic.OnRelease();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null.");
            // if(uiObject != null)
            //     Object.Destroy(uiObject);
            // Addressables.Release(objectHandle);
            releaseTag = true;
        }
        
        /// <summary>
        /// 打开UI窗体
        /// </summary>
        public void OnOpen()
        {
            isOpened = true;
            if(uiFormLogic != null)
                uiFormLogic.OnOpen();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        
        /// <summary>
        /// 关闭UI窗体
        /// </summary>
        /// <param name="isShutdown">是否为框架关闭时调用</param>
        public void OnClose(bool isShutdown)
        {
            isOpened = false;
            if(uiFormLogic != null)
                uiFormLogic.OnClose(isShutdown);
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        
        /// <summary>
        /// 覆盖UI窗体
        /// </summary>
        public void OnCover()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnCover();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");

        }
        
        /// <summary>
        /// 显示UI窗体（从覆盖状态恢复）
        /// </summary>
        public void OnReveal()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnReveal();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        
        /// <summary>
        /// 暂停UI窗体
        /// </summary>
        public void OnPause()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnPause();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        
        /// <summary>
        /// 恢复UI窗体
        /// </summary>
        public void OnResume()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnResume();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        
        /// <summary>
        /// 更新UI窗体
        /// </summary>
        public void OnUpdate()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnUpdate();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        
        /// <summary>
        /// UI窗体深度改变时调用
        /// </summary>
        /// <param name="formCountInUIGroup">UI组中UI窗体数量</param>
        /// <param name="newDepthInUIGroup">新的深度值</param>
        public void OnDepthChanged(int formCountInUIGroup, int newDepthInUIGroup)
        {
            depthInUIGroup = newDepthInUIGroup;
            if(uiFormLogic != null)
                uiFormLogic.OnDepthChanged(formCountInUIGroup, newDepthInUIGroup);
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        
        /// <summary>
        /// 重新聚焦UI窗体
        /// </summary>
        public void OnRefocus()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnRefocus();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        

        #endregion

    }
}


