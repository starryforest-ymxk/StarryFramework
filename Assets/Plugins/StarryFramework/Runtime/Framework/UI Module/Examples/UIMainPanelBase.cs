using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;


namespace StarryFramework
{

    public class UIMainPanelBase : UguiForm
    {
        public override void OnInit(GameObject uiPrefab)
        {
            base.OnInit(uiPrefab);
            SetFullRect();
        }
    }

}
