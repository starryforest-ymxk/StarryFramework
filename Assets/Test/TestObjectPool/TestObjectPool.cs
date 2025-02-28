using StarryFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObjectPool : MonoBehaviour
{
    public GameObject GameObject;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.Log("Register");
            Framework.ObjectPoolComponent.Register<TestObject>(GameObject, 10, 5);
            GameObject.GetComponent<TestObject>().UseObjectPool = true;
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("Insantiate");
            GameObject obj = Framework.ObjectPoolComponent.Require<TestObject>().gameObject;
            Vector3 a = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(a);
            obj.transform.position = new Vector3(a.x, a.y, 0);
        }
    }
}
