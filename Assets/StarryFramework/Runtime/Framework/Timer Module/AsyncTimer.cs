using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{

    public class AsyncTimer 
    {
        private readonly System.Timers.Timer _timer;
        private string _name;
        private TimerState _state;

        public string Name => _name;
        public TimerState TimerState => _state;


        internal AsyncTimer(float timeDelta, UnityAction action, bool repeat = false, string name = null)
        {
            this._timer = new System.Timers.Timer(timeDelta * 1000);//ת���ɺ���
            this._name = name;
            _timer.AutoReset = repeat;
            _timer.Elapsed += (s,e)=> 
            {
                if (!repeat) _state = TimerState.Stop;
                else _state = TimerState.Active;
                action.Invoke();
            };
        }

        internal void Start()
        {
            _timer.Enabled= true;
            _state = TimerState.Active;
        }

        internal void Stop()
        {
            _timer.Enabled= false;
            _state = TimerState.Stop;
        }

        internal void Close()
        {
            _timer.Close();
        }
    }


}
