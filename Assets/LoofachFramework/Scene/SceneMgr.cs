using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 场景管理器，Mono，挂载在GameManager
/// 用于完成场景的两种异步加载
/// 切换场景时的视觉效果取决于使用的Transitioner种类
/// </summary>
public class SceneMgr : MonoSingleton<SceneMgr>
{
    private Dictionary<string, GameObject> transDic = new Dictionary<string, GameObject>();
    protected override void Awake()
    {
        base.Awake();
        GameObject[] g = ResMgr.GetInstance().LoadAllRes<GameObject>("Prefab/Transitioner");
        foreach (var trans in g)
        {
            transDic.Add(trans.GetComponent<ITransitioner>().GetType().ToString(), trans);
        }
    }
    #region 场景切换
    /// <summary>
    /// 直接加载场景
    /// </summary>
    /// <param Name="transitionerType">场景切换物体的类型</param>    
    /// <param Name="to">切换后的场景名</param>
    /// <param Name="beforeTime">切换场景前Transitioner停留的时间</param>
    /// <param Name="afterTime">切换场景后Transitioner停留的时间</param>
    public void SingleTP(string to, string transitionerType = nameof(BlackTransitioner), float beforeTime = GameConstant.DefaultVisualFaderTime, float afterTime = GameConstant.DefaultVisualFaderTime)
    {
        if (!transDic.ContainsKey(transitionerType))
        {
            Debug.LogError("Error transitioner type!");
            return;
        }
        var g = GameObject.Instantiate(transDic[transitionerType]);
        g.GetComponent<ITransitioner>().SingleTrans(to, beforeTime, afterTime);
    }
    /// <summary>
    /// 直接加载场景
    /// </summary>
    /// <param Name="transitionerType">场景切换物体的类型</param>    
    /// <param Name="to">切换后的场景序号</param>
    /// <param Name="beforeTime">切换场景前Transitioner停留的时间</param>
    /// <param Name="afterTime">切换场景后Transitioner停留的时间</param>     
    public void SingleTP(int to, string transitionerType = nameof(BlackTransitioner), float beforeTime = GameConstant.DefaultVisualFaderTime, float afterTime = GameConstant.DefaultVisualFaderTime)
    {
        if (!transDic.ContainsKey(transitionerType))
        {
            Debug.LogError("Error transitioner type!");
            return;
        }
        var g = GameObject.Instantiate(transDic[transitionerType]);
        g.GetComponent<ITransitioner>().SingleTrans(to, beforeTime, afterTime);
    }


    /// <summary>
    /// 叠加加载场景
    /// </summary>
    /// <param Name="transitionerType">场景切换物体的类型</param>
    /// <param Name="from">切换前的场景名</param>
    /// <param Name="to">切换后的场景名</param>
    /// <param Name="beforeTime">切换场景前Transitioner停留的时间</param>
    /// <param Name="afterTime">切换场景后Transitioner停留的时间</param>
    public void AddtiveTP(string from, string to, string transitionerType = nameof(BlackTransitioner), float beforeTime = GameConstant.DefaultVisualFaderTime, float afterTime = GameConstant.DefaultVisualFaderTime)
    {
        if (!transDic.ContainsKey(transitionerType))
        {
            Debug.LogError("Error transitioner type!");
            return;
        }
        var g = GameObject.Instantiate(transDic[transitionerType]);
        g.GetComponent<ITransitioner>().AddtiveTrans(from, to, beforeTime, afterTime);
    }
    /// <summary>
    /// 叠加加载场景
    /// </summary>
    /// <param Name="transitionerType">场景切换物体的类型</param>
    /// <param Name="from">切换前的场景序号</param>
    /// <param Name="to">切换后的场景序号</param>
    /// <param Name="beforeTime">切换场景前Transitioner停留的时间</param>
    /// <param Name="afterTime">切换场景后Transitioner停留的时间</param>
    public void AddtiveTP(int from, int to, string transitionerType = nameof(BlackTransitioner), float beforeTime = GameConstant.DefaultVisualFaderTime, float afterTime = GameConstant.DefaultVisualFaderTime)
    {
        if (!transDic.ContainsKey(transitionerType))
        {
            Debug.LogError("Error transitioner type!");
            return;
        }
        var g = GameObject.Instantiate(transDic[transitionerType]);
        g.GetComponent<ITransitioner>().AddtiveTrans(from, to, beforeTime, afterTime);
    }
    #endregion
    public string GetCurrentSceneName() => SceneManager.GetActiveScene().name;    
}
