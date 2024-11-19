using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(AudioListener))]
    public class SceneChangeCameraControl : MonoBehaviour
    {
        private Camera m_camera;
        private AudioListener _audioListener;
        [SerializeField] private bool isMainCamera;
        private void Awake()
        {
            m_camera = GetComponent<Camera>();
            _audioListener = GetComponent<AudioListener>();
            gameObject.tag = isMainCamera ? "MainCamera" : "Untagged";
            m_camera.enabled = isMainCamera;
            _audioListener.enabled = isMainCamera;
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
            {
                m_camera.enabled = true;
                _audioListener.enabled = true;
            }
        }
        
        private void SetCameraNotEnabled()
        {
            if(!isMainCamera)
            {
                m_camera.enabled = false;
                _audioListener.enabled = false;
            }
        }
    }

}


