using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using StarryFramework;
using UnityEngine.AddressableAssets;

namespace StarryFramework
{

    public enum UIDynamicType
    {
        Static,
        Dynamic
    }

    public enum UIChangeType
    {
        Fade,
        Cut
    }

    public abstract class UguiForm : MonoBehaviour, UIFormLogic
    {
        private GameObject handle;

        protected RectTransform m_rectTransform;
        protected CanvasGroup m_group;

        [SerializeField] protected string gameUIFormName;
        [SerializeField] protected UIDynamicType uiDynamicType = UIDynamicType.Dynamic;
        [SerializeField] protected UIChangeType uiChangeType = UIChangeType.Fade;
        [SerializeField] private float uiFadeDuration = 0.5f;

        public string GameUIFormName => gameUIFormName;
        public UIDynamicType UIDynamicType => uiDynamicType;
        public UIChangeType UIChangeType => uiChangeType;

        private TweenerCore<float, float, FloatOptions> m_tweener;
        private bool releaseTag;

        protected virtual void Awake()
        {
            releaseTag = false;
            m_rectTransform = transform as RectTransform;
            transform.SetParent(uiDynamicType == UIDynamicType.Static
                ? GameObject.Find("CanvasStatic").GetComponent<Transform>()
                : GameObject.Find("CanvasDynamic").GetComponent<Transform>());

            if (!TryGetComponent(out m_group))
            {
                m_group = gameObject.AddComponent<CanvasGroup>();
            }

            m_group.interactable = false;
            m_group.blocksRaycasts = false;
            m_group.alpha = 0;
        }

        #region UI lifecycle

        public virtual void OnInit(GameObject uiPrefab)
        {
            handle = uiPrefab;
        }

        public virtual void OnRelease()
        {
            if (this != null && gameObject != null)
                Destroy(gameObject);
            Addressables.Release(handle);
        }

        public virtual void OnOpen()
        {
            SetVisible(true);
        }

        public virtual void OnClose(bool isShutdown)
        {
            if (!isShutdown)
                SetVisible(false);
        }

        public virtual void OnCover()
        {
        }

        public virtual void OnReveal()
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnDepthChanged(int formCountInUIGroup, int depthInUIGroup)
        {
            transform.SetSiblingIndex(depthInUIGroup);
        }

        public virtual void OnRefocus()
        {
        }

        #endregion

        protected void SetFullRect()
        {
            m_rectTransform.anchorMin = Vector2.zero;
            m_rectTransform.anchorMax = Vector2.one;

            m_rectTransform.offsetMin = Vector2.zero;
            m_rectTransform.offsetMax = Vector2.zero;

            m_rectTransform.pivot = new Vector2(0.5f, 0.5f);
            m_rectTransform.localPosition = Vector3.zero;
        }

        protected void SetUItoCenter(Vector2 size)
        {
            m_rectTransform.anchorMin = 0.5f * Vector2.one;
            m_rectTransform.anchorMax = 0.5f * Vector2.one;

            m_rectTransform.offsetMin = Vector2.zero;
            m_rectTransform.offsetMax = Vector2.zero;

            m_rectTransform.pivot = new Vector2(0.5f, 0.5f);
            m_rectTransform.localPosition = Vector3.zero;
            m_rectTransform.sizeDelta = size;
        }

        protected void SetVisible(bool visible)
        {
            if (releaseTag)
                return;
            switch (uiChangeType)
            {
                case UIChangeType.Fade:
                    m_tweener = DOTween.To(() => m_group.alpha, alpha => m_group.alpha = alpha, visible ? 1f : 0f,
                        uiFadeDuration);
                    m_group.interactable = visible;
                    m_group.blocksRaycasts = visible;
                    break;
                case UIChangeType.Cut:
                    m_group.alpha = visible ? 1f : 0f;
                    m_group.interactable = visible;
                    m_group.blocksRaycasts = visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy()
        {
            releaseTag = true;
            if (m_tweener != null && m_tweener.IsActive())
                m_tweener.Kill();
        }
    }

}