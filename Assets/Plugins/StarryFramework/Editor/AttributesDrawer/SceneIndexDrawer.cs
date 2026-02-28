using UnityEditor;
using UnityEngine;

namespace StarryFramework.Editor
{

    [CustomPropertyDrawer(typeof(SceneIndexAttribute))]
    public class SceneIndexDrawer : PropertyDrawer
    {
        int sceneIndex = -1;
        GUIContent[] sceneNames;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (sceneIndex == -1)
            {
                GetSceneNameArray(property);
            }

            sceneIndex = property.intValue;
            
            string key = $"{property.serializedObject.targetObject.GetInstanceID()}.{property.propertyPath}";
            string sceneName = EditorPrefs.GetString(key, string.Empty);

            if (!string.IsNullOrEmpty(sceneName) && sceneName != sceneNames[sceneIndex].text)
            {
                for(int i = 0; i<sceneNames.Length;i++)
                {
                    if (sceneName == sceneNames[i].text) 
                        sceneIndex = i;
                }
            }

            if (sceneIndex >= sceneNames.Length)
            {
                sceneIndex = sceneNames.Length - 1;
                sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);
                EditorPrefs.SetString(key, sceneNames[sceneIndex].text);
                EditorGUILayout.HelpBox($"scene index is out of bounds. Set index to {sceneIndex}.", MessageType.Warning);
            }
            else
            {
                sceneIndex = EditorGUI.Popup(position, label, sceneIndex, sceneNames);
                EditorPrefs.SetString(key, sceneNames[sceneIndex].text);
            }
            property.intValue = sceneIndex;

        }
    

        private void GetSceneNameArray(SerializedProperty property)
        {
            var scenes = EditorBuildSettings.scenes;
            int deletedScenes = 0;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (!System.IO.File.Exists(scenes[i].path))
                    deletedScenes++;
            }
            //初始化数组
            sceneNames = new GUIContent[scenes.Length-deletedScenes];

            for (int i = 0, index = 0; i < scenes.Length; i++)
            {
                if (!System.IO.File.Exists(scenes[i].path)) continue;
                string sceneName = Utilities.ScenePathToName(scenes[i].path);
                sceneNames[index++] = new GUIContent(sceneName);
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
