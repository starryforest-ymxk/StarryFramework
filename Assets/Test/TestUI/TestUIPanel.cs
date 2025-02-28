using System.Collections;
using System.Collections.Generic;
using StarryFramework;
using UnityEngine;

public class TestUIPanel : UIMainPanelBase
{
    [SerializeField] private string openPanelName;
    [SerializeField] private string openSettingName;

    public void OpenPanel()
    {
        Framework.UIComponent.CloseUIForm(gameUIFormName);
        Framework.UIComponent.OpenUIForm(openPanelName, "DefaultUIGroup", true);
    }
    
    public void OpenSetting()
    {
        Framework.UIComponent.OpenUIForm(openSettingName, "DefaultUIGroup", true);
    }
}
