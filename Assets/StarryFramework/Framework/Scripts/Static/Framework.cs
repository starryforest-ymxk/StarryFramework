using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public static class Framework
    {

        static public TimerComponent TimerComponent => FrameworkComponent.GetComponent<TimerComponent>();

        static public SaveComponent SaveComponent => FrameworkComponent.GetComponent<SaveComponent>();

        static public SceneComponent SceneComponent => FrameworkComponent.GetComponent<SceneComponent>();

        static public EventComponent EventComponent => FrameworkComponent.GetComponent<EventComponent>();

        static public ResourceComponent ResourceComponent => FrameworkComponent.GetComponent<ResourceComponent>();

        static public AudioComponent AudioComponent => FrameworkComponent.GetComponent<AudioComponent>();

        static public ObjectPoolComponent ObjectPoolComponent => FrameworkComponent.GetComponent<ObjectPoolComponent>();

        static public FSMComponent FSMComponent => FrameworkComponent.GetComponent<FSMComponent>();


        static public void ShutDown(ShutdownType shutdownType)
        {
            FrameworkComponent.Shutdown(shutdownType);
        }
    }
}
