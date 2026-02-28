using System;

namespace StarryFramework
{
    public interface ISaveDataProvider
    {
        /// <summary>
        /// 返回玩家存档模型类型。
        /// </summary>
        Type PlayerDataType { get; }

        /// <summary>
        /// 返回游戏设置模型类型。
        /// </summary>
        Type GameSettingsType { get; }

        /// <summary>
        /// 创建玩家存档默认实例。
        /// </summary>
        object CreateDefaultPlayerData();

        /// <summary>
        /// 创建游戏设置默认实例。
        /// </summary>
        object CreateDefaultGameSettings();
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class SaveDataProviderAttribute : Attribute
    {
        public int Priority { get; }

        /// <summary>
        /// 标记一个可被自动发现的存档数据提供器。
        /// </summary>
        /// <param name="priority">优先级，值越大优先级越高。</param>
        public SaveDataProviderAttribute(int priority = 0)
        {
            Priority = priority;
        }
    }
}
