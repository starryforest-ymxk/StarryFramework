using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace StarryFramework
{
    public class UIForm
    {
        private int serialID;
        private string uiFormAssetName;
        private UIGroup uiGroup;
        private int depthInUIGroup;
        private bool pauseCoveredUiForm;
        private UIFormLogic uiFormLogic;
        private GameObject objectHandle;
        private GameObject uiObject;
        private bool releaseTag;
        
        public int SerialID => serialID;
        public string UIFormAssetName => uiFormAssetName;
        public GameObject ObjectHandle => objectHandle;
        public GameObject UIObject => uiObject;
        public UIGroup UIGroup => uiGroup;
        public int DepthInUIGroup => depthInUIGroup; 
        public bool PauseCoveredUIForm => pauseCoveredUiForm;
        public UIFormLogic UIFormLogic => uiFormLogic;
        public bool ReleaseTag => releaseTag;
        
#if UNITY_EDITOR

        private bool _foldout = false;
        public bool Foldout { get => _foldout; set => _foldout = value; }
        
        private bool _foldoutInCache = false;
        public bool FoldoutInCache { get => _foldoutInCache; set => _foldoutInCache = value; }
#endif
        
        
        
        #region ÉúÃüÖÜÆÚ

        public void OnInit(int serialId, string assetName, UIGroup group, bool pauseCoveredUIForm, UIFormLogic logic, GameObject handle, GameObject @object)
        {
            serialID = serialId;
            uiFormAssetName = assetName;
            uiGroup = group;
            pauseCoveredUiForm = pauseCoveredUIForm;
            uiFormLogic = logic;
            objectHandle = handle;
            uiObject = @object;
            releaseTag = false;
            uiFormLogic.OnInit();
        }
        
        public void OnRelease()
        {
            uiFormLogic.OnRelease();
            Object.Destroy(uiObject);
            Addressables.Release(objectHandle);
            releaseTag = true;
        }
        
        public void OnOpen()
        {
            uiFormLogic.OnOpen();
        }
        
        public void OnClose(bool isShutdown)
        {
            uiFormLogic.OnClose(isShutdown);
        }
        
        public void OnCover()
        {
            uiFormLogic.OnCover();
        }
        
        public void OnReveal()
        {
            uiFormLogic.OnReveal();
        }
        
        public void OnPause()
        {
            uiFormLogic.OnPause();
        }
        
        public void OnResume()
        {
            uiFormLogic.OnResume();
        }
        
        public void OnUpdate()
        {
            uiFormLogic.OnUpdate();
        }
        
        public void OnDepthChanged(int formCountInUIGroup, int newDepthInUIGroup)
        {
            depthInUIGroup = newDepthInUIGroup;
            uiFormLogic.OnDepthChanged(formCountInUIGroup, newDepthInUIGroup);
        }
        
        public void OnRefocus()
        {
            uiFormLogic.OnRefocus();
        }
        

        #endregion

    }
}


