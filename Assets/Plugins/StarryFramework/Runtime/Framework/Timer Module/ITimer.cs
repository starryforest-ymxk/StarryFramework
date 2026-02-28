using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    public interface ITimer
    {
        float Time { get; }

        TimerState TimerState {  get; }

        void BindUpdateAction(UnityAction action);

        void Pause();

        void Resume();

        void Start();

        void Stop();

        void Reset();

    }
}

