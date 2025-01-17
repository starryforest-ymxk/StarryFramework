using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using StarryFramework;

public enum UIType { Static, Dynamic}

public abstract class UguiForm : MonoBehaviour, UIFormLogic
{
    protected RectTransform m_rectTransform;
    protected UINavigation uiNavigation;
    [SerializeField] protected string m_assetName;
    [SerializeField] protected UIType m_uiType;
    
    public UIType UIType => m_uiType;
    public string AssetName => m_assetName;

    protected virtual void Awake()
    {
        m_rectTransform = transform as RectTransform;
        if (m_uiType == UIType.Static)
        {
            transform.SetParent(GameObject.Find("CanvasStatic").GetComponent<Transform>());
        }
        else
        {
            transform.SetParent(GameObject.Find("CanvasDynamic").GetComponent<Transform>());
        }
        uiNavigation = GetComponent<UINavigation>();
    }

    #region UI lifecycle

    public virtual void OnInit() { }

    public virtual void OnRelease() { }

    public virtual void OnOpen()
    {
        uiNavigation?.SetNavigationEnabled(true);
    }

    public virtual void OnClose(bool isShutdown)
    {

    }

    public virtual void OnCover()
    {
        uiNavigation?.SetNavigationEnabled(false);
    }

    public virtual void OnReveal()
    {
        uiNavigation?.SetNavigationEnabled(true);
    }

    public virtual void OnPause() { }

    public virtual void OnResume() { }

    public virtual void OnUpdate() { }

    public virtual void OnDepthChanged(int formCountInUIGroup, int depthInUIGroup)
    {
        transform.SetSiblingIndex(depthInUIGroup);
    }

    public virtual void OnRefocus()
    {
        uiNavigation?.SetNavigationEnabled(true);
    }

    #endregion
    
    protected void SetDefaultRect()
    {
        m_rectTransform.anchorMin = Vector2.zero;
        m_rectTransform.anchorMax = Vector2.one;
        
        m_rectTransform.offsetMin = Vector2.zero;
        m_rectTransform.offsetMax = Vector2.zero;
        
        m_rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        Vector3 position = m_rectTransform.localPosition;
        position.z = 0;
        m_rectTransform.localPosition = position;
    }
    
}
