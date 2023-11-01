using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Events;
/// <summary>
/// 基本的黑色渐变场景切换者
/// </summary>
public sealed class BlackTransitioner : MonoBehaviour, ITransitioner
{
    private CanvasGroup group;
    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        group = GetComponent<CanvasGroup>();

    }
    /// <summary>
    /// 直接加载场景
    /// </summary>    
    /// <param Name="to">切换场景后的场景名</param>
    /// <param Name="beforeTime">切换场景前Transitioner的停留时间</param>
    /// <param Name="afterTime">切换场景后Transitioner的停留时间</param>
    public void SingleTrans(string to, float beforeTime, float afterTime)
    {
        StartCoroutine(RealSingleTrans(to, beforeTime, afterTime));
        IEnumerator RealSingleTrans(string to, float beforeTime, float afterTime)
        {
            WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
            WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
            DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
            yield return beforeW;
            EventMgr.GetInstance().InvokeEvent(EventDic.BeforeChangeScene);
            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Single);
            DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
            EventMgr.GetInstance().InvokeEvent(EventDic.AfterChangeScene);
            yield return afterW;
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// 直接加载场景
    /// </summary>    
    /// <param Name="to">切换场景后的场景序号</param>
    /// <param Name="beforeTime">切换场景前Transitioner的停留时间</param>
    /// <param Name="afterTime">切换场景后Transitioner的停留时间</param>
    public void SingleTrans(int to, float beforeTime, float afterTime)
    {
        StartCoroutine(RealSingleTrans(to, beforeTime, afterTime));
        IEnumerator RealSingleTrans(int to, float beforeTime, float afterTime)
        {
            WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
            WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
            DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
            yield return beforeW;
            EventMgr.GetInstance().InvokeEvent(EventDic.BeforeChangeScene);
            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Single);
            DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
            EventMgr.GetInstance().InvokeEvent(EventDic.AfterChangeScene);
            yield return afterW;
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// 叠加加载场景
    /// </summary>
    /// <param Name="from">切换场景前的场景名</param>
    /// <param Name="to">切换场景后的场景名</param>
    /// <param Name="beforeTime">切换场景前Transitioner的停留时间</param>
    /// <param Name="afterTime">切换场景后Transitioner的停留时间</param>
    public void AddtiveTrans(string from, string to, float beforeTime, float afterTime)
    {
        StartCoroutine(RealAddtiveTrans(from, to, beforeTime, afterTime));
        IEnumerator RealAddtiveTrans(string from, string to, float beforeTime, float afterTime)
        {
            WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
            WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(beforeTime);
            DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
            yield return beforeW;
            EventMgr.GetInstance().InvokeEvent(EventDic.BeforeChangeScene);
            yield return SceneManager.UnloadSceneAsync(from);
            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
            DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
            EventMgr.GetInstance().InvokeEvent(EventDic.AfterChangeScene);
            yield return afterW;
            Destroy(this.gameObject);
        }
    }
    /// <summary>
    /// 叠加加载场景
    /// </summary>
    /// <param Name="from">切换场景前的场景序号</param>
    /// <param Name="to">切换场景后的场景序号</param>
    /// <param Name="beforeTime">切换场景前Transitioner的停留时间</param>
    /// <param Name="afterTime">切换场景后Transitioner的停留时间</param>
    public void AddtiveTrans(int from, int to, float beforeTime, float afterTime)
    {
        StartCoroutine(RealAddtiveTrans(from, to, beforeTime, afterTime));
        IEnumerator RealAddtiveTrans(int from, int to, float beforeTime, float afterTime)
        {
            WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
            WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(beforeTime);
            DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
            yield return beforeW;
            EventMgr.GetInstance().InvokeEvent(EventDic.BeforeChangeScene);
            yield return SceneManager.UnloadSceneAsync(from);
            yield return SceneManager.LoadSceneAsync(to, LoadSceneMode.Additive);
            DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
            EventMgr.GetInstance().InvokeEvent(EventDic.AfterChangeScene);
            yield return afterW;
            Destroy(this.gameObject);
        }
    }
}
