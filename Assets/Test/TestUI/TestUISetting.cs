using StarryFramework;
using UnityEngine;

public class TestUISetting : UICoverPanelBase
{

    [SerializeField] private Vector2 uiSize = new(720, 480);
    public void CloseSetting()
    {
        UIForm topmost = Framework.UIComponent.GetTopUIForm(gameUIFormName);
        if (topmost != null)
        {
            Framework.UIComponent.CloseUIForm(topmost.SerialID);
        }
    }

    public override void OnInit(GameObject uiPrefab)
    {
        base.OnInit(uiPrefab);
        SetUItoCenter(uiSize);
    }
}
