using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class UIComponent : ConfigurableComponent
    {
        private UIManager _manager;
        private UIManager Manager => _manager ??= FrameworkManager.GetManager<UIManager>();
        
        [SerializeField] private UISettings settings = new();
        
        /// <summary>
        /// UI组只读视图（运行时状态）。
        /// </summary>
        public IReadOnlyDictionary<string, UIGroup> UIGroups => Manager.UIGroupsReadOnlyView;

        /// <summary>
        /// UI窗体缓存快照（只读）。
        /// </summary>
        public IReadOnlyList<UIForm> UIFormsCacheSnapshot => Manager.GetUIFormsCacheSnapshot();

        /// <summary>
        /// Opening request count snapshot (for runtime diagnostics/inspector).
        /// </summary>
        public int OpeningRequestCount => Manager.OpeningRequestCount;

        /// <summary>
        /// Active form count snapshot (for runtime diagnostics/inspector).
        /// </summary>
        public int ActiveFormCount => Manager.ActiveFormCount;

        /// <summary>
        /// Active asset key count snapshot (for runtime diagnostics/inspector).
        /// </summary>
        public int ActiveAssetKeyCount => Manager.ActiveAssetKeyCount;

        /// <summary>
        /// Active UI form snapshot ordered by topmost priority.
        /// </summary>
        public UIForm[] GetAllActiveUIFormsSnapshot()
        {
            return Manager.GetAllActiveUIFormsSnapshot();
        }

        /// <summary>
        /// Opening request key snapshot (for runtime diagnostics/inspector).
        /// </summary>
        public string[] GetOpeningRequestKeysSnapshot()
        {
            return Manager.GetOpeningRequestKeysSnapshot();
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            HotApplyConfigurableSettingsInPlayMode(_manager, ref settings);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            ResolveAndApplyConfigurableSettings(ref _manager, ref settings, FrameworkManager.GetManager<UIManager>);
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
        /// 检查是否存在指定资源名的活跃UI窗体实例（任意 InstanceKey）。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>是否存在该UI窗体</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            return Manager.HasUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 检查是否存在指定资源名且匹配 InstanceKey 的活跃UI窗体实例。
        /// InstanceKey 使用 Ordinal（大小写敏感）匹配，null/empty 视为未打标实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="instanceKey">业务实例标识键</param>
        /// <returns>是否存在匹配实例</returns>
        public bool HasUIForm(string uiFormAssetName, string instanceKey)
        {
            return Manager.HasUIForm(uiFormAssetName, instanceKey);
        }

        /// <summary>
        /// 检查是否存在指定序列号的UI窗体实例。
        /// </summary>
        /// <param name="serialId">UI窗体实例序列号</param>
        /// <returns>是否存在该实例</returns>
        public bool HasUIForm(int serialId)
        {
            return Manager.HasUIForm(serialId);
        }
        
        /// <summary>
        /// 获取指定资源名当前最上层（Topmost）的活跃UI窗体实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>UI窗体对象，未找到返回 null</returns>
        [Obsolete("Use GetTopUIForm(string) for topmost semantics, GetUIForms(string) for all instances, or GetUIForm(int) for exact instance targeting.", false)]
        public UIForm GetUIForm(string uiFormAssetName)
        {
            return Manager.GetUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 获取指定序列号的UI窗体实例。
        /// </summary>
        /// <param name="serialId">UI窗体实例序列号</param>
        /// <returns>UI窗体实例，未找到返回 null</returns>
        public UIForm GetUIForm(int serialId)
        {
            return Manager.GetUIForm(serialId);
        }

        /// <summary>
        /// 获取指定资源名且匹配 InstanceKey 的当前最上层（Topmost）活跃实例。
        /// InstanceKey 使用 Ordinal（大小写敏感）匹配，null/empty 视为未打标实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="instanceKey">业务实例标识键</param>
        /// <returns>UI窗体对象，未找到返回 null</returns>
        public UIForm GetUIForm(string uiFormAssetName, string instanceKey)
        {
            return Manager.GetUIForm(uiFormAssetName, instanceKey);
        }

        /// <summary>
        /// 获取指定资源名的所有活跃UI窗体实例（按 Topmost 优先排序）。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>UI窗体实例数组</returns>
        public UIForm[] GetUIForms(string uiFormAssetName)
        {
            return Manager.GetUIForms(uiFormAssetName);
        }

        /// <summary>
        /// 获取指定资源名在指定UI组中的所有活跃UI窗体实例（按 Topmost 优先排序）。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="uiGroupName">UI组名称</param>
        /// <returns>UI窗体实例数组</returns>
        public UIForm[] GetUIForms(string uiFormAssetName, string uiGroupName)
        {
            return Manager.GetUIForms(uiFormAssetName, uiGroupName);
        }

        /// <summary>
        /// 获取指定资源名且匹配 InstanceKey 的所有活跃UI窗体实例（按 Topmost 优先排序）。
        /// InstanceKey 使用 Ordinal（大小写敏感）匹配，null/empty 视为未打标实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="instanceKey">业务实例标识键</param>
        /// <returns>UI窗体实例数组</returns>
        public UIForm[] GetUIFormsByInstanceKey(string uiFormAssetName, string instanceKey)
        {
            return Manager.GetUIFormsByInstanceKey(uiFormAssetName, instanceKey);
        }

        /// <summary>
        /// 获取指定资源名的活跃UI窗体实例数量。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>实例数量</returns>
        public int GetUIFormCount(string uiFormAssetName)
        {
            return Manager.GetUIFormCount(uiFormAssetName);
        }

        /// <summary>
        /// 获取指定资源名当前最上层（最近打开/聚焦）的活跃实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <returns>最上层实例，未找到返回 null</returns>
        public UIForm GetTopUIForm(string uiFormAssetName)
        {
            return Manager.GetTopUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 按打开选项打开UI窗体。
        /// 打开行为由请求策略（OpenPolicy）和 InstanceKey 决定，支持全局/分组单实例与全局多实例。
        /// 单实例策略命中已存在实例时返回该实例（可由 RefocusIfExists 控制是否自动聚焦）。
        /// </summary>
        /// <param name="options">打开参数</param>
        /// <returns>异步操作句柄</returns>
        public AsyncOperationHandle<UIForm> OpenUIForm(OpenUIFormOptions options)
        {
            return Manager.OpenUIForm(options);
        }

        [Obsolete("Use OpenUIForm(OpenUIFormOptions) to specify UIOpenPolicy and InstanceKey explicitly.", false)]
        public AsyncOperationHandle<UIForm> OpenUIForm(string uiFormAssetName, string uiGroupName, bool pauseCoveredUIForm)
        {
            return OpenUIForm(new OpenUIFormOptions
            {
                AssetName = uiFormAssetName,
                GroupName = uiGroupName,
                PauseCoveredUIForm = pauseCoveredUIForm,
                OpenPolicy = UIOpenPolicy.SingleInstanceGlobal,
                RefocusIfExists = true
            });
        }

        /// <summary>
        /// 通过资源名称关闭UI窗体。
        /// 旧语义：关闭该资源名的 Topmost 活跃实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        [Obsolete("Use CloseUIForm(int serialId), CloseAllUIForms(string), or CloseUIForm(string assetName, string instanceKey). This overload closes the topmost instance only.", false)]
        public void CloseUIForm(string uiFormAssetName)
        {
            Manager.CloseUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 通过序列号关闭指定UI窗体实例。
        /// </summary>
        /// <param name="serialId">UI窗体实例序列号</param>
        public void CloseUIForm(int serialId)
        {
            Manager.CloseUIForm(serialId);
        }

        /// <summary>
        /// 通过资源名 + InstanceKey 关闭UI窗体。
        /// 语义：关闭匹配条件的 Topmost 活跃实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="instanceKey">业务实例标识键</param>
        public void CloseUIForm(string uiFormAssetName, string instanceKey)
        {
            Manager.CloseUIForm(uiFormAssetName, instanceKey);
        }

        /// <summary>
        /// 关闭指定资源名的所有活跃UI窗体实例（跨所有UI组）。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        public void CloseAllUIForms(string uiFormAssetName)
        {
            Manager.CloseAllUIForms(uiFormAssetName);
        }

        /// <summary>
        /// 关闭指定资源名在指定UI组中的所有活跃UI窗体实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="uiGroupName">UI组名称</param>
        public void CloseAllUIFormsInGroup(string uiFormAssetName, string uiGroupName)
        {
            Manager.CloseAllUIFormsInGroup(uiFormAssetName, uiGroupName);
        }

        /// <summary>
        /// 关闭指定资源名下匹配 InstanceKey 的所有活跃UI窗体实例。
        /// InstanceKey 使用 Ordinal（大小写敏感）匹配，null/empty 视为未打标实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="instanceKey">业务实例标识键</param>
        public void CloseAllUIFormsByInstanceKey(string uiFormAssetName, string instanceKey)
        {
            Manager.CloseAllUIFormsByInstanceKey(uiFormAssetName, instanceKey);
        }
        
        /// <summary>
        /// 关闭UI窗体
        /// </summary>
        /// <param name="uiForm">UI窗体对象</param>
        [Obsolete("Use CloseUIForm(int serialId) to avoid stale object references when forms are reused from cache.", false)]
        public void CloseUIForm(UIForm uiForm)
        {
            Manager.CloseUIForm(uiForm);
        }

        /// <summary>
        /// 通过资源名称重新聚焦UI窗体。
        /// 旧语义：聚焦该资源名的 Topmost 活跃实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        [Obsolete("Use RefocusUIForm(int serialId) or RefocusUIForm(string assetName, string instanceKey). This overload refocuses the topmost instance only.", false)]
        public void RefocusUIForm(string uiFormAssetName)
        {
            Manager.RefocusUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 通过序列号重新聚焦指定UI窗体实例。
        /// </summary>
        /// <param name="serialId">UI窗体实例序列号</param>
        public void RefocusUIForm(int serialId)
        {
            Manager.RefocusUIForm(serialId);
        }

        /// <summary>
        /// 通过资源名 + InstanceKey 重新聚焦UI窗体。
        /// 语义：聚焦匹配条件的 Topmost 活跃实例。
        /// </summary>
        /// <param name="uiFormAssetName">UI窗体资源名称</param>
        /// <param name="instanceKey">业务实例标识键</param>
        public void RefocusUIForm(string uiFormAssetName, string instanceKey)
        {
            Manager.RefocusUIForm(uiFormAssetName, instanceKey);
        }

        /// <summary>
        /// 重新聚焦UI窗体
        /// </summary>
        /// <param name="uiForm">UI窗体对象</param>
        [Obsolete("Use RefocusUIForm(int serialId) to avoid stale object references when forms are reused from cache.", false)]
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

