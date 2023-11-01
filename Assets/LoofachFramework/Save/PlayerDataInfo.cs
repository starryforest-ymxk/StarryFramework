using System;
/// <summary>
/// 玩家存档的辨识信息
/// 玩家通过这些信息辨识出需要读取的存档
/// </summary>
public class PlayerDataInfo
{
    public PlayerDataInfo(string note)
    {
        time = DateTime.Now.ToString("G");
        this.note = note;
    }
    public string time;
    public string note;
}
