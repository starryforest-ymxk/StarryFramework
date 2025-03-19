using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace StarryFramework
{
    internal class SceneManager : IManager
    {
        void IManager.Awake() { }
        void IManager.Init() { }
        void IManager.Update() { }
        void IManager.ShutDown() { }
        void IManager.SetSettings(IManagerSettings settings) { }


        #region Unload

        /// <summary>
        /// 卸载当前活动场景
        /// </summary>
        /// <param Name="callback">卸载完成时的回调函数</param>
        internal void UnloadScene(UnityAction callback = null)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);

            operation.completed += (asyncOperation) => callback?.Invoke();
        }

        /// <summary>
        /// 通过索引卸载场景
        /// </summary>
        /// <param Name="sceneIndex">已加载场景的buildIndex</param>
        /// <param Name="callback">卸载完成时的回调函数</param>
        internal void UnloadScene(int sceneIndex, UnityAction callback = null)
        {
            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneIndex);

            if (operation != null)
            {
                operation.completed += (asyncOperation) => callback?.Invoke();
            }
            else
            {
                FrameworkManager.Debugger.LogError("Wrong index of scene to unload.");
            }
        }

        /// <summary>
        /// 通过场景名称卸载场景
        /// </summary>
        /// <param name="sceneName">待卸载场景名</param>
        /// <param name="callback">卸载完成时的回调函数</param>
        internal void UnloadScene(string sceneName, UnityAction callback = null)
        {
            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);

            if (operation != null)
            {
                operation.completed += (asyncOperation) => callback?.Invoke();
            }
            else
            {
                FrameworkManager.Debugger.LogError("Scene to unload is not loaded or invalid.");
            }
        }

        #endregion

        #region Load

        /// <summary>
        /// 通过索引加载场景
        /// </summary>
        /// <param Name="buildIndex">场景的buildIndex</param>
        /// <param Name="callback">加载完成的回调</param>
        /// <returns>加载场景AsyncOperation</returns>
        internal AsyncOperation LoadScene(int buildIndex, UnityAction callback = null)
        {
            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);

            if (operation != null)
            {
                operation.completed += (asyncOperation) => callback?.Invoke();

                return operation;
            }
            else
            {
                FrameworkManager.Debugger.LogError("Wrong index of scene to load.");

                return null;
            }
        }

        /// <summary>
        /// 通过场景名称加载场景
        /// </summary>
        /// <param Name="sceneName">场景名称</param>
        /// <param Name="callback">加载完成的回调</param>
        /// <returns>加载场景AsyncOperation</returns>
        internal AsyncOperation LoadScene(string sceneName, UnityAction callback = null)
        {

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (operation != null)
            {
                operation.completed += (asyncOperation) => callback?.Invoke();

                return operation;
            }
            else
            {
                FrameworkManager.Debugger.LogError("Invalid scene Name.");

                return null;
            }
        }

        #endregion

        #region Change

        /// <summary>
        /// 通过索引切换场景
        /// </summary>
        /// <param Name="to">目的场景buildIndex</param>
        /// <param Name="from">待卸载的场景buildIndex，如果为-1，则卸载当前激活场景</param>
        /// <param Name="callback">切换场景完成的回调</param>
        /// <returns>加载场景AsyncOperation</returns>
        internal AsyncOperation ChangeScene(int to, int from = -1, UnityAction callback = null)
        {
            if (from == -1)
            {
                UnloadScene();
            }
            else
            {
                UnloadScene(from);
            }

            return LoadScene(to, callback);
        }

        /// <summary>
        /// 通过场景名称切换场景
        /// </summary>
        /// <param Name="to">目的场景名称</param>
        /// <param Name="from">待卸载的场景名称，如果为空，则卸载当前激活场景</param>
        /// <param Name="callback">切换场景完成的回调</param>
        /// <returns>加载场景AsyncOperation</returns>
        internal AsyncOperation ChangeScene(string to, string from = "", UnityAction callback = null)
        {
            if (from == "")
            {
                UnloadScene();
            }
            else
            {
                UnloadScene(from);
            }

            return LoadScene(to, callback);
        }


        #endregion


    }

}
