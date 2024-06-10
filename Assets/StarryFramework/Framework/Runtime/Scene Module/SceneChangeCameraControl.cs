using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public class SceneChangeCameraControl : MonoBehaviour
    {
        private Camera m_camera;
        [SerializeField] private bool isMainCamera;
        private void Awake()
        {
            m_camera = GetComponent<Camera>();
            gameObject.tag = isMainCamera ? "MainCamera" : "Untagged";
            m_camera.enabled = isMainCamera;
            FrameworkManager.EventManager.AddEventListener(FrameworkEvent.StartSceneLoadAnim, SetCameraEnabled);
            FrameworkManager.EventManager.AddEventListener(FrameworkEvent.EndSceneLoadAnim, SetCameraNotEnabled);
        }

        private void OnDestroy()
        {
            FrameworkManager.EventManager.RemoveEventListener(FrameworkEvent.StartSceneLoadAnim, SetCameraEnabled);
            FrameworkManager.EventManager.RemoveEventListener(FrameworkEvent.EndSceneLoadAnim, SetCameraNotEnabled);
        }
        
        private void SetCameraEnabled()
        {
            if(!isMainCamera)
                m_camera.enabled = true;
        }
        
        private void SetCameraNotEnabled()
        {
            if(!isMainCamera)
                m_camera.enabled = false;
        }
    }

}


