namespace StarryFramework
{

    public class ObjectPoolProperty 
    {
        private string _name;

        private string _fullName;

        private int _count;

        private float _autoReleaseInterval;

        private float _expireTime;

        private float _lastReleaseTime;

        private bool _locked = false;


        public string Name { get => _name; set => _name = value; }

        public string FullName { get => _fullName; set => _fullName = value; }

        public int Count { get => _count; set => _count = value; }

        public float AutoReleaseInterval { get => _autoReleaseInterval; set => _autoReleaseInterval = value; }

        public float ExpireTime { get => _expireTime; set => _expireTime = value; }

        public float LastReleaseTime { get => _lastReleaseTime; set => _lastReleaseTime = value; }

        public bool Locked { get => _locked; set => _locked = value; }

#if UNITY_EDITOR
        public bool foldout = false;
#endif
    }
}

