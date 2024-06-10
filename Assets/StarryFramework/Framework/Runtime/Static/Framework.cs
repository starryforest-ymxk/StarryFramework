using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public static partial class Framework
    {

        public static TimerComponent TimerComponent => FrameworkComponent.GetComponent<TimerComponent>();
        public static SaveComponent SaveComponent => FrameworkComponent.GetComponent<SaveComponent>();
        public static SceneComponent SceneComponent => FrameworkComponent.GetComponent<SceneComponent>();
        public static EventComponent EventComponent => FrameworkComponent.GetComponent<EventComponent>();
        public static ResourceComponent ResourceComponent => FrameworkComponent.GetComponent<ResourceComponent>();
        public static ObjectPoolComponent ObjectPoolComponent => FrameworkComponent.GetComponent<ObjectPoolComponent>();
        public static FSMComponent FSMComponent => FrameworkComponent.GetComponent<FSMComponent>();
        
        public static void ShutDown(ShutdownType shutdownType)
        {
            FrameworkComponent.Shutdown(shutdownType);
        }
    }
}
