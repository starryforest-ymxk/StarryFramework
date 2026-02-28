namespace StarryFramework
{
    /// <summary>
    /// UI 窗体打开策略（用于未来多实例模式）。
    /// 当前默认兼容行为为 SingleInstanceGlobal。
    /// </summary>
    public enum UIOpenPolicy
    {
        /// <summary>
        /// 同资源名全局单实例。
        /// </summary>
        SingleInstanceGlobal = 0,

        /// <summary>
        /// 同资源名在同一 UIGroup 内单实例，不同组可各有一个实例。
        /// </summary>
        SingleInstancePerGroup = 1,

        /// <summary>
        /// 同资源名全局多实例（包括同组多开）。
        /// </summary>
        MultiInstanceGlobal = 2
    }
}

