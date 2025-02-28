using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;

public class TestState1 : FSMState<Developer>
{
    protected internal override void OnLeave(IFSM<Developer> fsm, bool isShutdown)
    {
        Debug.Log("Leave Unity!");
    }

    protected internal override void OnUpdate(IFSM<Developer> fsm)
    {
        Debug.Log("Fuck Unity!");

        if(fsm.GetCurrentStateTime() > 10f)
        {
            ChangeState<TestState2>(fsm);
        }
    }
}
