using UnityEngine;
using System.Collections;
using DG.Tweening;
/// <summary>
/// 自定义拓展方法
/// </summary>
public static class CustomExtension
{
    /// <summary>
    /// 让CanvasGroup显示并启用交互和阻挡
    /// </summary>    
    /// <param Name="time">渐变到显示的用时</param>
    public static void SetOn(this CanvasGroup group, bool realTime = true, float time = GameConstant.DefaultVisualFaderTime)
    {
        group.blocksRaycasts = true;
        group.interactable = true;
        group.DOFade(1, time).SetUpdate(realTime);
    }
    /// <summary>
    /// 让CanvasGroup隐藏并禁用交互和阻挡
    /// </summary>    
    /// <param Name="time">渐变到消失的用时</param>
    public static void SetOff(this CanvasGroup group, bool realTime = true, float time = GameConstant.DefaultVisualFaderTime)
    {
        group.blocksRaycasts = false;
        group.interactable = false;
        group.DOFade(0, time).SetUpdate(realTime);
    }    
}
