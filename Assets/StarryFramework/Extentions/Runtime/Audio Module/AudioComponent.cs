using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GUID = FMOD.GUID;


namespace StarryFramework
{
    public class AudioComponent : BaseComponent
    {
        private AudioManager _manager = null;
        private AudioManager manager
        {
            get
            {
                if (_manager == null)
                {
                    _manager = FrameworkManager.GetManager<AudioManager>();
                }
                return _manager;
            }
        }
                
        [SerializeField] private AudioSettings settings = new AudioSettings();
        
        private List<EventReference> loadedAudio;

        private string currentBGM;

        private AudioState bgmState = AudioState.Stop;

        private List<EventReference> currentBGMList;

        public string CurrentBGM => currentBGM;
        public AudioState BGMState => bgmState;
        public List<EventReference> CurrentBGMList => currentBGMList;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(EditorApplication.isPlaying && _manager != null)
                (_manager as IManager).SetSettings(settings);
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            _manager ??= FrameworkManager.GetManager<AudioManager>();
            (_manager as IManager).SetSettings(settings);
        }

        private void Start()
        {
            FrameworkManager.EventManager.AddEventListener<int>(FrameworkEvent.SetNewActiveScene, OnNewActiveScene);
            FrameworkManager.EventManager.AddEventListener<int>(FrameworkEvent.SetCurrentActiveSceneNotActive, InActiveCurrentScene);
        }
        internal override void Shutdown() 
        {
            FrameworkManager.EventManager.RemoveEventListener<int>(FrameworkEvent.SetNewActiveScene, OnNewActiveScene);
            FrameworkManager.EventManager.RemoveEventListener<int>(FrameworkEvent.SetCurrentActiveSceneNotActive, InActiveCurrentScene);
        }
        
        private void InActiveCurrentScene(int sceneIndex)
        {
            manager.StopBGM(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            manager.StopAndReleaseAllUntaggedAudio(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            manager.StopAndReleaseAllTaggedAudio(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            if(loadedAudio != null)
                manager.UnloadData(loadedAudio);
        }
        
        private void OnNewActiveScene(int sceneIndex)
        {
            foreach(var s in settings.sceneAudioSettings)
            {
                if(s.scene == sceneIndex)
                {
                    currentBGMList = s.BGMList;
                    if(s.autoPlayBGM && s.BGMList.Count!=0)
                    {
                        manager.PlayBGM(s.BGMList[0]);
                    }
                    loadedAudio = s.preloadedAudios;
                    manager.PreloadData(s.preloadedAudios);
                    foreach(var e in s.autoPlayAudios)
                    {
                        if(e.tag == "")
                        {
                            PlayUntaggedAudio(e.eventReference);
                        }
                        else
                        {
                            CreateAudio(e.eventReference, e.tag, true);
                        }
                    }
                }
            }
        }

        #region PlayOneShot

        /// <summary>
        /// ����һ����������Ƶ��Դ�ڴ���������ж��
        /// Ƶ��ʹ�ý��������ܿ����������ڲ���ôʹ�õ���Ƶ��Դ
        /// </summary>
        /// <param Name="reference">��Ƶ�¼�</param>
        /// <param Name="pos">����λ�ã�3D��</param>
        public void PlayOneShot(EventReference reference, Vector3 pos = default)
        {
            manager.PlayOneShot(reference, pos);
        }

        /// <summary>
        /// ����һ����������Ƶ��Դ�ڴ���������ж��
        /// Ƶ��ʹ�ý��������ܿ����������ڲ���ôʹ�õ���Ƶ��Դ
        /// </summary>
        /// <param Name="path">��Ƶ�¼�·��</param>
        /// <param Name="pos">����λ�ã�3D��</param>
        public void PlayOneShot(string path, Vector3 pos = default)
        {
            manager.PlayOneShot(path, pos);
        }

        /// <summary>
        /// ����һ������������Դ���ŵ�ĳ�������ϣ���Ƶ��Դ�ڴ���������ж��
        /// Ƶ��ʹ�ý��������ܿ����������ڲ���ôʹ�õ���Ƶ��Դ
        /// </summary>
        /// <param Name="reference">��Ƶ�¼�</param>
        /// <param Name="gameObject">������</param>
        public void PlayOneShotAttached(EventReference reference, GameObject gameObject)
        {
            manager.PlayOneShotAttached(reference, gameObject);
        }

        /// <summary>
        /// ����һ������������Դ���ŵ�ĳ�������ϣ���Ƶ��Դ�ڴ���������ж��
        /// Ƶ��ʹ�ý��������ܿ����������ڲ���ôʹ�õ���Ƶ��Դ
        /// </summary>
        /// <param Name="path">��Ƶ�¼�·��</param>
        /// <param Name="gameObject">������</param>
        public void PlayOneShotAttached(string path, GameObject gameObject)
        {
            manager.PlayOneShotAttached(path, gameObject);
        }

        #endregion

        #region VCA

        /// <summary>
        /// ����VCA��Ƶ��������С
        /// </summary>
        /// <param Name="VCAPath"></param>
        /// <param Name="value"></param>
        public void SetVolume(string VCAPath, float value)
        {
            manager.SetVolume(VCAPath, value);
        }

        /// <summary>
        /// ���VCA��Ƶ��������С
        /// </summary>
        /// <param Name="VCAPath"></param>
        /// <returns></returns>
        public float GetVolume(string VCAPath)
        {
            return manager.GetVolume(VCAPath);
        }

        #endregion

        #region BGM

        /// <summary>
        /// ����BGM
        /// </summary>
        /// <param Name="index">����������BGM���</param>
        public void PlayBGM(int index)
        {
            if(index>=0 && index< currentBGMList.Count)
            {
                manager.PlayBGM(currentBGMList[index]);
#if UNITY_EDITOR
                currentBGM = currentBGMList[index].Path;
#endif
                bgmState = AudioState.Playing;
            }
            else
            {
                FrameworkManager.Debugger.LogError("BGM index out of bound.");
            }
        }

        /// <summary>
        /// ֹͣBGM
        /// </summary>
        /// <param Name="mode">FMOD.Studio.STOP_MODEö�٣���������ֹͣ����</param>
        public void StopBGM(FMOD.Studio.STOP_MODE mode)
        {
            manager.StopBGM(mode);
            bgmState = AudioState.Stop;
        }

        /// <summary>
        /// �л�BGM
        /// </summary>
        /// <param Name="index">Ҫ���ŵ�BGM���</param>
        /// <param Name="mode">FMOD.Studio.STOP_MODEö�٣���������ֹͣ����</param>
        public void ChangeBGM(int index, FMOD.Studio.STOP_MODE mode)
        {
            if (index >= 0 && index < currentBGMList.Count)
            {
                manager.ChangeBGM(currentBGMList[index],mode);
                bgmState = AudioState.Playing;
#if UNITY_EDITOR
                currentBGM = currentBGMList[index].Path;
#endif
            }
            else
            {
                FrameworkManager.Debugger.LogError("BGM index out of bound.");
            }
        }

        /// <summary>
        /// ����BGM���š���ͣ״̬
        /// </summary>
        /// <param Name="value">trueΪ��ͣ��falseΪ��������</param>
        public void SetBGMPause(bool value)
        {
            manager.SetBGMPause(value);
            bgmState = value ? AudioState.Pause : AudioState.Playing;
        }

        /// <summary>
        /// ���BGM״̬
        /// </summary>
        /// <returns></returns>
        public AudioState GetBGMState()
        {
            return bgmState;
        }

        /// <summary>
        /// ����BGM�Ĳ���
        /// </summary>
        /// <param Name="ID"></param>
        /// <param Name="value"></param>
        /// <param Name="ignoreSeekSpeed"></param>
        public void SetBGMParameter(PARAMETER_ID ID, float value, bool ignoreSeekSpeed = false)
        {
            manager.SetBGMParameter(ID, value, ignoreSeekSpeed);    
        }

        /// <summary>
        /// ����BGM����
        /// </summary>
        /// <param Name="name"></param>
        /// <param Name="value"></param>
        /// <param Name="ignoreSeekSpeed"></param>
        public void SetBGMParameter(string name, float value, bool ignoreSeekSpeed = false)
        {
            manager.SetBGMParameter(name, value, ignoreSeekSpeed);
        }

        /// <summary>
        /// ����BGMһ�����ֵ
        /// </summary>
        /// <param Name="IDs"></param>
        /// <param Name="values"></param>
        /// <param Name="Count"></param>
        /// <param Name="ignoreSeekSpeed"></param>
        public void SetBGMParameters(PARAMETER_ID[] IDs, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            manager.SetBGMParameters(IDs, values, count, ignoreSeekSpeed);  
        }

        /// <summary>
        /// �ñ�ǩ����BGM����
        /// </summary>
        /// <param Name="ID"></param>
        /// <param Name="valueLable"></param>
        /// <param Name="ignoreSeekSpeed"></param>
        public void SetBGMParameterWithLabel(PARAMETER_ID ID, string valueLable, bool ignoreSeekSpeed = false)
        {
            manager.SetBGMParameterWithLabel(ID, valueLable, ignoreSeekSpeed);
        }

        /// <summary>
        /// �ñ�ǩ����BGM����
        /// </summary>
        /// <param Name="name"></param>
        /// <param Name="valueLable"></param>
        /// <param Name="ignoreSeekSpeed"></param>
        public void SetBGMParameterWithLabel(string name, string valueLable, bool ignoreSeekSpeed = false)
        {
            manager.SetBGMParameterWithLabel(name, valueLable, ignoreSeekSpeed);    
        }

        /// <summary>
        /// ���BGM����ֵ
        /// </summary>
        /// <param Name="name"></param>
        /// <returns></returns>
        public float GetBGMParameter(string name)
        {
            return manager.GetBGMParameter(name);
        }

        /// <summary>
        /// ���BGM����ֵ
        /// </summary>
        /// <param Name="ID"></param>
        /// <returns></returns>
        public float GetBGMParameter(PARAMETER_ID ID)
        {
            return manager.GetBGMParameter(ID);
        }

        /// <summary>
        /// ����BGM����
        /// </summary>
        /// <param Name="property"></param>
        /// <param Name="value"></param>
        public void SetBGMProperty(EVENT_PROPERTY property, float value)
        {
            manager.SetBGMProperty(property, value);
        }

        /// <summary>
        /// ���BGM����ֵ
        /// </summary>
        /// <param Name="property"></param>
        /// <returns></returns>
        public float GetBGMProperty(EVENT_PROPERTY property)
        {
            return manager.GetBGMProperty(property);
        }
        #endregion

        #region PlayAudio

        //����untagged����Ƶ���ö����ͳһ����
        #region untagged

        /// <summary>
        /// ����δ��ǵ���Ƶ
        /// </summary>
        public void PlayUntaggedAudio(string path, float volume = 1f)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.PlayUntaggedAudio(guid, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ
        /// </summary>
        public void PlayUntaggedAudio(EventReference eventReference, float volume = 1f)
        {
            GUID guid = eventReference.Guid;
            manager.PlayUntaggedAudio(guid, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ����������ƵԴλ��
        /// </summary>
        public void PlayUntaggedAudio(string path, Transform transform, float volume = 1f)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.PlayUntaggedAudio(guid, transform, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ����������ƵԴλ��
        /// </summary>
        public void PlayUntaggedAudio(EventReference eventReference, Transform transform, float volume = 1f)
        {
            GUID guid = eventReference.Guid;
            manager.PlayUntaggedAudio(guid, transform, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�������ŵ�����
        /// </summary>
        public void PlayUntaggedAudio(string path, Transform transform, Rigidbody body, float volume = 1f)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.PlayUntaggedAudio(guid, transform, body, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�������ŵ�����
        /// </summary>
        public void PlayUntaggedAudio(EventReference eventReference, Transform transform, Rigidbody body, float volume = 1f)
        {
            GUID guid = eventReference.Guid;
            manager.PlayUntaggedAudio(guid, transform, body, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�������ŵ�2D����
        /// </summary>
        public void PlayUntaggedAudio(string path, Transform transform, Rigidbody2D body2d, float volume = 1f)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.PlayUntaggedAudio(guid, transform, body2d, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�������ŵ�2D����
        /// </summary>
        public void PlayUntaggedAudio(EventReference eventReference, Transform transform, Rigidbody2D body2d, float volume = 1f)
        {
            GUID guid = eventReference.Guid;
            manager.PlayUntaggedAudio(guid, transform, body2d, volume);
        }

        /// <summary>
        /// ֹͣ����δ��ǵ���Ƶ
        /// </summary>
        /// <param Name="path"></param>
        /// <param Name="mode"></param>
        public void StopUntaggedAudio(string path, FMOD.Studio.STOP_MODE mode)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.StopUntaggedAudio(guid, mode);
        }
        /// <summary>
        /// ֹͣ����δ��ǵ���Ƶ
        /// </summary>
        /// <param Name="eventReference"></param>
        /// <param Name="mode"></param>
        public void StopUntaggedAudio(EventReference eventReference, FMOD.Studio.STOP_MODE mode)
        {
            GUID guid = eventReference.Guid;
            manager.StopUntaggedAudio(guid, mode);
        }
        /// <summary>
        /// ֹͣ���ͷ�δ��ǵ���Ƶ
        /// </summary>
        /// <param Name="path"></param>
        /// <param Name="mode"></param>
        public void StopAndReleaseUntaggedAudio(string path, FMOD.Studio.STOP_MODE mode)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.StopAndReleaseUntaggedAudio(guid, mode);
        }

        /// <summary>
        /// ֹͣ���ͷ�δ��ǵ���Ƶ
        /// </summary>
        /// <param Name="eventReference"></param>
        /// <param Name="mode"></param>
        public void StopAndReleaseUntaggedAudio(EventReference eventReference, FMOD.Studio.STOP_MODE mode)
        {
            GUID guid = eventReference.Guid;
            manager.StopAndReleaseUntaggedAudio(guid, mode);
        }

        /// <summary>
        /// ����δ��ǵ���Ƶ��ͣ/����״̬
        /// </summary>
        /// <param Name="path"></param>
        /// <param Name="pause">trueΪ��ͣ��falseΪ��������</param>
        public void SetUntaggedAudioPaused(string path, bool pause)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioPaused(guid, pause);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ��ͣ/����״̬
        /// </summary>
        /// <param Name="eventReference"></param>
        /// <param Name="pause">trueΪ��ͣ��falseΪ��������</param>
        public void SetUntaggedAudioPaused(EventReference eventReference, bool pause)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioPaused(guid, pause);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ����
        /// </summary>
        /// <param Name="path"></param>
        /// <param Name="volume"></param>
        public void SetUntaggedAudioVolume(string path, float volume)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioVolume(guid, volume);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ����
        /// </summary>
        /// <param Name="eventReference"></param>
        /// <param Name="volume"></param>
        public void SetUntaggedAudioVolume(EventReference eventReference, float volume)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioVolume(guid, volume);
        }

        /// <summary>
        /// �ͷ������Ѿ�ֹͣ��δ�����Ƶ
        /// </summary>
        public void ClearStoppedUntaggedAudio()
        {
            manager.ClearStoppedUntaggedAudio();
        }

        /// <summary>
        /// ֹͣ���ͷ�����δ��ǵ���Ƶ
        /// </summary>
        /// <param Name="mode"></param>
        public void StopAndReleaseAllUntaggedAudio(FMOD.Studio.STOP_MODE mode)
        {
            manager.StopAndReleaseAllUntaggedAudio(mode);
        }

        /// <summary>
        /// ����δ��ǵ���Ƶ������
        /// </summary>
        public void SetUntaggedAudioProperty(string path, EVENT_PROPERTY property, float value)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioProperty(guid, property, value);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ������
        /// </summary>
        public void SetUntaggedAudioProperty(EventReference eventReference, EVENT_PROPERTY property, float value)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioProperty(guid, property, value);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameter(string path, string name, float value, bool ignoreSeekSpeed = false)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioParameter(guid, name, value, ignoreSeekSpeed);  
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameter(EventReference eventReference, string name, float value, bool ignoreSeekSpeed = false)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioParameter(guid, name, value, ignoreSeekSpeed);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameter(string path, PARAMETER_ID id, float value, bool ignoreSeekSpeed = false)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioParameter(guid, id, value, ignoreSeekSpeed);    
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameter(EventReference eventReference, PARAMETER_ID id, float value, bool ignoreSeekSpeed = false)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioParameter(guid, id, value, ignoreSeekSpeed);
        }
        /// <summary>
        /// �ñ�ǩ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameterWithLabel(string path, string name, string label, bool ignoreSeekSpeed = false)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioParameterWithLabel(guid, name, label, ignoreSeekSpeed);
        }
        /// <summary>
        /// �ñ�ǩ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameterWithLabel(EventReference eventReference, string name, string label, bool ignoreSeekSpeed = false)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioParameterWithLabel(guid, name, label, ignoreSeekSpeed);
        }
        /// <summary>
        /// �ñ�ǩ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameterWithLabel(string path, PARAMETER_ID id, string label, bool ignoreSeekSpeed = false)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioParameterWithLabel(guid, id, label, ignoreSeekSpeed);
        }
        /// <summary>
        /// �ñ�ǩ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameterWithLabel(EventReference eventReference, PARAMETER_ID id, string label, bool ignoreSeekSpeed = false)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioParameterWithLabel(guid, id, label, ignoreSeekSpeed);
        }
        /// <summary>
        /// ����δ��ǵ���Ƶ�Ĳ���
        /// </summary>
        public void SetUntaggedAudioParameters(string path, PARAMETER_ID[] ids, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.SetUntaggedAudioParameters(guid, ids, values, count, ignoreSeekSpeed);
        }
        /// <summary>
        /// �ñ�ǩ����δ��ǵ���Ƶ��һ�����ֵ
        /// </summary>
        public void SetUntaggedAudioParameters(EventReference eventReference, PARAMETER_ID[] ids, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            GUID guid = eventReference.Guid;
            manager.SetUntaggedAudioParameters(guid, ids, values, count, ignoreSeekSpeed);
        }

        #endregion

        //����tagged����Ƶ�����ö���ع���
        #region tagged

        /// <summary>
        /// ������Ƶ
        /// </summary>
        /// <param Name="path">��Ƶ�¼�·��</param>
        /// <param Name="tag">��Ƶ���</param>
        /// <param Name="play">�Ƿ����̲���</param>
        public void CreateAudio(string path, string tag, bool play = true)
        {
            GUID guid = RuntimeManager.PathToGUID(path);
            manager.CreateAudio(guid, tag, play);
        }

        /// <summary>
        /// ������Ƶ
        /// </summary>
        /// <param Name="eventReference">��Ƶ�¼�</param>
        /// <param Name="tag">��Ƶ���</param>
        /// <param Name="play">�Ƿ����̲���</param>
        public void CreateAudio(EventReference eventReference, string tag, bool play = true)
        {
            GUID guid = eventReference.Guid;
            manager.CreateAudio(guid, tag, play);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ
        /// </summary>
        public void PlayTaggedAudio(string tag)
        {
            manager.PlayTaggedAudio(tag);
        }

        /// <summary>
        /// ֹͣ�ѱ�ǵ���Ƶ
        /// </summary>
        public void StopTaggedAudio(string tag, FMOD.Studio.STOP_MODE mode)
        {
            manager.StopTaggedAudio(tag, mode);
        }

        /// <summary>
        /// �ͷ��ѱ�ǵ���Ƶ
        /// </summary>
        public void ReleaseTaggedAudio(string tag)
        {
            manager.ReleaseTaggedAudio(tag);
        }

        /// <summary>
        /// ֹͣ���ͷ��ѱ�ǵ���Ƶ
        /// </summary>
        public void StopAndReleaseTaggedAudio(string tag, FMOD.Studio.STOP_MODE mode)
        {
            manager.StopAndReleaseTaggedAudio(tag, mode);
        }

        /// <summary>
        /// ֹͣ���ͷ������ѱ�ǵ���Ƶ
        /// </summary>
        public void StopAndReleaseAllTaggedAudio(FMOD.Studio.STOP_MODE mode)
        {
            manager.StopAndReleaseAllTaggedAudio(mode);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ��λ��
        /// </summary>
        /// <param Name="tag"></param>
        /// <param Name="transform"></param>
        public void AttachedTaggedAudio(string tag, Transform transform)
        {
            manager.AttachedTaggedAudio(tag, transform);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ��������
        /// </summary>
        /// <param Name="tag"></param>
        /// <param Name="transform"></param>
        /// <param Name="body"></param>
        public void AttachedTaggedAudio(string tag, Transform transform, Rigidbody body)
        {
            manager.AttachedTaggedAudio(tag, transform, body);    
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ��3D������
        /// </summary>
        /// <param Name="tag"></param>
        /// <param Name="transform"></param>
        /// <param Name="body2d"></param>
        public void AttachTaggedAudio(string tag, Transform transform, Rigidbody2D body2d)
        {
            manager.AttachTaggedAudio(tag, transform, body2d);
        }

        /// <summary>
        /// ȡ����Ƶ�ĸ������弰λ��
        /// </summary>
        /// <param Name="tag"></param>
        public void DetachTaggedAudio(string tag)
        {
            manager.DetachTaggedAudio(tag);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ��ͣ/����״̬
        /// </summary>
        /// <param Name="tag"></param>
        /// <param Name="pause">trueΪ��ͣ��falseΪ��������</param>
        public void SetTaggedAudioPaused(string tag, bool pause)
        {
            manager.SetTaggedAudioPaused(tag, pause);
        }

        /// <summary>
        /// ����ѱ�ǵ���Ƶ��ͣ/����״̬
        /// </summary>
        /// <param Name="tag"></param>
        /// <returns></returns>
        public bool GetTaggedAudioPaused(string tag)
        {
            return manager.GetTaggedAudioPaused(tag);
        }

        /// <summary>
        /// ����ѱ�ǵ���Ƶ״̬
        /// </summary>
        /// <param Name="tag"></param>
        /// <returns></returns>
        public PLAYBACK_STATE GetTaggedAudioStage(string tag)
        {
            return manager.GetTaggedAudioStage(tag);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ����
        /// </summary>
        /// <param Name="tag"></param>
        /// <param Name="value"></param>
        public void SetTaggedAudioVolume(string tag, float value)
        {
            manager.SetTaggedAudioVolume(tag, value);
        }

        /// <summary>
        /// ����ѱ�ǵ���Ƶ����
        /// </summary>
        /// <param Name="tag"></param>
        /// <returns></returns>
        public float GetTaggedAudioVolume(string tag)
        {
            return manager.GetTaggedAudioVolume(tag);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ����
        /// </summary>
        public void SetTaggedAudioProperty(string tag, EVENT_PROPERTY property, float value)
        {
            manager.SetTaggedAudioProperty(tag, property, value);
        }

        /// <summary>
        /// ����ѱ�ǵ���Ƶ����
        /// </summary>
        public float GetTaggedAudioProperty(string tag, EVENT_PROPERTY property)
        {
            return manager.GetTaggedAudioProperty(tag, property);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ����
        /// </summary>
        public void SetTaggedAudioParameter(string tag, PARAMETER_ID ID, float value, bool ignoreSeekSpeed = false)
        {
            manager.SetTaggedAudioParameter(tag, ID, value, ignoreSeekSpeed);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ����
        /// </summary>
        public void SetTaggedAudioParameter(string tag, string name, float value, bool ignoreSeekSpeed = false)
        {
            manager.SetTaggedAudioParameter(tag, name, value, ignoreSeekSpeed);
        }

        /// <summary>
        /// �����ѱ�ǵ���Ƶ��һ�����
        /// </summary>
        public void SetTaggedAudioParameters(string tag, PARAMETER_ID[] IDs, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            manager.SetTaggedAudioParameters(tag, IDs, values, count, ignoreSeekSpeed);
        }

        /// <summary>
        /// �ñ�ǩ�����ѱ�ǵ���Ƶ����
        /// </summary>
        public void SetTaggedAudioParameterWithLabel(string tag, PARAMETER_ID ID, string valueLable, bool ignoreSeekSpeed = false)
        {
            manager.SetTaggedAudioParameterWithLabel(tag, ID, valueLable, ignoreSeekSpeed);
        }

        /// <summary>
        /// �ñ�ǩ�����ѱ�ǵ���Ƶ����
        /// </summary>
        public void SetTaggedAudioParameterWithLabel(string tag, string name, string valueLable, bool ignoreSeekSpeed = false)
        {
            manager.SetTaggedAudioParameterWithLabel(tag, name, valueLable, ignoreSeekSpeed);
        }

        /// <summary>
        /// ����ѱ�ǵ���Ƶ����
        /// </summary>
        public float GetTaggedAudioParameter(string tag, string name)
        {
            return manager.GetTaggedAudioParameter(tag, name);
        }

        /// <summary>
        /// ����ѱ�ǵ���Ƶ����
        /// </summary>
        public float GetTaggedAudioParameter(string tag, PARAMETER_ID ID)
        {
            return manager.GetTaggedAudioParameter(tag, ID);
        }

        /// <summary>
        /// �ͷ������Ѿ�ֹͣ���ѱ����Ƶ��Դ
        /// </summary>
        public void ReleaseAllStoppedTaggedAudios()
        {
            manager.ReleaseAllStoppedTaggedAudios();
        }

        #endregion

        #endregion


    }

}


