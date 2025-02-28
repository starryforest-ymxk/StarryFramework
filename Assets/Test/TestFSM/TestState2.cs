using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;


public class TestState2 : FSMState<Developer>
{
    protected override void OnEnter(IFSM<Developer> fsm)
    {
        Debug.Log("Go to Godot!");
    }

    protected override void OnUpdate(IFSM<Developer> fsm)
    {
        Debug.Log("Love Godot!");
    }
}
