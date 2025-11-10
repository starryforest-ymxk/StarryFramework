using UnityEngine;

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
        // private GameObject objectHandle;
        // private GameObject uiObject;
        private bool releaseTag;
        private bool isOpened;
        
        public int SerialID => serialID;
        public string UIFormAssetName => uiFormAssetName;
        // public GameObject ObjectHandle => objectHandle;
        // public GameObject UIObject => uiObject;
        public UIGroup UIGroup => uiGroup;
        public int DepthInUIGroup => depthInUIGroup; 
        public bool PauseCoveredUIForm => pauseCoveredUiForm;
        public UIFormLogic UIFormLogic => uiFormLogic;
        public bool ReleaseTag => releaseTag;
        public bool IsOpened => isOpened;
        
#if UNITY_EDITOR
        public bool Foldout { get; set; }
        public bool FoldoutInCache { get; set; }
#endif
        
        
        
        #region 生命周期

        public void OnInit(
            int serialId, string assetName, UIGroup group, bool pauseCoveredUIForm, 
            UIFormLogic logic, GameObject handle/*, GameObject @object*/)
        {
            serialID = serialId;
            uiFormAssetName = assetName;
            uiGroup = group;
            pauseCoveredUiForm = pauseCoveredUIForm;
            uiFormLogic = logic;
            // objectHandle = handle;
            // uiObject = @object;
            releaseTag = false;
            if(uiFormLogic != null)
                uiFormLogic.OnInit(handle);
            else
                FrameworkManager.Debugger.LogError("uiFormLogic is null.");
            
        }
        public void OnRelease()
        {
            if (releaseTag)
            {
                FrameworkManager.Debugger.LogWarning("uiForm has already been released.");
                return;
            }
            if(uiFormLogic != null)
                uiFormLogic.OnRelease();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null.");
            // if(uiObject != null)
            //     Object.Destroy(uiObject);
            // Addressables.Release(objectHandle);
            releaseTag = true;
        }
        public void OnOpen()
        {
            isOpened = true;
            if(uiFormLogic != null)
                uiFormLogic.OnOpen();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        public void OnClose(bool isShutdown)
        {
            isOpened = false;
            if(uiFormLogic != null)
                uiFormLogic.OnClose(isShutdown);
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        public void OnCover()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnCover();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");

        }
        public void OnReveal()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnReveal();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        public void OnPause()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnPause();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        public void OnResume()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnResume();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        public void OnUpdate()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnUpdate();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        public void OnDepthChanged(int formCountInUIGroup, int newDepthInUIGroup)
        {
            depthInUIGroup = newDepthInUIGroup;
            if(uiFormLogic != null)
                uiFormLogic.OnDepthChanged(formCountInUIGroup, newDepthInUIGroup);
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        public void OnRefocus()
        {
            if(uiFormLogic != null)
                uiFormLogic.OnRefocus();
            else
                FrameworkManager.Debugger.LogWarning("uiFormLogic is null. Maybe the UI Object has been destroyed?");
        }
        

        #endregion

    }
}


