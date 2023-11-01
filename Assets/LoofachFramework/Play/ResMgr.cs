using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResMgr : Singleton<ResMgr>
{
    /// <summary>
    /// 同步加载一个资源
    /// </summary>
    /// <typeparam Name="T">资源的类型</typeparam>
    /// <param Name="path">资源在Resources文件夹下完整路径名</param>
    /// <returns>如果相应资源为Gameobjet,则生成并返回物体；如果不是，则直接返回物体</returns>
    public T LoadRes<T>(string path) where T : Object
    {
        T res = Resources.Load<T>(path);
        if (res is GameObject)
        {
            return GameObject.Instantiate(res);
        }
        else
        {
            return res;
        }
    }
    /// <summary>
    /// 同步加载路径下所有资源
    /// </summary>
    /// <typeparam Name="T">资源的类型</typeparam>
    /// <param Name="path">路径</param>
    /// <returns>加载的资源数组</returns>
    public T[] LoadAllRes<T>(string path) where T : Object
    {
        T[] res = Resources.LoadAll<T>(path);
        return res;
    }



    /// <summary>
    /// 异步加载资源协程
    /// </summary>
    /// <typeparam Name="T">资源类型</typeparam>
    /// <param Name="name">资源在Resources文件夹下完整路径名</param>
    /// <param Name="callBack">处理资源的函数，即加载资源要做的事情,有唯一的参数为所获取物品</param>
    /// <returns></returns>
    private IEnumerator ReallyLoadAsync<T>(string name, UnityAction<T> callBack) where T : Object
    {
        ResourceRequest r = Resources.LoadAsync<T>(name);
        yield return r;
        //先进行资源的异步加载
        if (r.asset is GameObject)
        {
            callBack(GameObject.Instantiate(r.asset) as T);
        }
        else
        {
            callBack(r.asset as T);
        }
    }
    /// <summary>
    /// 异步加载资源
    /// </summary>
    /// <typeparam Name="T">加载资源类型</typeparam>
    /// <param Name="name">加载资源文件名</param>
    /// <param Name="callBack">以所加载资源为唯一参数的回调函数</param>
    public void AsyncLoad<T>(string name, UnityAction<T> callBack) where T : Object
    {
        MonoMgr.GetInstance().StartCoroutine(ReallyLoadAsync<T>(name, callBack));//开启异步加载协程
    }

}
