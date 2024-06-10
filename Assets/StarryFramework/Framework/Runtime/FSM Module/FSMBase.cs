using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace StarryFramework
{

    public class TypeNamePair
    {
        private readonly Type _type;

        private readonly string _name;

        public Type type => _type;
        public string name => _name;

        public TypeNamePair(Type type, string name)
        {
            this._type = type; 
            this._name = name;
        }

        public override string ToString()
        {
            return _type.ToString() +"-"+ _name;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is TypeNamePair) || obj == null)
            {
                return false;
            }
            else
            {
                TypeNamePair typeNamePair = (TypeNamePair)obj;
                return typeNamePair.type == _type && typeNamePair.name == _name;
            }
        }

        public override int GetHashCode() 
        { 
            return _type.GetHashCode() *31 + _name.GetHashCode();
        }
    }


    public abstract class FSMBase
    {
        private string _name;

        private bool _foldout = false;

        public string Name
        {
            get
            {
                return _name;
            }
            protected set
            {
                _name = value ?? string.Empty;
            }
        }
        public bool Foldout { get => _foldout; set => _foldout = value; }
        public string FullName => new TypeNamePair(OwnerType(), Name).ToString();
        public abstract Type OwnerType();
        public abstract bool IsRunning();
        public abstract bool IsDestroyed();
        public abstract int GetStateCount();
        public abstract IVariable[] GetAllData();
        public abstract string GetCurrentStateName();
        public abstract float GetCurrentStateTime();
        public abstract void Update();
        public abstract void Shutdown();
    }
}

