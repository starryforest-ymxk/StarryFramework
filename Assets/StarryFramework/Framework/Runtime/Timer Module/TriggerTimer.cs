using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{

    public class TriggerTimer
    {
        private string _name;
        private float _timeDelta;
        private float _startTime;
        private bool _ignoreTimeScale;
        private bool _repeat;
        private TimerState _state;


        private readonly UnityAction action;

        public string Name => _name;
        public float TimeDelta => _timeDelta;
        public float StartTime => _startTime;
        public bool IgnoreTimeScale => _ignoreTimeScale;
        public bool Repeat => _repeat;
        public TimerState TimerState => _state;

#if UNITY_EDITOR

        private bool _foldout = false;
        public bool Foldout { get => _foldout; set => _foldout = value; }
#endif




        internal TriggerTimer(float timeDelta, UnityAction action, bool ignoreTimeScale = false, bool repeat = false, string name = null)
        {
            this._timeDelta = timeDelta;
            this._ignoreTimeScale = ignoreTimeScale;
            this._repeat = repeat;
            this._name = name;
            this.action = action;
            _state = TimerState.Ready;
        }

        internal void Update()
        {
            if(_state == TimerState.Active)
            {
                if ((_ignoreTimeScale?Time.unscaledTime:Time.time) >= _startTime + _timeDelta) Trigger();
            }
        }

        internal void Start()
        {
            _startTime = _ignoreTimeScale ? Time.unscaledTime : Time.time;
            _state = TimerState.Active;
        }

        internal void Stop()
        {
            _state = TimerState.Stop;
        }

        private void Trigger()
        {
            if (_repeat) 
                Start();
            else
                _state = TimerState.Stop;
            action?.Invoke();
        }
        internal void Pause()
        {
            _state = TimerState.Pause;
        }

        internal void Resume()
        {
            _state = TimerState.Active;
        }
    }
}

