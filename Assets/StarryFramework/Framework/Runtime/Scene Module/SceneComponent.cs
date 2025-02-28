using DG.Tweening;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace StarryFramework
{
    [DisallowMultipleComponent]
    public sealed class SceneComponent: BaseComponent
    {

        private SceneManager _manager;
        private SceneManager Manager => _manager ??= FrameworkManager.GetManager<SceneManager>();

        [SerializeField] private SceneSettings settings;

        private int sceneIndex;
        private float sceneLoadedTime;
        private float sceneTime;
        private Scene currentActiveScene;

        public Scene CurrentActiveScene => currentActiveScene;
        public float SceneLoadedTime => sceneLoadedTime;
        public float SceneTime => sceneTime;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(EditorApplication.isPlaying && _manager != null)
                (_manager as IManager).SetSettings(settings);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<SceneManager>();
            (_manager as IManager).SetSettings(settings);
            currentActiveScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        }

        private IEnumerator Start()
        {
            yield return null;
            if (FrameworkManager.Setting.StartScene == FrameworkManager.Setting.FrameworkSceneID) yield break;
            if (FrameworkManager.Setting.StartSceneAnimation)
                LoadSceneDefault(FrameworkManager.Setting.StartScene);
            else
                LoadScene(FrameworkManager.Setting.StartScene);

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
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetNewActiveScene, sceneIndex);
        }

        #region Unload

        /// <summary>
        /// ж�ص�ǰ�����
        /// </summary>
        /// <param Name="callback">ж�����ʱ�Ļص�����</param>
        public void UnloadScene(UnityAction callback = null)
        {
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeUnloadScene);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetCurrentActiveSceneNotActive, currentActiveScene.buildIndex);
            Manager.UnloadScene(() => 
            {
                Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
                UpdateActiveScene();
                callback?.Invoke();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterUnloadScene); 
            });
        }

        /// <summary>
        /// ͨ������ж�س���
        /// </summary>
        /// <param Name="sceneIndex">�Ѽ��س�����buildIndex</param>
        /// <param Name="callback">ж�����ʱ�Ļص�����</param>
        public void UnloadScene(int sceneIndex, UnityAction callback = null, bool autoSetActiveSceneIfUnloadActiveScene = true)
        {
            bool setSceneActive = false;
            if (currentActiveScene.buildIndex == sceneIndex && autoSetActiveSceneIfUnloadActiveScene)
            {
                setSceneActive = true;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetCurrentActiveSceneNotActive, currentActiveScene.buildIndex);
            }

            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeUnloadScene);
            Manager.UnloadScene(sceneIndex, () => 
            {
                if (setSceneActive)
                {
                    Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
                    UpdateActiveScene();
                }
                callback?.Invoke();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterUnloadScene);
            });
        }

        /// <summary>
        /// ͨ����������ж�س���
        /// </summary>
        /// <param Name="sceneName">��ж�س�����</param>
        /// <param Name="callback">ж�����ʱ�Ļص�����</param>
        public void UnloadScene(string sceneName, UnityAction callback = null, bool autoSetActiveSceneIfUnloadActiveScene = true)
        {
            bool setSceneActive = false;
            if (currentActiveScene.name == sceneName && autoSetActiveSceneIfUnloadActiveScene)
            {
                setSceneActive = true;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetCurrentActiveSceneNotActive, currentActiveScene.buildIndex);
            }

            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeUnloadScene);
            Manager.UnloadScene(sceneName, () =>
            {
                if (setSceneActive)
                {
                    Scene s = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(s);
                    UpdateActiveScene();
                }
                callback?.Invoke();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterUnloadScene);
            });
        }

        #endregion

        #region Load

        /// <summary>
        /// ͨ���������س���
        /// </summary>
        /// <param Name="buildIndex">������buildIndex</param>
        /// <param Name="callback">������ɵĻص�</param>
        /// <returns>���س���AsyncOperation</returns>
        public AsyncOperation LoadScene(int buildIndex, UnityAction callback = null, bool setSceneToLoadActive = true)
        {
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadScene);
            if(setSceneToLoadActive)
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetCurrentActiveSceneNotActive, currentActiveScene.buildIndex);
            return Manager.LoadScene(buildIndex, () => 
            {
                if (setSceneToLoadActive)
                {
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(buildIndex));
                    UpdateActiveScene();
                }
                callback?.Invoke();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadScene);
            });
        }

        /// <summary>
        /// ͨ���������Ƽ��س���
        /// </summary>
        /// <param Name="sceneName">��������</param>
        /// <param Name="callback">������ɵĻص�</param>
        /// <returns>���س���AsyncOperation</returns>
        public AsyncOperation LoadScene(string sceneName, UnityAction callback = null, bool setSceneToLoadActive = true)
        {
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeLoadScene);
            if (setSceneToLoadActive)
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetCurrentActiveSceneNotActive, currentActiveScene.buildIndex);
            return Manager.LoadScene(sceneName, () =>
            {
                if (setSceneToLoadActive)
                {
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName));
                    UpdateActiveScene();
                }
                callback?.Invoke();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterLoadScene);
            });
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
        public AsyncOperation ChangeScene(int to, int from = -1, UnityAction callback = null)
        {
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeChangeScene);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetCurrentActiveSceneNotActive, currentActiveScene.buildIndex);
            return Manager.ChangeScene(to, from, () => 
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByBuildIndex(to));
                UpdateActiveScene();
                callback?.Invoke();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterChangeScene);
            });
        }

        /// <summary>
        /// ͨ�����������л�����
        /// </summary>
        /// <param Name="to">Ŀ�ĳ�������</param>
        /// <param Name="from">��ж�صĳ������ƣ������Ϊ""����ж�ص�ǰ�����</param>
        /// <param Name="callback">�л�������ɵĻص�</param>
        /// <returns>���س���AsyncOperation</returns>
        public AsyncOperation ChangeScene(string to, string from = "" , UnityAction callback = null)
        {
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.BeforeChangeScene);
            FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.SetCurrentActiveSceneNotActive, currentActiveScene.buildIndex);
            return Manager.ChangeScene(to, from, () =>
            {
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(UnityEngine.SceneManagement.SceneManager.GetSceneByName(to));
                UpdateActiveScene();
                callback?.Invoke();
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.AfterChangeScene);
            });
        }


        #endregion

        #region LoadWithAnimation

        /// <summary>
        /// ��Ĭ�϶������س���
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
                    FrameworkManager.Debugger.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = settings.defaultAnimationFadeInTime;
                float afterTime = settings.defaultAnimationFadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                yield return LoadScene(buildIndex, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// ��Ĭ�϶������س���
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
                    FrameworkManager.Debugger.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = settings.defaultAnimationFadeInTime;
                float afterTime = settings.defaultAnimationFadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                yield return LoadScene(_sceneName, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// �Խ������������س���
        /// </summary>
        /// <param Name="buildIndex"></param>
        /// <param Name="_gameObject">��������������,��Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(int buildIndex, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                FrameworkManager.Debugger.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if(obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += () => Destroy(obj);                
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                StartCoroutine(ProcessCoroutine(LoadScene(buildIndex, callback), sceneLoadBar, sceneLoadBar.BeforeSetActive));
            }
            else
            {
                FrameworkManager.Debugger.LogError("Animation gameObject should has component LoadProgressBase");
            }
        }

        /// <summary>
        /// �Խ������������س���
        /// </summary>
        /// <param Name="buildIndex"></param>
        /// <param Name="filepath">���������������·��,������Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(int buildIndex, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                FrameworkManager.Debugger.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    FrameworkManager.Debugger.LogError("No game object at the pointed file path.");
                    yield break;
                }
                LoadSceneProgressBar(buildIndex, (GameObject)r.asset, callback);
            }

        }
        
        /// <summary>
        /// �Խ������������س���
        /// </summary>
        /// <param Name="sceneName"></param>
        /// <param Name="_gameObject">��������������,��Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(string sceneName, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                FrameworkManager.Debugger.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if (obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += () => Destroy(obj);
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                StartCoroutine(ProcessCoroutine(LoadScene(sceneName, callback), sceneLoadBar, sceneLoadBar.BeforeSetActive));
            }
            else
            {
                FrameworkManager.Debugger.LogError("Animation gameObject should has component LoadProgressBase");
            }

        }

        /// <summary>
        /// �Խ������������س���
        /// </summary>
        /// <param Name="sceneName"></param>
        /// <param Name="filepath">���������������·��,������Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void LoadSceneProgressBar(string sceneName, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                FrameworkManager.Debugger.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    FrameworkManager.Debugger.LogError("No game object at the pointed file path.");
                    yield break;
                }
                LoadSceneProgressBar(sceneName, (GameObject)r.asset, callback);
            }

        }

        /// <summary>
        /// ��Ĭ�϶����л�����
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from">��Ϊ-1��Ϊ��ǰ�����</param>
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
                    FrameworkManager.Debugger.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = settings.defaultAnimationFadeInTime;
                float afterTime = settings.defaultAnimationFadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                yield return ChangeScene(to, from, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// ��Ĭ�϶����л�����
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from">��Ϊ""��Ϊ��ǰ�����</param>
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
                    FrameworkManager.Debugger.LogError("Default animation object has been deleted.");
                    yield break;
                }
                GameObject obj = Instantiate(r.asset, transform) as GameObject;
                CanvasGroup group = obj.GetComponent<CanvasGroup>();
                float beforeTime = settings.defaultAnimationFadeInTime;
                float afterTime = settings.defaultAnimationFadeOutTime;
                WaitForSecondsRealtime beforeW = new WaitForSecondsRealtime(beforeTime);
                WaitForSecondsRealtime afterW = new WaitForSecondsRealtime(afterTime);
                DOTween.To(() => group.alpha, x => group.alpha = x, 1, beforeTime);
                yield return beforeW;
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                yield return ChangeScene(to, from, callback);
                DOTween.To(() => group.alpha, x => group.alpha = x, 0, afterTime);
                yield return afterW;
                Destroy(obj);
            }

        }

        /// <summary>
        /// �Խ����������ķ�ʽ�л�����
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="_gameObject">��������������,��Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(int to, int from, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                FrameworkManager.Debugger.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if (obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += () => Destroy(obj);
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                StartCoroutine(ProcessCoroutine(ChangeScene(to, from, callback), sceneLoadBar, sceneLoadBar.BeforeSetActive));
            }
            else
            {
                FrameworkManager.Debugger.LogError("Animation gameObject should has component LoadProgressBase");
            }

        }

        /// <summary>
        /// �Խ����������ķ�ʽ�л�����
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="filepath">���������������·��,������Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(int to, int from, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                FrameworkManager.Debugger.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    FrameworkManager.Debugger.LogError("No game object at the pointed file path.");
                    yield break;
                }
                ChangeSceneProgressBar(to,from, (GameObject)r.asset, callback);
            }

        }

        /// <summary>
        /// �Խ����������ķ�ʽ�л�����
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="_gameObject">��������������,��Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(string to,string from, GameObject _gameObject, UnityAction callback = null)
        {
            if (gameObject == null)
            {
                FrameworkManager.Debugger.LogError("Game object can not be null.");
                return;
            }
            GameObject obj = Instantiate(_gameObject, transform);
            if (obj.TryGetComponent<LoadProgressBase>(out LoadProgressBase sceneLoadBar))
            {
                FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.StartSceneLoadAnim);
                callback += () => Destroy(obj);
                callback += ()=> FrameworkManager.EventManager.InvokeEvent(FrameworkEvent.EndSceneLoadAnim);
                StartCoroutine(ProcessCoroutine(ChangeScene(to, from, callback), sceneLoadBar, sceneLoadBar.BeforeSetActive));
            }
            else
            {
                FrameworkManager.Debugger.LogError("Animation gameObject should has component LoadProgressBase");
            }

        }
        /// <summary>
        /// �Խ����������ķ�ʽ�л�����
        /// </summary>
        /// <param Name="to"></param>
        /// <param Name="from"></param>
        /// <param Name="filepath">���������������·��,������Ҫӵ�м̳���ILoadProgress�Ľű����</param>
        /// <param Name="callback"></param>
        public void ChangeSceneProgressBar(string to, string from, string filepath, UnityAction callback = null)
        {
            if (filepath == "")
            {
                FrameworkManager.Debugger.LogError("File path can not be null.");
                return;
            }
            StartCoroutine(loadAssets());
            IEnumerator loadAssets()
            {
                ResourceRequest r = Resources.LoadAsync<GameObject>(filepath);
                yield return r;
                if (r.asset == null)
                {
                    FrameworkManager.Debugger.LogError("No game object at the pointed file path.");
                    yield break;
                }
                ChangeSceneProgressBar(to, from, (GameObject)r.asset, callback);
            }

        }

        /// <summary>
        /// ��Unity�첽�����Խ�����������ʽչ�֣�֧�ֶ����ʹ��һ������������չʾ
        /// </summary>
        /// <param name="asyncOperation"></param>
        /// <param name="loadProgress"></param>
        /// <param name="onLoaded"></param>
        /// <param name="startValue"></param>
        /// <param name="endValue"></param>
        /// <returns></returns>
        public IEnumerator ProcessCoroutine(AsyncOperation asyncOperation, LoadProgressBase loadProgress, UnityAction<AsyncOperation> onLoaded, float startValue = 0f, float endValue = 1f)
        {
            bool finish = false;
            if (loadProgress == null)
            {
                FrameworkManager.Debugger.LogError("Progress component can not be null");
                yield break;
            }
            asyncOperation.allowSceneActivation = false;
            var currentProgress = 0f;
            while (!finish)
            {
                var targetProgress = asyncOperation.progress;
                if(currentProgress < targetProgress) 
                    currentProgress += loadProgress.speed;
                
                var displayProgress = Mathf.Lerp(startValue, endValue, currentProgress * 10 / 9.0f);
                loadProgress.SetProgressValue(displayProgress);

                if (currentProgress >= 0.9f && !finish)
                {
                    finish = true;
                    onLoaded?.Invoke(asyncOperation);
                }
                yield return null;
            }
        }

        #endregion


    }
}
