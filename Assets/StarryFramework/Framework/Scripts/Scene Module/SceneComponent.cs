using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public sealed class SceneComponent: BaseComponent
    {

        private SceneManager _manager = null;

        private SceneManager manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = FrameworkManager.GetManager<SceneManager>();
                }
                return _manager;
            }
        }

        private Camera mainCamera;

        private int sceneIndex;

        private float sceneLoadedTime;

        private float sceneTime;

        private Scene currentActiveScene;

        public Scene CurrentActiveScene => currentActiveScene;

        public float SceneLoadedTime => sceneLoadedTime;

        public float SceneTime => sceneTime;

        public Camera MainCamera => mainCamera;



        protected override void Awake()
        {
            base.Awake();
            if (_manager == null)
            {
                _manager = FrameworkManager.GetManager<SceneManager>();
            }
            UpdateCamera();
            currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        private IEnumerator Start()
        {
            yield return null;
            if (FrameworkManager.setting.SceneSettings.StartScene != FrameworkManager.setting.FrameworkSceneID)
            {
                if (FrameworkManager.setting.SceneSettings.StartSceneAnimation)
                    LoadSceneDefault(FrameworkManager.setting.SceneSettings.StartScene);
                else
                    LoadScene(FrameworkManager.setting.SceneSettings.StartScene);
            }

        }

        private void Update()
        {
            sceneTime += Time.deltaTime;
        }


        private void UpdateActiveScene()
        {
            currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            sceneIndex = currentActiveScene.buildIndex;
            sceneLoadedTime = Time.time;
            sceneTime = 0f;
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.OnNewActiveScene, sceneIndex);
        }

        private void UpdateCamera()
        {
            mainCamera = Camera.main;
        }

        #region Unload

        /// <summary>
        /// 卸载当前活动场景
        /// </summary>
        /// <param Name="callback">卸载完成时的回调函数</param>
        public void UnloadScene(UnityAction callback = null)
        {
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeUnloadScene);
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.InActiveCurrentScene, currentActiveScene.buildIndex);
            manager.UnloadScene(() => 
            {
                Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeUnloadScene); 
                UpdateCamera(); 
                callback?.Invoke();
                UpdateActiveScene();
            });
        }

        /// <summary>
        /// 通过索引卸载场景
        /// </summary>
        /// <param Name="sceneIndex">已加载场景的buildIndex</param>
        /// <param Name="callback">卸载完成时的回调函数</param>
        public void UnloadScene(int sceneIndex, UnityAction callback = null)
        {
            if (currentActiveScene.buildIndex == sceneIndex)
            {
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.InActiveCurrentScene, currentActiveScene.buildIndex);
                callback += UpdateActiveScene;
            }

            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeUnloadScene);
            manager.UnloadScene(sceneIndex, () => 
            {
                Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.AfterUnloadScene);
                UpdateCamera();
                callback?.Invoke();
            });
        }

        /// <summary>
        /// 通过场景名称卸载场景
        /// </summary>
        /// <param Name="sceneName">待卸载场景名</param>
        /// <param Name="callback">卸载完成时的回调函数</param>
        public void UnloadScene(string sceneName, UnityAction callback = null)
        {
            if (currentActiveScene.name == sceneName)
            {
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.InActiveCurrentScene, currentActiveScene.buildIndex);
                callback += UpdateActiveScene;
            }

            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeUnloadScene);
            manager.UnloadScene(sceneName, () =>
            {
                Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.AfterUnloadScene);
                UpdateCamera();
                callback?.Invoke();
            });
        }

        #endregion

        #region Load

        /// <summary>
        /// 通过索引加载场景
        /// </summary>
        /// <param Name="buildIndex">场景的buildIndex</param>
        /// <param Name="callback">加载完成的回调</param>
        /// <returns>加载场景AsyncOperation</returns>
        public AsyncOperation LoadScene(int buildIndex, UnityAction callback = null)
        {
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeLoadScene);
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.InActiveCurrentScene, currentActiveScene.buildIndex);
            return manager.LoadScene(buildIndex, () => 
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(buildIndex));
                UpdateCamera(); 
                callback?.Invoke();
                UpdateActiveScene();
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.AfterLoadScene);
            });
        }

        /// <summary>
        /// 通过场景名称加载场景
        /// </summary>
        /// <param Name="sceneName">场景名称</param>
        /// <param Name="callback">加载完成的回调</param>
        /// <returns>加载场景AsyncOperation</returns>
        public AsyncOperation LoadScene(string sceneName, UnityAction callback = null)
        {
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeLoadScene);
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.InActiveCurrentScene, currentActiveScene.buildIndex);
            return manager.LoadScene(sceneName, () =>
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName));
                UpdateCamera();
                callback?.Invoke();
                UpdateActiveScene();
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.AfterLoadScene);
            });
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
        public AsyncOperation ChangeScene(int to, int from = -1, UnityAction callback = null)
        {
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeChangeScene);
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.InActiveCurrentScene, currentActiveScene.buildIndex);
            return manager.ChangeScene(to, from, () => 
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(to));
                UpdateCamera(); 
                callback?.Invoke();
                UpdateActiveScene();
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.AfterChangeScene);
            });
        }

        /// <summary>
        /// 通过场景名称切换场景
        /// </summary>
        /// <param Name="to">目的场景名称</param>
        /// <param Name="from">待卸载的场景名称，如果留为""，则卸载当前激活场景</param>
        /// <param Name="callback">切换场景完成的回调</param>
        /// <returns>加载场景AsyncOperation</returns>
        public AsyncOperation ChangeScene(string to, string from = "" , UnityAction callback = null)
        {
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.BeforeChangeScene);
            FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.InActiveCurrentScene, currentActiveScene.buildIndex);
            return manager.ChangeScene(to, from, () =>
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(to));
                UpdateCamera();
                callback?.Invoke();
                UpdateActiveScene();
                FrameworkManager.eventManager.InvokeEvent(FrameworkEvent.AfterChangeScene);
            });
        }


        #endregion

        #region LoadWithAnimation

        /// <summary>
        /// 以默认动画加载场景
        /// </summary>
        /// <param Name="buildIndex"></param>
        /// <param Name="callback"></param>
        public void LoadSceneDefault(int buildIndex, UnityAction callback = null)
        {
            StartCoroutine(defaultLoad());
            IEnumerator defaultLoad()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>("DefaultAnimationCanvas");
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = FrameworkManager.setting.SceneSettings.fadeInTime;
                float afterTime = FrameworkManager.setting.SceneSettings.fadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                yield return LoadScene(buildIndex, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// 以默认动画加载场景
        /// </summary>
        /// <param Name="_sceneName"></param>
        /// <param Name="callback"></param>
        public void LoadSceneDefault(string _sceneName, UnityAction callback = null)
        {
            StartCoroutine(defaultLoad());
            IEnumerator defaultLoad()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>("DefaultAnimationCanvas");
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = FrameworkManager.setting.SceneSettings.fadeInTime;
                float afterTime = FrameworkManager.setting.SceneSettings.fadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                yield return LoadScene(_sceneName, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// 以进度条动画加载场景
        /// </summary>
        /// <param Name="buildIndex"></param>
        /// <param Name="_gameObject">进度条动画物体,需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(int buildIndex, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                Debug.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if(obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                callback += () => Destroy(obj);
                StartCoroutine(ProcessCoroutine(LoadScene(buildIndex, callback), sceneLoadBar));
            }
            else
            {
                Debug.LogError("Animation gameObject should has component LoadProgressBase");
            }
        }

        /// <summary>
        /// 以进度条动画加载场景
        /// </summary>
        /// <param Name="buildIndex"></param>
        /// <param Name="filepath">进度条动画物体的路径,物体需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(int buildIndex, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                Debug.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("No game object at the pointed file path.");
                    yield break;
                }
                LoadSceneProgressBar(buildIndex, (GameObject)r.asset, callback);
            }

        }
        /// <summary>
        /// 以进度条动画加载场景
        /// </summary>
        /// <param Name="sceneName"></param>
        /// <param Name="_gameObject">进度条动画物体,需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(string sceneName, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                Debug.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if (obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                callback += () => Destroy(obj);
                StartCoroutine(ProcessCoroutine(LoadScene(sceneName, callback), sceneLoadBar));
            }
            else
            {
                Debug.LogError("Animation gameObject should has component LoadProgressBase");
            }

        }

        /// <summary>
        /// 以进度条动画加载场景
        /// </summary>
        /// <param Name="sceneName"></param>
        /// <param Name="filepath">进度条动画物体的路径,物体需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(string sceneName, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                Debug.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("No game object at the pointed file path.");
                    yield break;
                }
                LoadSceneProgressBar(sceneName, (GameObject)r.asset, callback);
            }

        }

        /// <summary>
        /// 以默认动画切换场景
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from">留为-1则为当前活动场景</param>
        /// <param Name="callback"></param>
        public void ChangeSceneDefault(int to, int from = -1, UnityAction callback = null)
        {
            StartCoroutine(defaultChange());
            IEnumerator defaultChange()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>("DefaultAnimationCanvas");
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = FrameworkManager.setting.SceneSettings.fadeInTime;
                float afterTime = FrameworkManager.setting.SceneSettings.fadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                yield return ChangeScene(to, from, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// 以默认动画切换场景
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from">留为""则为当前活动场景</param>
        /// <param Name="callback"></param>
        public void ChangeSceneDefault(string to, string from = "", UnityAction callback = null)
        {
            StartCoroutine(defaultChange());
            IEnumerator defaultChange()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>("DefaultAnimationCanvas");
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = FrameworkManager.setting.SceneSettings.fadeInTime;
                float afterTime = FrameworkManager.setting.SceneSettings.fadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                yield return ChangeScene(to, from, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// 以进度条动画的方式切换场景
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="_gameObject">进度条动画物体,需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(int to, int from, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                Debug.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if (obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                callback += () => Destroy(obj);
                StartCoroutine(ProcessCoroutine(ChangeScene(to, from, callback), sceneLoadBar));
            }
            else
            {
                Debug.LogError("Animation gameObject should has component LoadProgressBase");
            }

        }

        /// <summary>
        /// 以进度条动画的方式切换场景
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="filepath">进度条动画物体的路径,物体需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(int to, int from, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                Debug.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("No game object at the pointed file path.");
                    yield break;
                }
                ChangeSceneProgressBar(to,from, (GameObject)r.asset, callback);
            }

        }

        /// <summary>
        /// 以进度条动画的方式切换场景
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="_gameObject">进度条动画物体,需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(string to,string from, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                Debug.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if (obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                callback += () => Destroy(obj);
                StartCoroutine(ProcessCoroutine(ChangeScene(to, from, callback), sceneLoadBar));
            }
            else
            {
                Debug.LogError("Animation gameObject should has component LoadProgressBase");
            }

        }
        /// <summary>
        /// 以进度条动画的方式切换场景
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="filepath">进度条动画物体的路径,物体需要拥有继承自ILoadProgress的脚本组件</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(string to, string from, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                Debug.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    Debug.LogError("No game object at the pointed file path.");
                    yield break;
                }
                ChangeSceneProgressBar(to, from, (GameObject)r.asset, callback);
            }

        }

        /// <summary>
        /// 将Unity异步进程以进度条动画形式展现，支持多进程使用一个进度条动画展示
        /// </summary>
        /// <param Name="asyncOperation"></param>
        /// <param Name="LoadProgress"></param>
        /// <returns></returns>
        public IEnumerator ProcessCoroutine(AsyncOperation asyncOperation, LoadProgressBase LoadProgress, float startValue = 0f, float endValue = 1f)
        {
            if (LoadProgress == null)
            {
                Debug.LogError("Progress component can not be null");
                yield break;
            }
            asyncOperation.allowSceneActivation = false;
            float currentProgress = 0f;
            float targetProgress = 0f;
            float displayProgress = 0f;
            while (!asyncOperation.isDone)
            {
                targetProgress = asyncOperation.progress;
                if(currentProgress < targetProgress) 
                {
                    currentProgress += LoadProgress.speed;
                }
                displayProgress = Mathf.Lerp(startValue, endValue, currentProgress * 10 / 9.0f);
                LoadProgress.SetProgressValue(displayProgress);
                if (currentProgress >= 0.9f)
                {
                    LoadProgress.BeforeSetActive(asyncOperation);
                }
                yield return null;
            }
        }

        #endregion


    }
}
