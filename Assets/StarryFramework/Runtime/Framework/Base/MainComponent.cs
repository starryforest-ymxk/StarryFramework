using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class MainComponent : MonoBehaviour
    {
        [Header("Unity Setting")]
        [Space(10)]
        [SerializeField]
        [Range(0, 120)]
        private int frameRate = 30;

        [SerializeField]
        [Range(0,10)]
        private float gameSpeed = 1f;

        [SerializeField]
        private bool runInBackground = true;

        [SerializeField]
        private bool neverSleep = true;

        [Header("Framework Setting")]
        [Space(10)]
        [SerializeField]
        private FrameworkSettings frameworkSetting = new FrameworkSettings();






        #region �������

        private void Awake()
        {
            FrameworkManager.BeforeAwake();

            frameworkSetting.Init();

            frameworkSetting.SettingCheck();

            UnitySetup();
            
            FrameworkManager.RegisterSetting(frameworkSetting);

            SetComponentsActive();

            Application.quitting += SceneUnload;

            FrameworkManager.Awake();
        }

        private IEnumerator Start()
        {

            FrameworkManager.Init();

            yield return null;

            FrameworkManager.AfterInit();
        }

        private void Update()
        {
            FrameworkManager.Update();
        }

        private void SceneUnload()
        {
            // ��ȡ��������
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;

            bool hasDontDestroy = false;

            // ����ж�س���
            for (int i = sceneCount - 1; i >= 0; i--)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.name != "DontDestroyOnLoad" && scene.name != "GameFramework")
                {
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
                }
                if(scene.name == "DontDestroyOnLoad")
                {
                    hasDontDestroy = true;
                }
            }
            if(hasDontDestroy)
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("DontDestroyOnLoad");
        }

        private void OnDestroy()
        {
            if (FrameworkManager.FrameworkState != FrameworkState.ShutDown)
            {
                FrameworkComponent.Shutdown(ShutdownType.None);
            }

            Shutdown();
        }

        private void Shutdown()
        {

        }

        #endregion

        /// <summary>
        /// Unity�༭������
        /// </summary>
        private void UnitySetup()
        {
            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        }

        /// <summary>
        /// �������
        /// </summary>
        private void SetComponentsActive()
        {
            BaseComponent[] components = gameObject.GetComponentsInChildren<BaseComponent>();
            foreach(BaseComponent component in components)
            {
                try
                {
                    ModuleType type = (ModuleType)Enum.Parse(typeof(ModuleType), component.gameObject.name);

                    if (!frameworkSetting.modules.Contains(type))
                    {
                        FrameworkManager.Debugger.Log($"Unused module: {type}");
                        component.gameObject.SetActive(false);
                        component.DisableProcess();
                    }
                }
                catch
                {
                    Debug.LogError("The Name of component gameObject can not be modified.");
                }
            }
        }
    }
}
