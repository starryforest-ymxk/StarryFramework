using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    public static class Utilities
    {
        private static MainComponent _component;
        private static MainComponent component
        {
            get
            {
                if (_component == null)
                {
                    _component = GameObject.FindGameObjectWithTag("GameFramework").GetComponent<MainComponent>();
                    if (_component == null)
                        _component = GameObject.Find("GameFramework").GetComponent<MainComponent>();
                }
                return _component;
            }
        }

        /// <summary>
        /// 延时调用，采用协程实现
        /// </summary>
        /// <param Name="time">延时时间，秒为单位</param>
        /// <param Name="unityAction">延时调用的函数</param>
        public static Coroutine DelayInvoke(float time, UnityAction unityAction)
        {
            return component.StartCoroutine(invoke());
            IEnumerator invoke()
            {
                yield return new WaitForSeconds(time);
                unityAction.Invoke();
            }
        }

        /// <summary>
        /// 根据条件触发调用，采用协程实现
        /// </summary>
        /// <param Name="condition">调用条件</param>
        /// <param Name="unityAction">触发调用的函数</param>
        public static Coroutine ConditionallyInvoke(Func<bool> condition, UnityAction unityAction)
        {
            return component.StartCoroutine(invoke());
            IEnumerator invoke()
            {
                yield return new WaitUntil(condition);
                unityAction.Invoke();
            }
        }
        
        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="coroutine">需要停止的协程</param>
        public static void StopCoroutine(Coroutine coroutine)
        {
            component.StopCoroutine(coroutine);
        }
        
        /// <summary>
        /// 停止所有协程
        /// </summary>
        public static void StopAllCoroutines()
        {
            component.StopAllCoroutines();
        }

        /// <summary>
        /// 异步延时调用
        /// </summary>
        /// <param Name="time">延时时间，秒为单位</param>
        /// <param Name="unityAction">延时调用的函数</param>
        public static void AsyncDelayInvoke(float time, UnityAction unityAction)
        {
            System.Timers.Timer timer = new System.Timers.Timer(time * 1000);
            timer.AutoReset = false;
            timer.Elapsed += (s, e) =>
            {
                unityAction.Invoke();
                timer.Close();
            };
            timer.Start();
        }

        /// <summary>
        /// 字典筛选器，输入字典并给定筛选规则，返回筛选后的字典
        /// </summary>
        /// <typeparam Name="T1">字典键类型</typeparam>
        /// <typeparam Name="T2">字典值类型</typeparam>
        /// <param Name="dic">输入字典</param>
        /// <param Name="filter">筛选器，用于筛选字典所有键值对，符合则保留，不符合则舍弃</param>
        /// <param Name="action">回调函数，舍弃不符合的键值对时调用</param>
        /// <returns>经过筛选后的字典</returns>
        public static Dictionary<T1, T2> DictionaryFilter<T1, T2>(Dictionary<T1, T2> dic, Func<T1, T2, bool> filter, Action<T1, T2> action = null)
        {
            List<T1> keyslist = dic.Keys.ToList();
            List<T2> valueslist = dic.Values.ToList();
            for (int i = 0; i < dic.Count; i++)
            {
                if (!filter(keyslist[i], valueslist[i]))
                {
                    action?.Invoke(keyslist[i], valueslist[i]);
                    dic.Remove(keyslist[i]);
                }
            }
            return dic;
        }

        /// <summary>
        /// 输入场景路径，返回场景名
        /// </summary>
        /// <param Name="scenePath">场景路径</param>
        /// <returns>场景名</returns>
        public static string ScenePathToName(string scenePath)
        {

            string[] scenePathSplit = { "/", ".unity" };

            string[] splitPath = scenePath.Split(scenePathSplit, System.StringSplitOptions.RemoveEmptyEntries);

            if (splitPath.Length > 0)
            {
                return splitPath[splitPath.Length - 1];
            }
            
            return null;

        }
    }
}

