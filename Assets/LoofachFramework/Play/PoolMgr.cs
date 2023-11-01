using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 对象池，存取需要在场景中反复出现和销毁的物品，降低物品生成开销
/// </summary>
public class PoolMgr : MonoSingleton<PoolMgr>
{
    // 对象字典，每一个对象名对应一个栈    
    Dictionary<string, Stack<GameObject>> pool = new Dictionary<string, Stack<GameObject>>();
    //对象池游戏物体，对象池中的物体会在禁用后成为这个物体的子物体
    GameObject poolObj = null;
    protected override void Awake()
    {
        base.Awake();
        //每次切换场景都需要清空对象池，防止空引用报错
        EventMgr.GetInstance().AddEventListener(EventDic.BeforeChangeScene, () => { Clear(); });
    }
    /// <summary>
    /// 通过资源路径从对象池中获取游戏物体(若池中没有则创建新物体)
    /// </summary>
    /// <param Name="pathName">物体在对象池中的路径名</param>
    /// <returns>路径名对应的对象池中物品</returns>
    public GameObject PopOutPool(string pathName)
    {
        GameObject obj = null;
        if (pool.ContainsKey(pathName) && pool[pathName].Count > 0)
        {
            obj = pool[pathName].Pop();
        }
        else
        {
            obj = ResMgr.GetInstance().LoadRes<GameObject>(pathName);
        }
        obj.name = pathName;
        obj.SetActive(true);
        return obj;
    }
    /// <summary>
    /// 通过预制体从对象池中获取游戏物体(若池中没有则创建新物体)
    /// </summary>
    /// <param Name="obj">预制体</param>
    /// <returns>预制体名对应的对象池中物品</returns>
    public GameObject PopOutPool(GameObject obj)
    {
        if (pool.ContainsKey(obj.name) && pool[obj.name].Count > 0)
        {
            obj = pool[obj.name].Pop();
        }
        else
        {
            obj = GameObject.Instantiate(obj);
        }
        obj.SetActive(true);
        return obj;
    }

    /// <summary>
    /// 将游戏物体放入对象池中
    /// </summary>
    /// <param Name="pathName">物体在对象池中的路径</param>
    /// <param Name="obj">需要放入的物体</param>
    public void PushInPool(string pathName, GameObject obj)
    {
        if (poolObj == null)
        { poolObj = new GameObject(); }
        obj.SetActive(false);
        if (pool.ContainsKey(pathName))
        {
            pool[pathName].Push(obj);
        }
        else
        {
            Stack<GameObject> s = new Stack<GameObject>();
            s.Push(obj);
            pool.Add(pathName, s);
        }
        obj.transform.parent = poolObj.transform;
    }
    /// <summary>
    /// 将游戏物体放入对象池中
    /// </summary>    
    /// <param Name="obj">需要放入的物体</param>
    public void PushInPool(GameObject obj)
    {
        if (poolObj == null)
        { poolObj = new GameObject(); }
        obj.SetActive(false);
        if (pool.ContainsKey(obj.name))
        {
            pool[obj.name].Push(obj);
        }
        else
        {
            Stack<GameObject> s = new Stack<GameObject>();
            s.Push(obj);
            pool.Add(obj.name, s);
        }
        obj.transform.parent = poolObj.transform;
    }
    /// <summary>
    /// 删除对象池并清空字典
    /// </summary>
    public void Clear()
    {
        Destroy(poolObj);
        poolObj = null;
        pool.Clear();
    }


}
