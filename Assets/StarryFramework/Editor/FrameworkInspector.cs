using UnityEditor;

namespace StarryFramework.Editor
{
    public abstract class FrameworkInspector : UnityEditor.Editor
    {
        private bool m_IsCompiling = false;

        /// <summary>
        /// �����¼���
        /// </summary>
        public override void OnInspectorGUI()
        {   
            if (m_IsCompiling && !EditorApplication.isCompiling)
            {
                m_IsCompiling = false;
                OnCompileComplete();
            }
            else if (!m_IsCompiling && EditorApplication.isCompiling)
            {
                m_IsCompiling = true;
                OnCompileStart();
            }
        }

        /// <summary>
        /// ���뿪ʼ�¼���
        /// </summary>
        protected virtual void OnCompileStart()
        {
        }

        /// <summary>
        /// ��������¼���
        /// </summary>
        protected virtual void OnCompileComplete()
        {
        }
    }
}

