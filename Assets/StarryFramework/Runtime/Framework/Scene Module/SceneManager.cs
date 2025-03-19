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
        /// ж�ص�ǰ�����
        /// </summary>
        /// <param Name="callback">ж�����ʱ�Ļص�����</param>
        internal void UnloadScene(UnityAction callback = null)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);

            operation.completed += (asyncOperation) => callback?.Invoke();
        }

        /// <summary>
        /// ͨ������ж�س���
        /// </summary>
        /// <param Name="sceneIndex">�Ѽ��س�����buildIndex</param>
        /// <param Name="callback">ж�����ʱ�Ļص�����</param>
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
        /// ͨ����������ж�س���
        /// </summary>
        /// <param name="sceneName">��ж�س�����</param>
        /// <param name="callback">ж�����ʱ�Ļص�����</param>
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
        /// ͨ���������س���
        /// </summary>
        /// <param Name="buildIndex">������buildIndex</param>
        /// <param Name="callback">������ɵĻص�</param>
        /// <returns>���س���AsyncOperation</returns>
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
        /// ͨ���������Ƽ��س���
        /// </summary>
        /// <param Name="sceneName">��������</param>
        /// <param Name="callback">������ɵĻص�</param>
        /// <returns>���س���AsyncOperation</returns>
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
        /// ͨ�������л�����
        /// </summary>
        /// <param Name="to">Ŀ�ĳ���buildIndex</param>
        /// <param Name="from">��ж�صĳ���buildIndex�����Ϊ-1����ж�ص�ǰ�����</param>
        /// <param Name="callback">�л�������ɵĻص�</param>
        /// <returns>���س���AsyncOperation</returns>
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
        /// ͨ�����������л�����
        /// </summary>
        /// <param Name="to">Ŀ�ĳ�������</param>
        /// <param Name="from">��ж�صĳ������ƣ����Ϊ�գ���ж�ص�ǰ�����</param>
        /// <param Name="callback">�л�������ɵĻص�</param>
        /// <returns>���س���AsyncOperation</returns>
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
