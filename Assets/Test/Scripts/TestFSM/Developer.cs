using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;

public class Developer : MonoBehaviour
{
    private IFSM<Developer> fsm;
    private IFSM<Developer> fsm2;
    private void Start()
    {
        fsm = Framework.FSMComponent.CreateFSM("Game Engine", this, new FSMState<Developer>[] { new TestState1(), new TestState2() });
        fsm.SetData<int>("testDataInt", 1);
        fsm.SetData<string>("testDataString", "test");

        fsm2 = Framework.FSMComponent.CreateFSM("666", this, new FSMState<Developer>[] { new TestState1(), new TestState2() });
        fsm2.SetData<int>("888", 8);
        fsm2.SetData<string>("6666", "2233 haha");

    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            fsm.Start<TestState1>();
        }
    }
}
