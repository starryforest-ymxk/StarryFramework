using System.Collections.Generic;
/// <summary>
/// 玩家的存档文件
/// 一切需要在读取存档时获取的信息都要写在这里(不包括打开游戏即获取的音量等设置内容)
/// 信息对外的接口则需要在GameManager中编写
/// </summary>
public sealed class PlayerData
{
    #region 角色数据    
    #endregion
    #region 事件数据
    public Triggers triggers = new Triggers();
    #endregion
    #region 场景数据
    public Dictionary<string, List<string>> bgmDic = null;
    #endregion
}
