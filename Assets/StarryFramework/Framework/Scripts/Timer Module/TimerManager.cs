using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace StarryFramework
{
    public class TimerManager : IManager
    {

        private Dictionary<string, Timer> timersDic = new Dictionary<string, Timer>();
        internal List<Timer> timers = new List<Timer>();

        internal List<Timer> tempAddTimers = new List<Timer>();
        internal List<Timer> tempDeleteTimers = new List<Timer>();


        private Dictionary<string, TriggerTimer> triggerTimersDic = new Dictionary<string, TriggerTimer>();
        internal List<TriggerTimer> triggerTimers = new List<TriggerTimer>();
        
        internal List<TriggerTimer> tempAddTriggerTimers = new List<TriggerTimer>();
        internal List<TriggerTimer> tempDeleteTriggerTimers = new List<TriggerTimer>();
        
        internal TriggerTimer unusedTriggerTimersClear;


        private Dictionary<string, AsyncTimer> asyncTimersDic = new Dictionary<string, AsyncTimer>();
        internal List<AsyncTimer> asyncTimers = new List<AsyncTimer>();
        internal AsyncTimer unusedAsyncTimersClear;

        private float clearUnusedTriggerTimersInterval;
        private float clearUnusedAsyncTimersInterval;

        internal float ClearUnusedTriggerTimersInterval => clearUnusedTriggerTimersInterval;
        internal float ClearUnusedAsyncTimersInterval => clearUnusedAsyncTimersInterval;

        void IManager.Awake()
        {

        }

        void IManager.Init()
        {
            clearUnusedTriggerTimersInterval = FrameworkManager.setting.TimerSettings.ClearUnusedTriggerTimersInterval;
            clearUnusedAsyncTimersInterval = FrameworkManager.setting.TimerSettings.ClearUnusedAsyncTimersInterval;
            unusedTriggerTimersClear  = new TriggerTimer(clearUnusedTriggerTimersInterval, ClearUnusedTriggerTimers, false, true, "UnusedTriggerTimersClear");
            unusedAsyncTimersClear = new AsyncTimer(clearUnusedAsyncTimersInterval, ClearUnusedAsyncTimers, true, "UnusedAsyncTimersClear");
            unusedTriggerTimersClear.Start();
            unusedAsyncTimersClear.Start();
        }

        void IManager.Update()
        {
            unusedTriggerTimersClear.Update();

            if (tempAddTriggerTimers.Count > 0)
            {
                tempAddTriggerTimers.ForEach((a) => { triggerTimers.Add(a); });
                tempAddTriggerTimers.Clear();
            }
            if (tempDeleteTriggerTimers.Count > 0)
            {
                tempDeleteTriggerTimers.ForEach((a) => { triggerTimers.Remove(a); });
                tempDeleteTriggerTimers.Clear();
            }
            foreach (var triggerTimer in triggerTimers)
            {
                triggerTimer.Update();
            }

            //使用tempAddTimers和tempDeleteTimers是避免在timer中的回调绑定了注册或删除逻辑，导致foreach中改变list的错误
            if (tempAddTimers.Count > 0)
            {
                tempAddTimers.ForEach((a) => { timers.Add(a); });
                tempAddTimers.Clear();
            }
            if (tempDeleteTimers.Count > 0)
            {
                tempDeleteTimers.ForEach((a) => { timers.Remove(a); });
                tempDeleteTimers.Clear();
            }
            foreach (var timer in timers)
            {
                timer.Update();
            }
        }

        void IManager.ShutDown()
        {
            CloseAllAsyncTimers();
            timersDic.Clear();
            timers.Clear();
            tempAddTimers.Clear();
            tempDeleteTimers.Clear();
            triggerTimersDic.Clear();
            triggerTimers.Clear();
            asyncTimersDic.Clear();
            asyncTimers.Clear();
            unusedTriggerTimersClear = null;
            unusedAsyncTimersClear.Close();
            unusedAsyncTimersClear = null;
        }

        #region Timer       
        internal Timer RegisterTimer(bool ignoreTimeScale, float startValue = 0f, UnityAction action = null)
        {
            Timer timer = new Timer(ignoreTimeScale,"", startValue);
            tempAddTimers.Add(timer);
            timer.BindUpdateAction(action);
            return timer;
        }

        internal void DeleteTimer(Timer timer)
        {
            
            if (timers.Contains(timer))
            {
                tempDeleteTimers.Add(timer);
            }
            else
            {
                Debug.LogError($"Timer doesn't exist.");
            }

        }

        internal void RegisterTimer(string name, bool ignoreTimeScale, float startValue = 0f)
        {
            if (name == null || name == "")
            {
                Debug.LogError("Timer Name can not be null or empty");
                return;
            }
            Timer timer = new(ignoreTimeScale, name, startValue);
            timersDic.Add(name, timer);
            tempAddTimers.Add(timer);
        }

        internal void DeleteTimer(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                timersDic.Remove(name);
                tempDeleteTimers.Add(timer);
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
            }

        }
        internal void BindUpdateAction(string name, UnityAction action)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                timer.BindUpdateAction(action);
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
            }
        }

        internal TimerState GetTimerState(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                return timer.TimerState;
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
                return TimerState.Null;
            }
        }

        internal float GetTimerTime(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                return timer.Time;
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
                return 0f;
            }
        }

        internal void PauseTimer(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                timer.Pause();
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
            }
        }

        internal void ActivateTimer(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                timer.Resume();
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
            }
        }

        internal void StopTimer(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                timer.Stop();
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
            }
        }

        internal void StartTimer(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                timer.Start();
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
            }
        }

        internal void ResetTimer(string name)
        {
            if (timersDic.ContainsKey(name))
            {
                Timer timer = timersDic[name];
                timer.Reset();
            }
            else
            {
                Debug.LogError($"Timer[{name}] doesn't exist.");
            }
        }
        #endregion

        #region TriggerTimer

        internal void RegisterTriggerTimer(float timeDelta, UnityAction action, bool ignoreTimeScale = false, bool repeat = false, string name = "")
        {
            if(name == null)
            {
                Debug.LogError("Name of trigger timer can not be null.");
                return;
            }
            TriggerTimer triggerTimer = new TriggerTimer(timeDelta, action, ignoreTimeScale, repeat, name);
            if(name!= "") { triggerTimersDic.Add(name, triggerTimer); }
            else triggerTimer.Start();
            tempAddTriggerTimers.Add(triggerTimer);
        }

        internal void DeleteTriggerTimer(string name)
        {
            if (triggerTimersDic.ContainsKey(name))
            {
                TriggerTimer triggerTimer = triggerTimersDic[name];
                triggerTimer.Stop();
                triggerTimersDic.Remove(name);
                tempDeleteTriggerTimers.Add(triggerTimer);
            }
            else
            {
                Debug.LogError($"TriggerTimer[{name}] doesn't exist.");
            }

        }

        internal TimerState GetTriggerTimerState(string name)
        {
            if (triggerTimersDic.ContainsKey(name))
            {
                TriggerTimer triggerTimer = triggerTimersDic[name];
                return triggerTimer.TimerState;
            }
            else
            {
                Debug.LogError($"TriggerTimer[{name}] doesn't exist.");
                return TimerState.Null;
            }
        }

        internal void PauseTriggerTimer(string name)
        {
            if (triggerTimersDic.ContainsKey(name))
            {
                TriggerTimer triggerTimer = triggerTimersDic[name];
                triggerTimer.Pause();
            }
            else
            {
                Debug.LogError($"TriggerTimer[{name}] doesn't exist.");
            }
        }

        internal void ActivateTriggerTimer(string name)
        {
            if (triggerTimersDic.ContainsKey(name))
            {
                TriggerTimer triggerTimer = triggerTimersDic[name];
                triggerTimer.Resume();
            }
            else
            {
                Debug.LogError($"TriggerTimer[{name}] doesn't exist.");
            }
        }

        internal void StopTriggerTimer(string name)
        {
            if (triggerTimersDic.ContainsKey(name))
            {
                TriggerTimer triggerTimer = triggerTimersDic[name];
                triggerTimer.Stop();
            }
            else
            {
                Debug.LogError($"TriggerTimer[{name}] doesn't exist.");
            }
        }

        internal void StartTriggerTimer(string name)
        {
            if (triggerTimersDic.ContainsKey(name))
            {
                TriggerTimer triggerTimer = triggerTimersDic[name];
                triggerTimer.Start();
            }
            else
            {
                Debug.LogError($"TriggerTimer[{name}] doesn't exist.");
            }
        }

        private void ClearUnusedTriggerTimers()
        {
            for(int i = 0; i<triggerTimers.Count; i++)
            {
                var triggerTimer = triggerTimers[i];
                if(triggerTimer.TimerState == TimerState.Stop && triggerTimer.Name == "")
                {
                    triggerTimers.Remove(triggerTimer);
                    i--;
                }
            }
        }

        internal void ClearUnnamedTriggerTimers()
        {
            for (int i = 0; i < triggerTimers.Count; i++)
            {
                var triggerTimer = triggerTimers[i];
                if (triggerTimer.Name == "")
                {
                    triggerTimers.Remove(triggerTimer);
                    i--;
                }
            }
        }



        #endregion

        #region AsyncTimer

        internal void RegisterAsyncTimer(float timeDelta, UnityAction action, bool repeat = false, string name = "")
        {
            if (name == null)
            {
                Debug.LogError("Name of async timer can not be null.");
                return;
            }
            AsyncTimer asyncTimer = new AsyncTimer(timeDelta, action, repeat, name);
            if (name != "") { asyncTimersDic.Add(name, asyncTimer); }
            else asyncTimer.Start();
            asyncTimers.Add(asyncTimer);
        }

        internal void DeleteAsyncTimer(string name)
        {
            if (asyncTimersDic.ContainsKey(name))
            {
                AsyncTimer asyncTimer = asyncTimersDic[name];
                asyncTimer.Close();
                asyncTimers.Remove(asyncTimer);
                asyncTimersDic.Remove(name);
            }
            else
            {
                Debug.LogError($"AsyncTimer[{name}] doesn't exist.");
            }

        }

        internal TimerState GetAsyncTimerState(string name)
        {
            if (asyncTimersDic.ContainsKey(name))
            {
                AsyncTimer asyncTimer = asyncTimersDic[name];
                return asyncTimer.TimerState;
            }
            else
            {
                Debug.LogError($"AsyncTimer[{name}] doesn't exist.");
                return TimerState.Null;
            }
        }

        internal void StartAsyncTimer(string name)
        {
            if (asyncTimersDic.ContainsKey(name))
            {
                AsyncTimer asyncTimer = asyncTimersDic[name];
                asyncTimer.Start();
            }
            else
            {
                Debug.LogError($"AsyncTimer[{name}] doesn't exist.");
            }
        }

        internal void StopAsyncTimer(string name)
        {
            if (asyncTimersDic.ContainsKey(name))
            {
                AsyncTimer asyncTimer = asyncTimersDic[name];
                asyncTimer.Stop();
            }
            else
            {
                Debug.LogError($"AsyncTimer[{name}] doesn't exist.");
            }
        }

        private void ClearUnusedAsyncTimers()
        {
            for (int i = 0; i < asyncTimers.Count; i++)
            {
                var asyncTimer = asyncTimers[i];
                if (asyncTimer.TimerState == TimerState.Stop && asyncTimer.Name == "")
                {
                    asyncTimer.Close();
                    asyncTimers.Remove(asyncTimer);
                    i--;
                }
            }
        }

        internal void ClearUnnamedAsyncTimers()
        {
            for (int i = 0; i < asyncTimers.Count; i++)
            {
                var asyncTimer = asyncTimers[i];
                if (asyncTimer.Name == "")
                {
                    asyncTimer.Close();
                    asyncTimers.Remove(asyncTimer);
                    i--;
                }
            }
        }

        private void CloseAllAsyncTimers()
        {
            foreach(var timer in asyncTimers)
            {
                timer.Close();
            }
        }



        #endregion
    }
}

