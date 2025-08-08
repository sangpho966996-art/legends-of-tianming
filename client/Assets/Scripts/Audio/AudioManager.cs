using UnityEngine;
using System.Collections.Generic;

namespace LegendsOfTianming.Core
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource ambientSource;
        
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        [Range(0f, 1f)]
        public float sfxVolume = 1f;
        [Range(0f, 1f)]
        public float ambientVolume = 0.5f;
        
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioSource> loopingSources = new Dictionary<string, AudioSource>();

        private void Start()
        {
            InitializeAudioSources();
            LoadAudioClips();
        }

        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (ambientSource == null)
            {
                GameObject ambientObj = new GameObject("AmbientSource");
                ambientObj.transform.SetParent(transform);
                ambientSource = ambientObj.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
            }

            UpdateVolumes();
        }

        private void LoadAudioClips()
        {
            Debug.Log("🎵 Audio clips loaded");
        }

        public void PlayMusic(string clipName, bool loop = true)
        {
            if (audioClips.ContainsKey(clipName))
            {
                musicSource.clip = audioClips[clipName];
                musicSource.loop = loop;
                musicSource.Play();
                Debug.Log($"🎵 Playing music: {clipName}");
            }
            else
            {
                Debug.LogWarning($"Music clip not found: {clipName}");
            }
        }

        public void PlaySFX(string clipName, float volumeScale = 1f)
        {
            if (audioClips.ContainsKey(clipName))
            {
                sfxSource.PlayOneShot(audioClips[clipName], volumeScale);
                Debug.Log($"🔊 Playing SFX: {clipName}");
            }
            else
            {
                Debug.LogWarning($"SFX clip not found: {clipName}");
            }
        }

        public void PlayAmbient(string clipName)
        {
            if (audioClips.ContainsKey(clipName))
            {
                ambientSource.clip = audioClips[clipName];
                ambientSource.Play();
                Debug.Log($"🌊 Playing ambient: {clipName}");
            }
            else
            {
                Debug.LogWarning($"Ambient clip not found: {clipName}");
            }
        }

        public void PlaySkillSound(string skillId)
        {
            string soundName = $"skill_{skillId}";
            PlaySFX(soundName);
        }

        public void PlayCombatSound(string soundType)
        {
            string soundName = $"combat_{soundType}";
            PlaySFX(soundName);
        }

        public void PlayUISound(string uiAction)
        {
            string soundName = $"ui_{uiAction}";
            PlaySFX(soundName, 0.7f);
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void StopAmbient()
        {
            ambientSource.Stop();
        }

        public void StopAllSounds()
        {
            musicSource.Stop();
            sfxSource.Stop();
            ambientSource.Stop();
            
            foreach (var source in loopingSources.Values)
            {
                if (source != null)
                    source.Stop();
            }
        }

        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }

        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = masterVolume * musicVolume;
            
            if (sfxSource != null)
                sfxSource.volume = masterVolume * sfxVolume;
            
            if (ambientSource != null)
                ambientSource.volume = masterVolume * ambientVolume;
        }

        public void AddAudioClip(string name, AudioClip clip)
        {
            audioClips[name] = clip;
        }

        public bool HasAudioClip(string name)
        {
            return audioClips.ContainsKey(name);
        }
    }
}
