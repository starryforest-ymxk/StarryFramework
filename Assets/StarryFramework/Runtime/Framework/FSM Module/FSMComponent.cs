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
        /// 获取有限状态机数量
        /// </summary>
        /// <returns></returns>
        public int GetFSMCount() => Manager.FSMCount;

        /// <summary>
        /// 创建有限状态机，不能有拥有者类型和名称完全相同的状态机
        /// </summary>
        /// <typeparam Name="T">拥有者类型</typeparam>
        /// <param Name="name">状态机名称</param>
        /// <param Name="owner">拥有者</param>
        /// <param Name="states">状态列表</param>
        /// <returns>状态机</returns>
        public IFSM<T> CreateFSM<T>(string name, T owner, List<FSMState<T>> states) where T : class
        {
            return Manager.CreateFSM<T>(name, owner, states);
        }

        /// <summary>
        /// 创建有限状态机，不能有拥有者类型和名称完全相同的状态机
        /// </summary>
        /// <typeparam Name="T">拥有者类型</typeparam>
        /// <param Name="name">状态机名称</param>
        /// <param Name="owner">拥有者</param>
        /// <param Name="states">状态数组</param>
        /// <returns>状态机</returns>
        public IFSM<T> CreateFSM<T>(string name, T owner, FSMState<T>[] states) where T : class
        {
            return Manager.CreateFSM(name, owner, states);
        }

        /// <summary>
        /// 注销有限状态机
        /// </summary>
        /// <typeparam Name="T">拥有者类型</typeparam>
        /// <param Name="name">状态机名称</param>
        public void DestroyFSM<T>(string name) where T : class
        {
            Manager.DestroyFSM<T>(name);
        }

        /// <summary>
        /// 注销有限状态机
        /// </summary>
        /// <typeparam Name="T">拥有者类型</typeparam>
        /// <param Name="_fsm">状态机</param>
        public void DestroyFSM<T>(IFSM<T> _fsm) where T : class
        {
            Manager.DestroyFSM<T>(_fsm);
        }

        /// <summary>
        /// 查询是否拥有某状态机
        /// </summary>
        /// <typeparam Name="T">拥有者类型</typeparam>
        /// <param Name="name">状态机名称</param>
        /// <returns></returns>
        public bool HasFSM<T>(string name) where T : class
        {
            return Manager.HasFSM<T>(name);
        }

        /// <summary>
        /// 获得某状态机
        /// </summary>
        /// <typeparam Name="T">拥有者类型</typeparam>
        /// <param Name="name">状态机名称</param>
        /// <returns></returns>
        public IFSM<T> GetFSM<T>(string name) where T : class
        {
            return Manager.GetFSM<T>(name);
        }

        /// <summary>
        /// 获取所有状态机
        /// </summary>
        /// <returns></returns>
        public FSMBase[] GetAllFSMs()
        {
            return Manager.GetAllFSMs();
        }


    }
}

