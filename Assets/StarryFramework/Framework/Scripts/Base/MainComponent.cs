using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public class MainComponent : MonoBehaviour
    {
        [Header("Unity Setting")]
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
        [SerializeField]
        private FrameworkSettings frameworkSetting = new FrameworkSettings();






        #region 组件流程

        protected void Awake()
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
            // 获取场景数量
            int sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;

            bool hasDontDestroy = false;

            // 逆序卸载场景
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
            if (FrameworkManager.frameworkState != FrameworkState.ShutDown)
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
        /// Unity编辑器设置
        /// </summary>
        private void UnitySetup()
        {
            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
        }

        /// <summary>
        /// 启用组件
        /// </summary>
        private void SetComponentsActive()
        {
            Component[] components  = gameObject.GetComponentsInChildren<BaseComponent>();
            foreach(BaseComponent component in components)
            {
                try
                {
                    if(!component.Equals(this))
                    {
                        ModuleType type = (ModuleType)Enum.Parse(typeof(ModuleType), component.gameObject.name);

                        if (!frameworkSetting.modules.Contains(type))
                        {
                            component.DisableProcess();
                            component.gameObject.SetActive(false);
                        }
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
