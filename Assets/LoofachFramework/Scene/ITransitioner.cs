using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 场景切换者接口
/// 场景切换者用于完成场景切换逻辑和切换场景的动画效果
/// </summary>
public interface ITransitioner
{
    public void AddtiveTrans(string from, string to, float beforeTime, float afterTime);
    public void AddtiveTrans(int from, int to, float beforeTime, float afterTime);
    public void SingleTrans(string to, float beforeTime, float afterTime);
    public void SingleTrans(int to, float beforeTime, float afterTime);
}
