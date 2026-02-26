using StarryFramework;
using UnityEngine;

public class TestUIPanel : UIMainPanelBase
{
    [SerializeField] private string openPanelName;
    [SerializeField] private string openSettingName;

    public void OpenPanel()
    {
        CloseTopmost(gameUIFormName);
        OpenSingleGlobal(openPanelName);
    }
    
    public void OpenSetting()
    {
        OpenSingleGlobal(openSettingName);
    }

    private static void CloseTopmost(string assetName)
    {
        UIForm topmost = Framework.UIComponent.GetTopUIForm(assetName);
        if (topmost != null)
        {
            Framework.UIComponent.CloseUIForm(topmost.SerialID);
        }
    }

    private static void OpenSingleGlobal(string assetName)
    {
        Framework.UIComponent.OpenUIForm(new OpenUIFormOptions
        {
            AssetName = assetName,
            GroupName = "DefaultUIGroup",
            PauseCoveredUIForm = true,
            OpenPolicy = UIOpenPolicy.SingleInstanceGlobal,
            RefocusIfExists = true
        });
    }
}
