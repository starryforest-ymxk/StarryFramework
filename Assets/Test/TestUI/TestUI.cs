using System;
using System.Collections;
using System.Collections.Generic;
using StarryFramework;
using UnityEngine;

public class TestUI : MonoBehaviour
{
    [SerializeField] private string openUIName;
    private void Awake()
    {
        if (!Framework.UIComponent.HasUIGroup("DefaultUIGroup"))
        {
            Framework.UIComponent.AddUIGroup("DefaultUIGroup");
        }
    }

    private void Start()
    {
        Framework.UIComponent.OpenUIForm(openUIName, "DefaultUIGroup", true);
    }
}
