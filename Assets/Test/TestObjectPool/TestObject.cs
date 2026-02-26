using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarryFramework;

public class TestObject : GameObjectBase
{
    [FoldOutGroup("对象池属性")]
    public bool UseObjectPool;
    private void Update()
    {
        if(transform.position.y<-90)
            Framework.ObjectPoolComponent.Recycle(this);
    }
    public override void OnSpawn()
    {
        base.OnSpawn();
    }

    public override void OnUnspawn()
    {
        base.OnUnspawn();
    }

    public override void OnRelease()
    {
        base.OnRelease();
    }
}
