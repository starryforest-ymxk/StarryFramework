using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public interface UIFormLogic
    {
        
        #region 生命周期

        /// <summary>
        /// 资源加载时被调用
        /// </summary>
        public void OnInit();

        /// <summary>
        /// 资源释放时被调用
        /// </summary>
        public void OnRelease();

        /// <summary>
        /// 界面打开时被调用
        /// </summary>
        public void OnOpen();

        /// <summary>
        /// 界面关闭时被调用
        /// </summary>
        /// <param name="isShutdown">是否为框架关闭的时候被调用</param>
        public void OnClose(bool isShutdown);

        /// <summary>
        /// 界面被覆盖时，被暂停时，被关闭时被调用，后两种情况被调用时先于OnPause
        /// </summary>
        public void OnCover();

        /// <summary>
        /// 界面从覆盖状态被揭露显示时，界面取消暂停状态时被调用，后一种情况被调用时晚于OnResume
        /// </summary>
        public void OnReveal();

        /// <summary>
        /// 界面被暂停时，被关闭时被调用，晚于OnCover
        /// </summary>
        public void OnPause();

        /// <summary>
        /// 界面取消暂停状态时被调用，先于OnReveal
        /// </summary>
        public void OnResume();

        /// <summary>
        /// 如果界面打开且未暂停，则每帧被调用
        /// </summary>
        public void OnUpdate();
    
        /// <summary>
        /// 当界面在界面组中的堆叠深度改变时被调用
        /// </summary>
        /// <param name="formCountInUIGroup">界面组中UI界面数量</param>
        /// <param name="depthInUIGroup">改变后界面在界面组中的深度值，0代表界面位于界面组最底层</param>
        public void OnDepthChanged(int formCountInUIGroup, int depthInUIGroup);
    
        /// <summary>
        /// 当界面被重新聚焦的时候被调用，触发时间晚于上面所有周期函数
        /// </summary>
        public void OnRefocus();

        #endregion
    }
}

