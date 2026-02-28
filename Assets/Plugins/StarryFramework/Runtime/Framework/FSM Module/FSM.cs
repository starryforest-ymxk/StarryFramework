using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarryFramework
{

    public abstract class IVariable 
    {
        protected readonly string _key;

        public IVariable(string key)
        {
            this._key = key;
        }

        public string Key => _key;

        public abstract string GetValueString();

        public abstract Type Type { get; }

        public abstract bool CompareType(Type type);
    }
    public class Variable<T>: IVariable
    {
        private readonly T _value;

        public Variable(string key, T value): base(key)
        {
            this._value = value;
        }

        public T Value => _value;

        public override string GetValueString()
        {
            return _value.ToString();
        }

        public override Type Type => typeof(T);

        public override bool CompareType(Type type)
        {
            return type == typeof(T);
        }

    }



    public class FSM<T> : FSMBase, IFSM<T> where T : class
    {
        private readonly T _owner;

        private readonly Dictionary<Type, FSMState<T>> _states;

        private readonly Dictionary<string, IVariable> _data;

        private FSMState<T> _currentState;

        private float _currentStateTime;

        private bool _isRunning;

        private bool _isDestroyed;

        private FSM(string name, T owner, List<FSMState<T>> list)
        {

            if (name == null)
            {
                FrameworkManager.Debugger.LogError("Name of FSM can not be null.");
                return;
            }

            if (owner == null)
            {
                FrameworkManager.Debugger.LogError("FSM owner is invalid.");
                return;
            }

            if (list == null || list.Count < 1)
            {
                FrameworkManager.Debugger.LogError("FSM list is invalid.");
                return;
            }

            Name = name;
            _owner = owner;
            _isDestroyed = false;
            _isRunning = false;
            _currentState = null;
            _currentStateTime = 0f;
            _data = new Dictionary<string, IVariable>();
            _states = new Dictionary<Type, FSMState<T>>();
            foreach (FSMState<T> state in list)
            {
                if (state == null)
                {
                    FrameworkManager.Debugger.LogError("FSM state is invalid.");
                    continue;
                }

                Type type = state.GetType();
                if (_states.ContainsKey(type))
                {
                    FrameworkManager.Debugger.LogError($"FSM '{name}' state '{type}' is already exist.");
                    continue;
                }

                _states.Add(type, state);
                state.OnInit(this);
            }
        }

        #region Basic

        public static FSM<T> Create(string name, T owner, List<FSMState<T>> list)
        {
            return new FSM<T>(name, owner, list);
        }

        public static FSM<T> Create(string name, T owner, FSMState<T>[] states)
        {
            return new FSM<T>(name, owner, new List<FSMState<T>>(states));
        }

        public string GetFullName()
        {
            return FullName;
        }

        public string GetName()
        {
            return Name;
        }

        public T GetOwner()
        {
            return _owner;
        }

        public override Type OwnerType()
        {
            return typeof(T);   
        }

        public override bool IsRunning()
        {
            return _isRunning;
        }

        public override bool IsDestroyed()
        {
            return _isDestroyed;
        }

        #endregion

        #region State

        public bool HasState(Type stateType)
        {
            if(stateType == null)
            {
                FrameworkManager.Debugger.LogWarning("TimerState type is null");
                return false;
            }
            else
            {
                return _states.ContainsKey(stateType);
            }
        }

        public bool HasState<S>() where S : FSMState<T>
        {
            return HasState(typeof(S));
        }

        public override int GetStateCount()
        {
            return _states.Count;
        }
        public FSMState<T> GetState(Type stateType)
        {
            if (stateType == null)
            {
                FrameworkManager.Debugger.LogError("TimerState type can't be null");
                return null;
            }
            else if (!_states.ContainsKey(stateType))
            {
                FrameworkManager.Debugger.LogError($"FSM {Name} doesn 't have state {stateType}.");
                return null;
            }
            else 
                return _states[stateType];
        }

        public S GetState<S>() where S : FSMState<T>
        {
            return GetState(typeof(S)) as S;
        }

        public List<FSMState<T>> GetAllStates()
        {
            List<FSMState<T>> list = new List<FSMState<T>>();
            foreach (var typeStatePair in _states)
            {
                list.Add(typeStatePair.Value);
            }
            return list;
        }

        public FSMState<T> GetCurrentState()
        {
            if (_isRunning)
                return _currentState;
            else
            {
                FrameworkManager.Debugger.Log($"FSM {Name} has not started yet.");
                return null;
            }
        }


        public override string GetCurrentStateName()
        {
            if (_isRunning)
                return _currentState.GetType().ToString();
            else
            {
                FrameworkManager.Debugger.Log($"FSM {Name} has not started yet.");
                return null;
            }
        }

        public override float GetCurrentStateTime()
        {
            if (_isRunning)
                return _currentStateTime;
            else
            {
                FrameworkManager.Debugger.Log($"FSM {Name} has not started yet.");
                return 0f;
            }
        }

        #endregion

        #region Data

        public bool HasData(string key)
        {
            if(key == null)
            {
                FrameworkManager.Debugger.LogError("key can't be null.");
                return false;
            }
            else
                return _data.ContainsKey(key);
        }

        public D GetData<D>(string key)
        {
            if (key == null)
            {
                FrameworkManager.Debugger.LogError("key can't be null.");
                return default;
            }
            else if(!_data.ContainsKey(key))
            {
                FrameworkManager.Debugger.LogError("FSM Doesn't contain variable.");
                return default;
            }
            else
            {
                IVariable variable = _data[key];
                if (!variable.CompareType(typeof(D)))
                {
                    FrameworkManager.Debugger.LogError($"the type of variable is not \"{typeof(D)}\".");
                    return default;
                }
                else
                    return (variable as Variable<D>).Value;
            }
        }

        public void SetData<D>(string key, D data)
        {
            if (key == null)
            {
                FrameworkManager.Debugger.LogError("key can't be null.");
                return;
            }
            else if (!_data.ContainsKey(key))
            {
                _data.Add(key, new Variable<D>(key,data));
            }
            else
            {
                _data[key] = new Variable<D>(key,data);
            }
        }

        public void RemoveData(string key)
        {
            if (key == null)
            {
                FrameworkManager.Debugger.LogError("key can't be null.");
            }
            else if(!_data.ContainsKey(key))
            {
                FrameworkManager.Debugger.LogWarning("FSM Doesn't contain variable.");
            }
            else
            {
                _data.Remove(key);
            }
        }

        public override IVariable[] GetAllData()
        {
            return _data.Values.ToArray();
        }

        #endregion

        #region Procedure

        public void Start<S>() where S : FSMState<T>
        {
            Start(typeof(S));
        }

        public void Start(Type stateType)
        {
            if (_isRunning)
            {
                FrameworkManager.Debugger.LogWarning($"FSM {Name} is running, can not start again.");
            }

            if (stateType == null)
            {
                FrameworkManager.Debugger.LogError("TimerState type can not be null.");
            }

            if (!_states.ContainsKey(stateType))
            {
                FrameworkManager.Debugger.LogError($"TimerState {stateType} is invalid.");
                return;
            }

            _currentStateTime = 0f;
            _currentState = _states[stateType];
            _currentState.OnEnter(this);
            _isRunning = true;
        }

        public override void Update()
        {
            if (_isRunning)
            {
                _currentStateTime += Time.deltaTime;
                _currentState.OnUpdate(this);
            }
        }

        public override void Shutdown()
        {
            if (_isDestroyed)
            {
                return;
            }

            if (_isRunning)
            {
                _currentState.OnLeave(this, true);
                _currentStateTime = 0;
                _currentState = null;
                _isRunning = false;
            }
            foreach (var typeStatePair in _states)
            {
                typeStatePair.Value.OnDestroy(this);
            }
            Name = null;
            _states.Clear();
            _data.Clear();
            _isDestroyed = true;
        }

        #endregion

        #region ChangeState
        public void ChangeState<S>() where S : FSMState<T>
        {
             ChangeState(typeof(S));    
        }

        public void ChangeState(Type stateType)
        {
            if (_isDestroyed)
            {
                FrameworkManager.Debugger.LogError($"FSM {Name} has been released.");
                return;
            }
            if(!_isRunning)
            {
                FrameworkManager.Debugger.LogError($"FSM {Name} has not started.");
                return;
            }
            if(stateType == null) 
            {
                FrameworkManager.Debugger.LogError($"TimerState type can not be null.");
                return;
            }
            if(!_states.ContainsKey(stateType))
            {
                FrameworkManager.Debugger.LogError($"TimerState {stateType} is invalid.");
                return;
            }

            _currentState.OnLeave(this, false);
            _currentStateTime = 0f;
            _currentState = _states[stateType];
            _currentState.OnEnter(this);

        }
        #endregion
    }
}

