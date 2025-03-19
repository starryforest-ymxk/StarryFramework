using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace StarryFramework
{
    public class FSMManager : IManager
    {
        private Dictionary<TypeNamePair, FSMBase> _fsms = new Dictionary<TypeNamePair, FSMBase>();

        private List<FSMBase> _fsmList = new List<FSMBase>();

        private List<FSMBase> _tempAddList = new List<FSMBase>();

        private List<FSMBase> _tempRemoveList = new List<FSMBase>();


        void IManager.Awake()
        {

        }

        void IManager.Init()
        {

        }
        void IManager.Update()
        {
            if(_tempAddList.Count > 0)
            {
                foreach (var fsm in _tempAddList)
                {
                    _fsmList.Add(fsm);
                }

                _tempAddList.Clear();
            }


            if(_tempRemoveList.Count > 0)
            {
                foreach (var fsm in _tempRemoveList)
                {
                    _fsmList.Remove(fsm);
                }

                _tempRemoveList.Clear();
            }


            if ( _fsmList.Count > 0 )
            {
                foreach (FSMBase fsm in _fsmList)
                {
                    fsm.Update();
                }
            }

        }

        void IManager.ShutDown()
        {
            if (_tempAddList.Count > 0)
            {
                foreach (var fsm in _tempAddList)
                {
                    _fsmList.Add(fsm);
                }

                _tempAddList.Clear();
            }


            if (_tempRemoveList.Count > 0)
            {
                foreach (var fsm in _tempRemoveList)
                {
                    _fsmList.Remove(fsm);
                }

                _tempRemoveList.Clear();
            }

            foreach (FSMBase fsm in _fsmList)
            {
                fsm.Shutdown();
            }

            _fsmList.Clear();
            _fsms.Clear();

            _fsms = null;
            _fsmList = null;
            _tempAddList = null;
            _tempRemoveList = null;
        }

        void IManager.SetSettings(IManagerSettings settings) { }

        public int FSMCount => _fsms.Count;

        public IFSM<T> CreateFSM<T>(string name, T owner, List<FSMState<T>> states) where T : class
        {
            FSM<T> fsm = FSM<T>.Create(name, owner, states);
            TypeNamePair key = new TypeNamePair(typeof(T), name);
            if (fsm != null) 
            { 
                if(_fsms.ContainsKey(key))
                {
                    FrameworkManager.Debugger.LogError($"FSM called {key} has existed. ");
                }
                else
                {
                    _fsms.Add(key, fsm);
                    _tempAddList.Add(fsm);
                }
            }
            return fsm;
        }

        public IFSM<T> CreateFSM<T>(string name, T owner, FSMState<T>[] states) where T : class
        {
            FSM<T> fsm = FSM<T>.Create(name, owner, states);
            TypeNamePair key = new TypeNamePair(typeof(T), name);
            if (fsm != null)
            {
                if (_fsms.ContainsKey(key))
                {
                    FrameworkManager.Debugger.LogError($"FSM called {key} has existed. ");
                }
                else
                {
                    _fsms.Add(key, fsm);
                    _tempAddList.Add(fsm);
                }
            }
            return fsm;
        }

        public void DestroyFSM<T>(string name) where T : class
        {
            TypeNamePair key = new TypeNamePair(typeof(T), name);
            if (!_fsms.ContainsKey(key))
            {
                FrameworkManager.Debugger.LogError($"FSM called {key} has not existed. ");
            }
            else
            {
                FSMBase fsm = _fsms[key];
                fsm.Shutdown();
                _fsms.Remove(key);
                _tempRemoveList.Add(fsm);
            }
        }

        public void DestroyFSM<T>(IFSM<T> _fsm) where T : class
        {
            TypeNamePair key = new TypeNamePair(typeof(T), _fsm.GetName());
            if (!_fsms.ContainsKey(key))
            {
                FrameworkManager.Debugger.LogError($"FSM called {key} has not existed. ");
            }
            else
            {
                FSMBase fsm = _fsms[key];
                fsm.Shutdown();
                _fsms.Remove(key);
                _tempRemoveList.Add(fsm);
            }
        }

        public IFSM<T> GetFSM<T>(string name) where T : class
        {
            TypeNamePair key = new TypeNamePair(typeof(T), name);
            if (!_fsms.ContainsKey(key))
            {
                FrameworkManager.Debugger.LogError($"FSM called {key} has not existed. ");
                return null;
            }
            else
            {
                return _fsms[key] as IFSM<T>;
            }
        }

        public FSMBase[] GetAllFSMs()
        {
            return _fsms.Values.ToArray();
        }

        public bool HasFSM<T>(string name) where T :class
        {
            TypeNamePair key = new TypeNamePair(typeof(T), name);
            return _fsms.ContainsKey(key);
        }

    }
}

