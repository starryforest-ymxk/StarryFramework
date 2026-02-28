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
        private string instanceKey;
        private UIOpenPolicy openPolicy;
        private UIGroup uiGroup;
        private int depthInUIGroup;
        private bool pauseCoveredUiForm;
        private UIFormLogic uiFormLogic;
        private bool releaseTag;
        private bool isOpened;
        private long lastFocusSequence;
        
        /// <summary>
        /// UI窗体序列号
        /// </summary>
        public int SerialID => serialID;
        
        /// <summary>
        /// UI窗体资源名称
        /// </summary>
        public string UIFormAssetName => uiFormAssetName;

        /// <summary>
        /// 业务层实例标识键（多实例模式使用）。
        /// </summary>
        public string InstanceKey => instanceKey;

        /// <summary>
        /// 本次打开会话采用的打开策略。
        /// </summary>
        public UIOpenPolicy OpenPolicy => openPolicy;
        
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

        /// <summary>
        /// 最近一次打开/聚焦的顺序号（用于 Topmost 判定）。
        /// </summary>
        public long LastFocusSequence => lastFocusSequence;
        
#if UNITY_EDITOR
        public bool Foldout { get; set; }
        public bool FoldoutInCache { get; set; }
#endif
        
        /// <summary>
        /// 绑定本次打开上下文（所属组、覆盖暂停策略）。
        /// 用于缓存复用时更新运行时上下文，不触发资源初始化逻辑。
        /// </summary>
        internal void BindOpenContext(UIGroup group, bool pauseCoveredUIForm)
        {
            BindOpenContext(group, pauseCoveredUIForm, null, UIOpenPolicy.SingleInstanceGlobal);
        }

        /// <summary>
        /// 绑定本次打开上下文（所属组、覆盖暂停策略、业务实例标识）。
        /// 用于缓存复用时更新运行时上下文，不触发资源初始化逻辑。
        /// </summary>
        internal void BindOpenContext(UIGroup group, bool pauseCoveredUIForm, string newInstanceKey)
        {
            BindOpenContext(group, pauseCoveredUIForm, newInstanceKey, UIOpenPolicy.SingleInstanceGlobal);
        }

        /// <summary>
        /// 绑定本次打开上下文（所属组、覆盖暂停策略、业务实例标识、打开策略）。
        /// 用于缓存复用时更新运行时上下文，不触发资源初始化逻辑。
        /// </summary>
        internal void BindOpenContext(UIGroup group, bool pauseCoveredUIForm, string newInstanceKey, UIOpenPolicy newOpenPolicy)
        {
            if (group == null)
            {
                FrameworkManager.Debugger.LogError("UI group is invalid.");
                return;
            }

            uiGroup = group;
            pauseCoveredUiForm = pauseCoveredUIForm;
            instanceKey = string.IsNullOrEmpty(newInstanceKey) ? null : newInstanceKey;
            openPolicy = newOpenPolicy;
        }

        /// <summary>
        /// Prepare one open session for this form (serial id is session-scoped and reallocated on every open).
        /// </summary>
        internal bool PrepareForOpenSession(int newSerialId, UIGroup group, bool pauseCoveredUIForm, string newInstanceKey, UIOpenPolicy newOpenPolicy)
        {
            if (releaseTag)
            {
                FrameworkManager.Debugger.LogError("Can not prepare an already released uiForm.");
                return false;
            }

            if (isOpened)
            {
                FrameworkManager.Debugger.LogError("Can not prepare an already opened uiForm.");
                return false;
            }

            serialID = newSerialId;
            BindOpenContext(group, pauseCoveredUIForm, newInstanceKey, newOpenPolicy);
            lastFocusSequence = 0;
            return true;
        }

        /// <summary>
        /// 更新最近一次聚焦顺序号（由 UIManager 维护）。
        /// </summary>
        internal void SetLastFocusSequence(long sequence)
        {
            lastFocusSequence = sequence;
        }
        
        
        #region 生命周期

        /// <summary>
        /// 初始化UI窗体
        /// </summary>
        public void OnInit(
            int serialId, string assetName, UIGroup group, bool pauseCoveredUIForm, 
            UIFormLogic logic, GameObject handle, string newInstanceKey = null,
            UIOpenPolicy newOpenPolicy = UIOpenPolicy.SingleInstanceGlobal)
        {
            uiFormAssetName = assetName;
            if (!PrepareForOpenSession(serialId, group, pauseCoveredUIForm, newInstanceKey, newOpenPolicy))
            {
                return;
            }
            uiFormLogic = logic;
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
            isOpened = false;
            if(uiFormLogic != null)
                uiFormLogic.OnRelease();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null.");
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


