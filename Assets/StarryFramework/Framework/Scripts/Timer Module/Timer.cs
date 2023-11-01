using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    public class Timer :ITimer
    {
        private string _name;
        private float _timer;
        private float _startValue;
        private bool _ignoreTimeScale;
        private TimerState _state;


        private UnityAction UpdateAction;

        public string Name => _name;
        public float Time => _timer;
        public float StartValue => _startValue;
        public bool IgnoreTimeScale => _ignoreTimeScale;
        public TimerState TimerState => _state;

#if UNITY_EDITOR

        private bool _foldout = false;
        public bool Foldout { get => _foldout; set => _foldout = value; }
#endif

        internal Timer(bool ignoreTimeScale, string name, float startValue)
        {
            _ignoreTimeScale = ignoreTimeScale;
            _startValue = startValue;
            _state = TimerState.Ready;
            _name = name;
        }

        internal void Update()
        {
            if (_state == TimerState.Active)
            {
                _timer += _ignoreTimeScale? UnityEngine.Time.unscaledDeltaTime : UnityEngine.Time.deltaTime;
                UpdateAction?.Invoke();
            }
        }

        public void BindUpdateAction(UnityAction action)
        {
            UpdateAction += action;
        }

        public void Pause()
        {
            _state = TimerState.Pause;
        }

        public void Resume()
        {
            _state = TimerState.Active;
        }

        public void Stop()
        {
            _timer = _startValue;
            _state = TimerState.Stop;
        }

        public void Start()
        {
            _timer = _startValue;
            _state = TimerState.Active;
        }

        public void Reset()
        {
            _timer = _startValue;
        }

    }
}
