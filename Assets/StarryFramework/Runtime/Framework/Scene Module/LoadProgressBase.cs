using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework
{
    public abstract class LoadProgressBase: MonoBehaviour
    {
        /// <summary>
        /// �����������ٶȣ�ÿ֡������ӵ�ֵ��
        /// </summary>
        [Range(0, 0.1f)]
        public float speed = 0.05f;

        /// <summary>
        /// ������ֵ������UI�����
        /// </summary>
        /// <param Name="value"></param>
        public abstract void SetProgressValue(float value);

        /// <summary>
        /// ���ڼ�����ɺ�Ļص��������ڼ��س�����һ�첽�¼��У���Ҫ�ڴ˴����ú��ʵ�����������AllowSceneActivate()
        /// </summary>
        /// <param Name="asyncOperation"></param>
        public abstract void BeforeSetActive(AsyncOperation asyncOperation);

        /// <summary>
        /// ���������غ󼤻�
        /// </summary>
        /// <param Name="asyncOperation"></param>
        protected void AllowSceneActivate(AsyncOperation asyncOperation)
        {
            asyncOperation.allowSceneActivation = true;
        }

    }
}

