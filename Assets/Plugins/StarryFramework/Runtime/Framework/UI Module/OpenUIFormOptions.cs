namespace StarryFramework
{
    /// <summary>
    /// UI 窗体打开参数（方案 B：选项对象）。
    /// PR1 阶段先引入类型定义，后续阶段再接入打开流程。
    /// </summary>
    public sealed class OpenUIFormOptions
    {
        /// <summary>
        /// UI 窗体资源名称（Addressables key / 资源标识）。
        /// </summary>
        public string AssetName;

        /// <summary>
        /// UIGroup 名称。
        /// </summary>
        public string GroupName;

        /// <summary>
        /// 是否暂停被覆盖的 UI 窗体。
        /// </summary>
        public bool PauseCoveredUIForm = true;

        /// <summary>
        /// 打开策略。默认保持兼容：同资源名全局单实例。
        /// </summary>
        public UIOpenPolicy OpenPolicy = UIOpenPolicy.SingleInstanceGlobal;

        /// <summary>
        /// 单实例策略命中已存在实例时是否自动聚焦（后续阶段接入）。
        /// </summary>
        public bool RefocusIfExists = true;

        /// <summary>
        /// 业务层实例标识键（后续阶段接入）。
        /// </summary>
        public string InstanceKey;
    }
}

