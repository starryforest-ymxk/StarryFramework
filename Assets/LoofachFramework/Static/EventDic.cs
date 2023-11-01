/// <summary>
/// 记录事件系统的事件名
/// 为了防止参数的个数被记错，最好在定义字符串的时候在开头写出参数个数
/// </summary>
public static class EventDic
{
    #region 基本功能事件
    public const string BeforeChangeScene = "0_BeforeChangeScene";
    public const string AfterChangeScene = "0_AfterChangeScene";
    public const string OnEnterMainGame = "0_OnStartMainGame";      //进入主游戏
    public const string OnLeaveMainGame = "0_OnEndMainGame";        //退出主游戏
    public const string BeforeEventSave = "0_BeforeEventSave";
    #endregion
}
