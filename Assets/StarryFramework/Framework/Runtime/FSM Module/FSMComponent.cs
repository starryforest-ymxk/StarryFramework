using System.Collections.Generic;

namespace StarryFramework
{
    public class FSMComponent : BaseComponent
    {
        private FSMManager _manager;
        private FSMManager Manager => _manager ??= FrameworkManager.GetManager<FSMManager>();

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<FSMManager>();
        }

        internal override void Shutdown()
        {
            
        }



        /// <summary>
        /// ��ȡ����״̬������
        /// </summary>
        /// <returns></returns>
        public int GetFSMCount() => Manager.FSMCount;

        /// <summary>
        /// ��������״̬����������ӵ�������ͺ�������ȫ��ͬ��״̬��
        /// </summary>
        /// <typeparam Name="T">ӵ��������</typeparam>
        /// <param Name="name">״̬������</param>
        /// <param Name="owner">ӵ����</param>
        /// <param Name="states">״̬�б�</param>
        /// <returns>״̬��</returns>
        public IFSM<T> CreateFSM<T>(string name, T owner, List<FSMState<T>> states) where T : class
        {
            return Manager.CreateFSM<T>(name, owner, states);
        }

        /// <summary>
        /// ��������״̬����������ӵ�������ͺ�������ȫ��ͬ��״̬��
        /// </summary>
        /// <typeparam Name="T">ӵ��������</typeparam>
        /// <param Name="name">״̬������</param>
        /// <param Name="owner">ӵ����</param>
        /// <param Name="states">״̬����</param>
        /// <returns>״̬��</returns>
        public IFSM<T> CreateFSM<T>(string name, T owner, FSMState<T>[] states) where T : class
        {
            return Manager.CreateFSM(name, owner, states);
        }

        /// <summary>
        /// ע������״̬��
        /// </summary>
        /// <typeparam Name="T">ӵ��������</typeparam>
        /// <param Name="name">״̬������</param>
        public void DestroyFSM<T>(string name) where T : class
        {
            Manager.DestroyFSM<T>(name);
        }

        /// <summary>
        /// ע������״̬��
        /// </summary>
        /// <typeparam Name="T">ӵ��������</typeparam>
        /// <param Name="_fsm">״̬��</param>
        public void DestroyFSM<T>(IFSM<T> _fsm) where T : class
        {
            Manager.DestroyFSM<T>(_fsm);
        }

        /// <summary>
        /// ��ѯ�Ƿ�ӵ��ĳ״̬��
        /// </summary>
        /// <typeparam Name="T">ӵ��������</typeparam>
        /// <param Name="name">״̬������</param>
        /// <returns></returns>
        public bool HasFSM<T>(string name) where T : class
        {
            return Manager.HasFSM<T>(name);
        }

        /// <summary>
        /// ���ĳ״̬��
        /// </summary>
        /// <typeparam Name="T">ӵ��������</typeparam>
        /// <param Name="name">״̬������</param>
        /// <returns></returns>
        public IFSM<T> GetFSM<T>(string name) where T : class
        {
            return Manager.GetFSM<T>(name);
        }

        /// <summary>
        /// ��ȡ����״̬��
        /// </summary>
        /// <returns></returns>
        public FSMBase[] GetAllFSMs()
        {
            return Manager.GetAllFSMs();
        }


    }
}

