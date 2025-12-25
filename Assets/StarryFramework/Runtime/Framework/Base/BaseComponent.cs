using System.Runtime.CompilerServices;
using UnityEngine;


namespace StarryFramework
{
    public abstract class BaseComponent : MonoBehaviour
    {
        protected virtual void Awake()
        {
            FrameworkComponent.RegisterComponent(this);
        }

        internal virtual void Shutdown() { }

        internal virtual void DisableProcess()
        {
            FrameworkComponent.DeleteComponent(this);
        }
        
    }
}

