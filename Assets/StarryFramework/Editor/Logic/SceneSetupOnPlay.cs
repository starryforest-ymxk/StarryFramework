using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using StarryFramework;

namespace StarryFramework.Editor
{
    [InitializeOnLoad] 
    public class SceneSetupOnPlay
    {
        private static SceneSetup[] originalScenes;
        private static readonly string sessionKey = "OriginalScenes";
        
        
        static SceneSetupOnPlay()
        {
            BindCallback();
        }

        public static void BindCallback()
        {
            EditorApplication.playModeStateChanged += PrepareStartScenes;
        }

        private static void PrepareStartScenes(PlayModeStateChange state)
        {
            FrameworkSettings settings = FrameworkSettings.Instance;
            if (settings == null || settings.enterPlayModeWay != EnterPlayModeWay.FrameworkStart) return;
            
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                string frameworkScenePath = settings.frameworkScenePath;
                if (string.IsNullOrEmpty(frameworkScenePath))
                {
                    Debug.LogError("Framework Scene Path is empty. Check your framework settings.");
                    return;
                }
                
                originalScenes = EditorSceneManager.GetSceneManagerSetup();
                SessionState.SetString(sessionKey, JsonUtility.ToJson(new SceneSetupContainer(originalScenes)));
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(frameworkScenePath, OpenSceneMode.Single);
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                string jsonData = SessionState.GetString(sessionKey, "{}");
                SceneSetupContainer container = JsonUtility.FromJson<SceneSetupContainer>(jsonData);
                if (container.Scenes != null && container.Scenes.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(container.Scenes);
                }
                else
                {
                    Debug.LogError("No original scenes to restore.");
                }
            }
        }
    }

    [Serializable]
    class SceneSetupContainer
    {
        public SceneSetup[] Scenes;
        public SceneSetupContainer(SceneSetup[] scenes)
        {
            Scenes = scenes;
        }
    }
}
