using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public abstract class LoadProgressBase: MonoBehaviour
    {
        /// <summary>
        /// 进度条最快的速度（每帧最快增加的值）
        /// </summary>
        [Range(0, 0.1f)]
        public float speed = 0.05f;

        /// <summary>
        /// 将进度值设置在UI组件上
        /// </summary>
        /// <param Name="value"></param>
        public abstract void SetProgressValue(float value);

        /// <summary>
        /// 对于加载完成后的回调操作，在加载场景这一异步事件中，需要在此处设置合适的条件并调用AllowSceneActivate()
        /// </summary>
        /// <param Name="asyncOperation"></param>
        public abstract void BeforeSetActive(AsyncOperation asyncOperation);

        /// <summary>
        /// 允许场景加载后激活
        /// </summary>
        /// <param Name="asyncOperation"></param>
        protected void AllowSceneActivate(AsyncOperation asyncOperation)
        {
            asyncOperation.allowSceneActivation = true;
        }

    }
}

