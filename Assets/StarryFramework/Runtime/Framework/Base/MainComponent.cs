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

        private FrameworkSettings FrameworkSetting
        {
            get
            {
                return FrameworkSettings.Instance;
            }
        }






        #region 组件流程

        private void Awake()
        {
            FrameworkManager.BeforeAwake();

            if (FrameworkSetting == null)
            {
                Debug.LogError("FrameworkSettings is null! Please create a FrameworkSettings asset.");
                return;
            }

            FrameworkSetting.Init();
            FrameworkSetting.SettingCheck();

            UnitySetup();
            
            FrameworkManager.RegisterSetting(FrameworkSetting);

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
            BaseComponent[] components = gameObject.GetComponentsInChildren<BaseComponent>();
            foreach(BaseComponent component in components)
            {
                if (!FrameworkManager.TryGetModuleType(component, out ModuleType type))
                {
                    FrameworkManager.Debugger.LogError($"Module component type is not registered: {component.GetType().FullName}");
                    continue;
                }

                if (!FrameworkSetting.modules.Contains(type))
                {
                    FrameworkManager.Debugger.Log($"Unused module: {type}");
                    component.gameObject.SetActive(false);
                    component.DisableProcess();
                }
            }
        }
    }
}
