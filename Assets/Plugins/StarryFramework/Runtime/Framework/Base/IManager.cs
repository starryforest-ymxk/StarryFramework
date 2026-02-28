using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    internal interface IManager
    {
        internal void Awake();
        internal void Init();
        internal void Update();
        internal void ShutDown();
    }

    internal interface IConfigurableManager
    {
        internal void SetSettings(IManagerSettings settings);
    }

    internal interface IManagerSettings
    {
        
    }
}
