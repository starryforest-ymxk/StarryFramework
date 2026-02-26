using FMOD;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

namespace StarryFramework.Extentions
{
    internal class AudioManager : IManager, IConfigurableManager
    {
        private AudioSettings settings;
        private bool isInitialized;
        private readonly HashSet<string> loadedGlobalBanks = new HashSet<string>();
        
        private EventInstance currentBGM;

        private AudioState bgmState = AudioState.Stop;

        private Dictionary<GUID, List<EventInstance>> unnamedEventDic = new Dictionary<GUID, List<EventInstance>>();

        private Dictionary<string, EventInstance> namedEventDic = new Dictionary<string, EventInstance>();

        private System.Timers.Timer clearTimer = new System.Timers.Timer();

        void IManager.Awake()
        {

        }
        void IManager.Init()
        {
            clearTimer.Elapsed += (a, e) => FrameworkManager.PostToMainThread(() =>
            {
                if (isInitialized)
                {
                    ClearStoppedUnnamedEvent();
                }
            });
            ApplySettings();
            isInitialized = true;
        }
        void IManager.Update()
        {

        }
        void IManager.ShutDown()
        {
            isInitialized = false;
            if (loadedGlobalBanks.Count > 0)
            {
                UnloadBankData(new List<string>(loadedGlobalBanks));
                loadedGlobalBanks.Clear();
            }
            bgmState = AudioState.Stop;
            namedEventDic.Clear();
            unnamedEventDic.Clear();
            clearTimer.Close();
        }

        void IConfigurableManager.SetSettings(IManagerSettings settings)
        {
            this.settings = settings as AudioSettings;
            if (isInitialized)
            {
                ApplySettings();
            }
        }

        private void ApplySettings()
        {
            if (settings == null)
            {
                FrameworkManager.Debugger.LogError("AudioSettings is null.");
                return;
            }

            clearTimer.Interval = Mathf.Max(1f, settings.clearUnusedAudioInterval);
            SyncGlobalBanks(settings.globalBanks);
        }

        private void SyncGlobalBanks(List<string> targetBanks)
        {
            HashSet<string> nextBanks = new HashSet<string>();
            if (targetBanks != null)
            {
                foreach (var bank in targetBanks)
                {
                    if (!string.IsNullOrEmpty(bank))
                    {
                        nextBanks.Add(bank);
                    }
                }
            }

            List<string> banksToUnload = new List<string>();
            foreach (var loadedBank in loadedGlobalBanks)
            {
                if (!nextBanks.Contains(loadedBank))
                {
                    banksToUnload.Add(loadedBank);
                }
            }

            if (banksToUnload.Count > 0)
            {
                UnloadBankData(banksToUnload);
                foreach (var bank in banksToUnload)
                {
                    loadedGlobalBanks.Remove(bank);
                }
            }

            List<string> banksToLoad = new List<string>();
            foreach (var bank in nextBanks)
            {
                if (!loadedGlobalBanks.Contains(bank))
                {
                    banksToLoad.Add(bank);
                    loadedGlobalBanks.Add(bank);
                }
            }

            if (banksToLoad.Count > 0)
            {
                PreloadBankData(banksToLoad);
            }
        }

        #region eventDic 内部操作

        #region unnamed
        private EventInstance GetUnnamedEvent(GUID guid)
        {
            if(!unnamedEventDic.ContainsKey(guid))
            {
                EventInstance instance = RuntimeManager.CreateInstance(guid);
                unnamedEventDic.Add(guid, new List<EventInstance>());
                unnamedEventDic[guid].Add(instance);
                if(!clearTimer.Enabled) clearTimer.Enabled = true;
                return instance;
            }
            else
            {
                foreach(EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.getPlaybackState(out PLAYBACK_STATE state);
                    if (state == PLAYBACK_STATE.STOPPED)
                        return eventInstance;
                }
                EventInstance instance = RuntimeManager.CreateInstance(guid);
                unnamedEventDic[guid].Add(instance);
                return instance;
            }
        }

        private void StopUnnamedEvent(GUID guid, FMOD.Studio.STOP_MODE mode)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.stop(mode);
                }
            }
        }

        private void StopAndReleaseUnnamedEvent(GUID guid, FMOD.Studio.STOP_MODE mode)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.stop(mode);
                    eventInstance.release();
                }
                unnamedEventDic[guid].Clear();
            }
        }

        private void PauseUnnamedEvent(GUID guid, bool pause)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setPaused(pause);
                }
            }
        }

        private void SetUnnamedEventVolume(GUID guid, float volume)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setVolume(volume);
                }
            }
        }

        private void SetUnnamedEventProperty(GUID guid, EVENT_PROPERTY property, float value)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setProperty(property, value);
                }
            }
        }

        private void SetUnnamedEventParameter(GUID guid, string name, float value, bool ignoreSeekSpeed = false)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setParameterByName(name, value, ignoreSeekSpeed);
                }
            }
        }

        private void SetUnnamedEventParameter(GUID guid, PARAMETER_ID id, float value, bool ignoreSeekSpeed = false)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setParameterByID(id, value, ignoreSeekSpeed);
                }
            }
        }

        private void SetUnnamedEventParameterWithLabel(GUID guid, string name, string label, bool ignoreSeekSpeed = false)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setParameterByNameWithLabel(name, label, ignoreSeekSpeed);
                }
            }
        }

        private void SetUnnamedEventParameterWithLabel(GUID guid, PARAMETER_ID id, string label, bool ignoreSeekSpeed = false)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setParameterByIDWithLabel(id, label, ignoreSeekSpeed);
                }
            }
        }

        private void SetUnnamedEventParameters(GUID guid, PARAMETER_ID[] ids, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            if (!unnamedEventDic.ContainsKey(guid))
            {
                FrameworkManager.Debugger.LogWarning($"Audio event [{guid}] not found.");
                return;
            }
            else
            {
                foreach (EventInstance eventInstance in unnamedEventDic[guid])
                {
                    eventInstance.setParametersByIDs(ids, values, count, ignoreSeekSpeed);
                }
            }
        }

        private void ClearStoppedUnnamedEvent()
        {
            foreach (var list in unnamedEventDic.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    EventInstance eventInstance = list[i];
                    eventInstance.getPlaybackState(out PLAYBACK_STATE state);
                    if (state == PLAYBACK_STATE.STOPPED)
                    {
                        eventInstance.release();
                        list.Remove(eventInstance);
                        i--;
                    }
                }
            }
        }

        private void StopAndReleaseAllUnnamedEvent(FMOD.Studio.STOP_MODE mode)
        {
            foreach (var list in unnamedEventDic.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    EventInstance eventInstance = list[i];
                    eventInstance.getPlaybackState(out PLAYBACK_STATE state);
                    if(state != PLAYBACK_STATE.STOPPED)
                        eventInstance.stop(mode);
                    eventInstance.release();
                    list.Remove(eventInstance);
                    i--;
                }
            }
            unnamedEventDic.Clear();
        }

        #endregion

        #region named

        private bool HasNamedEventInstance(string tag)
        {
            return namedEventDic.ContainsKey(tag);
        }
        private EventInstance GetNamedEventInstance(string tag)
        {
            if(!namedEventDic.ContainsKey(tag))
            {
                FrameworkManager.Debugger.LogError($"Audio event with tag [{tag}] not found.");
                return new EventInstance();
            }
            else
            {
                return namedEventDic[tag];
;           }
        }
        private void AddNamedEventInstance(string tag, EventInstance eventInstance)
        {
            if(namedEventDic.ContainsKey(tag))
            {
                FrameworkManager.Debugger.LogError($"Audio event with tag [{tag}] has already existed.");
                return;
            }
            else
            {
                namedEventDic.Add(tag, eventInstance);
            }
        }
        private void RemoveNamedEventInstance(string tag)
        {
            if (!namedEventDic.ContainsKey(tag))
            {
                FrameworkManager.Debugger.LogError($"Audio event with tag [{tag}] doesn't exist.");
                return;
            }
            else
            {
                namedEventDic[tag].release();
                namedEventDic.Remove(tag);
            }
        }
        private void ClearStoppedNamedEventInstance()
        {
            namedEventDic = Utilities.DictionaryFilter(namedEventDic, 
                (a, b) => { b.getPlaybackState(out PLAYBACK_STATE state); return state != PLAYBACK_STATE.STOPPED; },
                (a,b)=>b.release());
        }
        private void StopAndReleaseAllNamedEventInstance(FMOD.Studio.STOP_MODE mode)
        {
            foreach(var a in namedEventDic.Values)
            {
                a.getPlaybackState(out PLAYBACK_STATE state);
                if(state != PLAYBACK_STATE.STOPPED)
                {
                    a.stop(mode);
                }
                a.release();
            }
            namedEventDic.Clear();
        }


        #endregion

        #endregion

        #region AudioData

        internal void PreloadData(List<EventReference> events)
        {
            foreach(var e in events)
            {
                RuntimeManager.GetEventDescription(e).loadSampleData();
            }
        }
        internal void UnloadData(List<EventReference> events)
        {
            foreach (var e in events)
            {
                RuntimeManager.GetEventDescription(e).unloadSampleData();
            }
        }
        internal void PreloadBankData(List<string> banks)
        {
            foreach (var bank in banks)
            {
                RuntimeManager.StudioSystem.getBank($"bank:/{bank}", out Bank _bank);
                _bank.loadSampleData();
            }
        }
        internal void UnloadBankData(List<string> banks)
        {
            foreach (var bank in banks)
            {
                RuntimeManager.StudioSystem.getBank($"bank:/{bank}", out Bank _bank);
                _bank.unloadSampleData();
            }
        }


        #endregion

        #region PlayOneShot

        internal void PlayOneShot(EventReference reference , Vector3 pos = default)
        {
            RuntimeManager.PlayOneShot(reference, default);
        }
        internal void PlayOneShot(string path, Vector3 pos = default)
        {
            RuntimeManager.PlayOneShot(path, default);
        }
        internal void PlayOneShotAttached(EventReference reference , GameObject gameObject)
        {
            RuntimeManager.PlayOneShotAttached(reference, gameObject);
        }
        internal void PlayOneShotAttached(string path, GameObject gameObject)
        {
            RuntimeManager.PlayOneShotAttached(path, gameObject);
        }

        #endregion

        #region PlayAudio

        #region untagged

        internal void PlayUntaggedAudio(GUID guid, float volume = 1f)
        {
            EventInstance instance = GetUnnamedEvent(guid);
            instance.setVolume(volume);
            instance.start();
        }
        internal void PlayUntaggedAudio(GUID guid, Transform transform, float volume = 1f)
        {
            EventInstance instance = GetUnnamedEvent(guid);
            RuntimeManager.AttachInstanceToGameObject(instance, transform);
            instance.setVolume(volume);
            instance.start();
        }
        internal void PlayUntaggedAudio(GUID guid, Transform transform, Rigidbody body, float volume = 1f)
        {
            EventInstance instance = GetUnnamedEvent(guid);
            RuntimeManager.AttachInstanceToGameObject(instance, transform, body);
            instance.setVolume(volume);
            instance.start();
        }
        internal void PlayUntaggedAudio(GUID guid, Transform transform, Rigidbody2D body2d, float volume = 1f)
        {
            EventInstance instance = GetUnnamedEvent(guid);
            RuntimeManager.AttachInstanceToGameObject(instance, transform, body2d);
            instance.setVolume(volume);
            instance.start();
        }
        internal void StopUntaggedAudio(GUID guid, FMOD.Studio.STOP_MODE mode)
        {
            StopUnnamedEvent(guid, mode);
        }
        internal void StopAndReleaseUntaggedAudio(GUID guid, FMOD.Studio.STOP_MODE mode)
        {
            StopAndReleaseUnnamedEvent(guid, mode);
        }
        internal void SetUntaggedAudioPaused(GUID guid, bool pause)
        {
            PauseUnnamedEvent(guid, pause);
        }
        internal void SetUntaggedAudioVolume(GUID guid, float volume)
        {
            SetUnnamedEventVolume(guid, volume);
        }
        internal void ClearStoppedUntaggedAudio()
        {
            ClearStoppedUnnamedEvent();
        }
        internal void StopAndReleaseAllUntaggedAudio(FMOD.Studio.STOP_MODE mode)
        {
            StopAndReleaseAllUnnamedEvent(mode);
        }
        internal void SetUntaggedAudioProperty(GUID guid, EVENT_PROPERTY property, float value)
        {
            SetUnnamedEventProperty(guid, property, value); 
        }
        internal void SetUntaggedAudioParameter(GUID guid, string name, float value, bool ignoreSeekSpeed = false)
        {
            SetUnnamedEventParameter(guid, name, value, ignoreSeekSpeed);   
        }
        internal void SetUntaggedAudioParameter(GUID guid, PARAMETER_ID id, float value, bool ignoreSeekSpeed = false)
        {
            SetUnnamedEventParameter(guid, id, value, ignoreSeekSpeed);
        }
        internal void SetUntaggedAudioParameterWithLabel(GUID guid, string name, string label, bool ignoreSeekSpeed = false)
        {
            SetUnnamedEventParameterWithLabel(guid, name, label, ignoreSeekSpeed);  
        }
        internal void SetUntaggedAudioParameterWithLabel(GUID guid, PARAMETER_ID id, string label, bool ignoreSeekSpeed = false)
        {
            SetUnnamedEventParameterWithLabel(guid, id, label, ignoreSeekSpeed);
        }
        internal void SetUntaggedAudioParameters(GUID guid, PARAMETER_ID[] ids, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            SetUnnamedEventParameters(guid, ids, values, count, ignoreSeekSpeed); 
        }

        #endregion

        #region tagged

        internal void CreateAudio(GUID guid, string tag, bool play = true)
        {
            EventInstance instance = RuntimeManager.CreateInstance(guid);
            AddNamedEventInstance(tag, instance);
            if(play)
                instance.start();
        }
        internal void PlayTaggedAudio(string tag)
        {
            GetNamedEventInstance(tag).start();
        }
        internal void StopTaggedAudio(string tag, FMOD.Studio.STOP_MODE mode)
        {
            GetNamedEventInstance(tag).stop(mode);
        }
        internal void ReleaseTaggedAudio(string tag)
        {
            RemoveNamedEventInstance(tag);
        }
        internal void StopAndReleaseTaggedAudio(string tag, FMOD.Studio.STOP_MODE mode)
        {
            EventInstance instance = GetNamedEventInstance(tag);
            instance.stop(mode);
            RemoveNamedEventInstance(tag);
        }
        internal void StopAndReleaseAllTaggedAudio(FMOD.Studio.STOP_MODE mode)
        {
            StopAndReleaseAllNamedEventInstance(mode);
        }
        internal void AttachedTaggedAudio(string tag, Transform transform)
        {
            RuntimeManager.AttachInstanceToGameObject(GetNamedEventInstance(tag), transform);
        }
        internal void AttachedTaggedAudio(string tag, Transform transform, Rigidbody body)
        {
            RuntimeManager.AttachInstanceToGameObject(GetNamedEventInstance(tag), transform, body);
        }
        internal void AttachTaggedAudio(string tag, Transform transform, Rigidbody2D body2d)
        {
            RuntimeManager.AttachInstanceToGameObject(GetNamedEventInstance(tag), transform, body2d);
        }
        internal void DetachTaggedAudio(string tag)
        {
            RuntimeManager.DetachInstanceFromGameObject(GetNamedEventInstance(tag));
        }
        internal void SetTaggedAudioPaused(string tag,bool pause)
        {
            GetNamedEventInstance(tag).setPaused(pause);
        }
        internal bool GetTaggedAudioPaused(string tag)
        {
            GetNamedEventInstance(tag).getPaused(out bool pause);
            return pause;
        }
        internal PLAYBACK_STATE GetTaggedAudioStage(string tag)
        {
            GetNamedEventInstance(tag).getPlaybackState(out PLAYBACK_STATE state);
            return state;
        }
        internal void SetTaggedAudioVolume(string tag, float value)
        {
            GetNamedEventInstance(tag).setVolume(value);
        }
        internal float GetTaggedAudioVolume(string tag)
        {
            GetNamedEventInstance(tag).getVolume(out float value);
            return value;
        }
        internal void SetTaggedAudioProperty(string tag, EVENT_PROPERTY property, float value)
        {
            GetNamedEventInstance(tag).setProperty(property, value);
        }
        internal float GetTaggedAudioProperty(string tag, EVENT_PROPERTY property)
        {
            GetNamedEventInstance(tag).getProperty(property, out float value);
            return value;
        }
        internal void SetTaggedAudioParameter(string tag, PARAMETER_ID ID, float value, bool ignoreSeekSpeed = false)
        {
            GetNamedEventInstance(tag).setParameterByID(ID, value, ignoreSeekSpeed);
        }
        internal void SetTaggedAudioParameter(string tag, string name, float value, bool ignoreSeekSpeed = false)
        {
            GetNamedEventInstance(tag).setParameterByName(name, value, ignoreSeekSpeed);
        }
        internal void SetTaggedAudioParameters(string tag, PARAMETER_ID[] IDs, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            GetNamedEventInstance(tag).setParametersByIDs(IDs, values, count, ignoreSeekSpeed);
        }
        internal void SetTaggedAudioParameterWithLabel(string tag, PARAMETER_ID ID, string valueLable, bool ignoreSeekSpeed = false)
        {
            GetNamedEventInstance(tag).setParameterByIDWithLabel(ID, valueLable, ignoreSeekSpeed);
        }
        internal void SetTaggedAudioParameterWithLabel(string tag, string name, string valueLable, bool ignoreSeekSpeed = false)
        {
            GetNamedEventInstance(tag).setParameterByNameWithLabel(name, valueLable, ignoreSeekSpeed);
        }
        internal float GetTaggedAudioParameter(string tag, string name)
        {
            GetNamedEventInstance(tag).getParameterByName(name, out float value);
            return value;
        }
        internal float GetTaggedAudioParameter(string tag, PARAMETER_ID ID)
        {
            GetNamedEventInstance(tag).getParameterByID(ID, out float value);
            return value;
        }
        internal void ReleaseAllStoppedTaggedAudios()
        {
            ClearStoppedNamedEventInstance();
        }

        #endregion

        #endregion

        #region BGM

        internal void PlayBGM(EventReference _event)
        {
            if(bgmState != AudioState.Playing)
            {
                currentBGM = RuntimeManager.CreateInstance(_event);
                currentBGM.start();
                bgmState = AudioState.Playing;
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM is playing.");
            }
        }
        internal void StopBGM(FMOD.Studio.STOP_MODE mode)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.stop(mode);
                currentBGM.release();
                bgmState = AudioState.Stop;
            }

        }
        internal void ChangeBGM(EventReference _event, FMOD.Studio.STOP_MODE mode)
        {
            if(bgmState == AudioState.Playing)
            {
                StopBGM(mode);
            }
            PlayBGM(_event);
        }
        internal void SetBGMPause(bool value)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.setPaused(value);
                bgmState = value? AudioState.Pause:AudioState.Playing;
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
            }
        }
        internal AudioState GetBGMState()
        {
            return bgmState;
        }
        internal void SetBGMParameter(PARAMETER_ID ID, float value, bool ignoreSeekSpeed = false)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.setParameterByID(ID, value, ignoreSeekSpeed);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
            }
        }
        internal void SetBGMParameter(string name, float value, bool ignoreSeekSpeed = false)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.setParameterByName(name, value, ignoreSeekSpeed);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
            }
        }
        internal void SetBGMParameters(PARAMETER_ID[] IDs, float[] values, int count, bool ignoreSeekSpeed = false)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.setParametersByIDs(IDs, values, count,ignoreSeekSpeed);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
            }
        }
        internal void SetBGMParameterWithLabel(PARAMETER_ID ID, string valueLable, bool ignoreSeekSpeed = false)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.setParameterByIDWithLabel(ID, valueLable, ignoreSeekSpeed);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
            }
        }
        internal void SetBGMParameterWithLabel(string name, string valueLable, bool ignoreSeekSpeed = false)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.setParameterByNameWithLabel(name, valueLable, ignoreSeekSpeed);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
            }
        }
        internal float GetBGMParameter(string name)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.getParameterByName(name, out float value);
                return value;
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
                return 0.0f;
            }
        }
        internal float GetBGMParameter(PARAMETER_ID ID)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.getParameterByID(ID, out float value);
                return value;
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
                return 0.0f;
            }
        }
        internal void SetBGMProperty(EVENT_PROPERTY property, float value)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.setProperty(property, value);
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
            }
        }
        internal float GetBGMProperty(EVENT_PROPERTY property)
        {
            if (bgmState == AudioState.Playing)
            {
                currentBGM.getProperty(property, out float value);
                return value;
            }
            else
            {
                FrameworkManager.Debugger.LogWarning("BGM has stopped.");
                return 0.0f;
            }
        }
        #endregion

        #region VCA
        internal void SetVolume(string VCAPath, float value)
        {
            if(value > 1f||value < -1f)
            {
                FrameworkManager.Debugger.LogError("invalid volume value");
            }
            VCA vca = RuntimeManager.GetVCA(VCAPath);
            vca.setVolume(value);
        }
        internal float GetVolume(string VCAPath)
        {
            VCA vca = RuntimeManager.GetVCA(VCAPath);
            vca.getVolume(out float value);
            return value;
        }

        #endregion
    }
}

