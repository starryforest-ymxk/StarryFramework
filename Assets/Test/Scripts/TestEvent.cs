using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;

public class TestEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Framework.EventComponent.AddEventListener("Event1", Event1);
        Framework.EventComponent.AddEventListener<int,string>("Event1", Event1);
        Framework.EventComponent.AddEventListener<Object>("Event2", Event2);
        Framework.EventComponent.AddEventListener("Event3", Event3);
    }

    public void Event1() { }
    public void Event1(int a, string b) { }
    public void Event2(Object _object) { }
    public void Event3() { }

}
