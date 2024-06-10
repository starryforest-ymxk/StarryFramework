using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;

public class TestTimer : MonoBehaviour
{
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            Framework.TimerComponent.RegisterTimer().Start();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Framework.TimerComponent.RegisterTriggerTimer(2f, () => { Debug.Log("This is trigger timer."); });
        }
    }
}
