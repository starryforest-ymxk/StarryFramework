using StarryFramework;
using UnityEngine;


namespace StarryFramework
{

    public class UIRoot : MonoBehaviour
    {
        [Header("Optional Canvas References (recommended)")]
        [SerializeField] private Transform canvasStatic;
        [SerializeField] private Transform canvasDynamic;

        public static UIRoot Instance { get; private set; }

        private const string CanvasStaticName = "CanvasStatic";
        private const string CanvasDynamicName = "CanvasDynamic";

        private void Awake()
        {
            RegisterInstance();
            AutoResolveCanvasParents();
        }

        private void OnEnable()
        {
            RegisterInstance();
            Framework.EventComponent.AddEventListener(FrameworkEvent.BeforeChangeScene, ClearCache);
        }

        private void OnDisable()
        {
            Framework.EventComponent.RemoveEventListener(FrameworkEvent.BeforeChangeScene, ClearCache);

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void ClearCache()
        {
            Framework.UIComponent.CloseAndReleaseAllForms();
        }

        private void RegisterInstance()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning($"Multiple {nameof(UIRoot)} instances found. The latest enabled instance will be used.");
            }

            Instance = this;
        }

        private void AutoResolveCanvasParents()
        {
            if (canvasStatic == null)
            {
                canvasStatic = FindChildRecursiveByName(transform, CanvasStaticName);
            }

            if (canvasDynamic == null)
            {
                canvasDynamic = FindChildRecursiveByName(transform, CanvasDynamicName);
            }
        }

        private static Transform FindChildRecursiveByName(Transform root, string targetName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == targetName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursiveByName(root.GetChild(i), targetName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        internal static bool TryGetCanvasParent(UIDynamicType dynamicType, out Transform parent)
        {
            parent = null;

            if (Instance == null)
            {
                return false;
            }

            Instance.AutoResolveCanvasParents();

            parent = dynamicType == UIDynamicType.Static ? Instance.canvasStatic : Instance.canvasDynamic;
            return parent != null;
        }
    }

}
