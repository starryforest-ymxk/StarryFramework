using StarryFramework;
using UnityEngine;

public class TestUISetting : UICoverPanelBase
{

    [SerializeField] private Vector2 uiSize = new(720, 480);
    public void CloseSetting()
    {
        Framework.UIComponent.CloseUIForm(gameUIFormName);
    }

    public override void OnInit(GameObject uiPrefab)
    {
        base.OnInit(uiPrefab);
        SetUItoCenter(uiSize);
    }
}
