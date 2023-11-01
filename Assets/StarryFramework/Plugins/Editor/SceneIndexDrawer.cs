using UnityEditor;
using UnityEngine;



namespace StarryFramework.StarryUnityEditor
{

    [CustomPropertyDrawer(typeof(SceneIndexAttribute))]
    public class SceneIndexDrawer : PropertyDrawer
    {
        int sceneIndex = -1;

        GUIContent[] sceneNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            if (sceneIndex == -1)
                GetSceneNameArray(property);

            sceneIndex = property.intValue;

            sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);

            property.intValue = sceneIndex;

        }
    

        private void GetSceneNameArray(SerializedProperty property)
        {
            var scenes = EditorBuildSettings.scenes;
            //初始化数组
            sceneNames = new GUIContent[scenes.Length];

            for (int i = 0; i < sceneNames.Length; i++)
            {

                string sceneName = Utilities.ScenePathToName(scenes[i].path);

                if (sceneName == null)
                {
                    sceneName = "(Deleted Scene)";
                }

                sceneNames[i] = new GUIContent(sceneName);
            }

            if (sceneNames.Length == 0)
            {
                sceneNames = new[] { new GUIContent("Check Your Build Settings") };
            }

            sceneIndex = 0;

            for (int i = 0; i < sceneNames.Length; i++)
            {
                 if (i == property.intValue)
                 {
                     sceneIndex = i;
                     break;
                 }
            }
        }
    }
}
