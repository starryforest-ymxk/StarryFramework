using StarryFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour
{
    [SceneIndex] public int scene;

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            Framework.SceneComponent.ChangeSceneDefault("TestScene");
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            Framework.SceneComponent.ChangeSceneProgressBar("TestScene", "TestScene", "ExampleBar");
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha1))
        {
            Framework.SceneComponent.LoadScene("TestScene1", null, false);
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            Framework.SceneComponent.LoadScene("TestScene2", null, false);
        }
        
        if (Input.GetKeyUp(KeyCode.Alpha3))
        {
            Framework.SceneComponent.LoadScene("TestScene3", null, false);
        }
        
        if (Input.GetKeyUp(KeyCode.F1))
        {
            Framework.SceneComponent.UnloadScene("TestScene1");
        }
        
        if (Input.GetKeyUp(KeyCode.F2))
        {
            Framework.SceneComponent.UnloadScene("TestScene2");
        }
        
        if (Input.GetKeyUp(KeyCode.F3))
        {
            Framework.SceneComponent.UnloadScene("TestScene3");
        }
    }
}
