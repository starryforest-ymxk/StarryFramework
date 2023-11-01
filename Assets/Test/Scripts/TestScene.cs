using StarryFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScene : MonoBehaviour
{

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.A))
        {
            Framework.SceneComponent.ChangeSceneDefault("TestSave");
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            Framework.SceneComponent.ChangeSceneProgressBar("TestScene", "TestScene", "ExampleBar");
        }
    }
}
