using StarryFramework;
using UnityEngine;

public class UIRoot : MonoBehaviour
{
    private void OnEnable()
    {
        Framework.EventComponent.AddEventListener(FrameworkEvent.BeforeChangeScene, ClearCache);
    }

    private void OnDisable()
    {
        Framework.EventComponent?.RemoveEventListener(FrameworkEvent.BeforeChangeScene, ClearCache);
    }

    private void ClearCache()
    {
        Framework.UIComponent.CloseAndReleaseAllForms();
    }
}
