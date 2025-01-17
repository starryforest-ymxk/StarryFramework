using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;

public class UIMainPanelBase : UguiForm
{
    public override void OnInit()
    {
        base.OnInit();
        SetDefaultRect();
    }

    public override void OnCover()
    {
        base.OnCover();
        gameObject.SetActive(false);
    }

    public override void OnReveal()
    {
        base.OnReveal();
        gameObject.SetActive(true);
    }


}
