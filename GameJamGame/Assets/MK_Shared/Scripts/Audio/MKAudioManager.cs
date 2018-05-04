using MightyKingdom.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace MightyKingdom
{
    public class MKAudioManager : MonoBehaviour
    {
        public enum FadeTypes { FadeOutFadeIn, CrossFade }

        //Lazily instantiate on first reference
        private static MKAudioManager _instance;

        private const float MUTE_VOLUME = -80f;
        private const float PLAYING_VOLUME = 0f;
        private const string MUSIC_SAVE_KEY = "MK_MusicEnabled";
        private const string SOUNDEFFECTS_SAVE_KEY = "MK_SoundEffectsEnabled";

        //Audio components
        private static AudioMixer audioMixer;
        private static AudioSource musicChannel1;
        private static AudioSource musicChannel2;
        private static AudioMixerGroup musicMixerGroup;
        private static AudioMixerGroup soundEffectsMixerGroup;
        private static PausedAudio musicPaused1;
        private static PausedAudio musicPaused2;

        private static bool musicEnabled;
        private static bool soundEffectsEnabled;
        private static AudioSource currentMusicSource;
        private static Coroutine musicFadeRoutine;
        private static List<AudioSource> audioSources = new List<AudioSource>();
        private static List<PausedAudio> pausedSources = new List<PausedAudio>();
        private static Dictionary<AudioClip, float> clipLastPlayTimes = new Dictionary<AudioClip, float>();
        private static Dictionary<string, MKAudioBank> audioBanks = new Dictionary<string, MKAudioBank>();
        private static List<QueuedMusic> queuedMusic = new List<QueuedMusic>();
        private static Coroutine queuedMusicWatcher;

        public static bool MusicEnabled
        {
            //Return true if music volume is not muted
            get
            {
                return musicEnabled;
            }
            //Set music volume to mute_volume if music is disabled
            set
            {
                audioMixer.SetFloat("Music Volume", value ? PLAYING_VOLUME : MUTE_VOLUME);
                MKPlayerPrefs.SetBool(MUSIC_SAVE_KEY, value);
                musicEnabled = value;
            }
        }

        public static bool SoundEffectsEnabled
        {
            //Return true if sound effects volume is not muted
            get
            {
                return soundEffectsEnabled;
            }
            //Set sound effects volume to mute_volume if music is disabled
            set
            {
                audioMixer.SetFloat("Sound Effects Volume", value ? PLAYING_VOLUME : MUTE_VOLUME);
                MKPlayerPrefs.SetBool(SOUNDEFFECTS_SAVE_KEY, value);
                soundEffectsEnabled = value;
            }
        }

        /// <summary>
        /// Adds and sets all the required game components
        /// Must be created before calling any functions
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            if (_instance != null)
                return;

            MKPlayerPrefs.Init();

            //Create GameObject and add components
            GameObject go = new GameObject();
            go.name = "MKAudioManager";
            _instance = go.AddComponent<MKAudioManager>();
            DontDestroyOnLoad(go);

            MKLog.Log("MKAudioManager created", "green");

            audioMixer = Resources.Load<AudioMixer>("MK_AudioMixer");
            musicMixerGroup = audioMixer.FindMatchingGroups("Music")[0];
            soundEffectsMixerGroup = audioMixer.FindMatchingGroups("SoundEffects")[0];

            musicChannel1 = go.AddComponent<AudioSource>();
            musicChannel1.outputAudioMixerGroup = musicMixerGroup;
            musicChannel2 = go.AddComponent<AudioSource>();
            musicChannel2.outputAudioMixerGroup = musicMixerGroup;
            currentMusicSource = musicChannel1;

            musicPaused1 = new PausedAudio()
            {
                audioSource = musicChannel1,
                startVolume = musicChannel1.volume
            };

            musicPaused2 = new PausedAudio()
            {
                audioSource = musicChannel2,
                startVolume = musicChannel2.volume
            };

            //Load enabled state from disk. 
            MusicEnabled = MKPlayerPrefs.GetBool(MUSIC_SAVE_KEY, true);
            SoundEffectsEnabled = MKPlayerPrefs.GetBool(SOUNDEFFECTS_SAVE_KEY, true);
        }

        //Hack to force audio to respect loaded volume
        private void Start()
        {
            MusicEnabled = MusicEnabled;
            SoundEffectsEnabled = SoundEffectsEnabled;
        }

        //Unloads the audio bank and all of it's clips from memory
        public static void UnloadBank(string bankName)
        {
            //Audio bank already unloaded
            if (!audioBanks.ContainsKey(bankName))
                return;

            MKAudioBank bank = audioBanks[bankName];

            if (bank != null)
                bank.UnloadBank();

            audioBanks.Remove(bankName);
        }

        public static void SetMusicSpeed(float speed)
        {
            musicChannel1.pitch = speed;
            musicChannel2.pitch = speed;
        }

        //Play the given music clip after the current music has finished playing. 
        public static void QueueMusic(AudioClip music, float fadeTime, float volume = 1, bool loop = true, FadeTypes fadeType = FadeTypes.FadeOutFadeIn)
        {
            if (queuedMusicWatcher != null)
                CoroutineHelper.Instance.StopCoroutine(queuedMusicWatcher);

            QueuedMusic q = new QueuedMusic();
            q.music = music;
            q.fadeTime = fadeTime;
            q.volume = volume;
            q.loop = loop;
            q.fadeType = fadeType;
            queuedMusic.Add(q);

            queuedMusicWatcher = CoroutineHelper.Instance.StartCoroutine(MusicQueueWatcher());
        }

        public static void ClearMusicQueue()
        {
            queuedMusic.Clear();
        }

        //Watches the currently playing music and checks if it's time to fade in the next queued item
        private static IEnumerator MusicQueueWatcher()
        {
            while (queuedMusic.Count > 0)
            {
                QueuedMusic q = queuedMusic[0];
                float timeLeft = currentMusicSource.clip.length - currentMusicSource.time;
                if (timeLeft <= q.fadeTime / 2f)
                {
                    queuedMusic.RemoveAt(0);
                    PlayMusic(q.music, q.fadeTime, q.volume, q.loop, q.fadeType);
                }
                yield return 0;
            }

            queuedMusicWatcher = null;
        }

        //Play the given music clip at volume, fading in over fadeTime seconds
        public static void PlayMusic(AudioClip music, float fadeTime, float volume = 1, bool loop = true, FadeTypes fadeType = FadeTypes.FadeOutFadeIn, bool clearQueue = true)
        {
            //Already playing this clip
            if (music == currentMusicSource.clip && currentMusicSource.isPlaying)
            {
                currentMusicSource.FadeTo(volume, fadeTime);
                return;
            }

            if (musicFadeRoutine != null)
            {
                CoroutineHelper.Instance.StopCoroutine(musicFadeRoutine);
                musicFadeRoutine = null;
            }

            if (currentMusicSource == musicChannel1)
            {
                musicChannel2.clip = music;
                musicChannel2.volume = 0;
                musicChannel2.loop = loop;
                musicChannel2.pitch = 1;
                musicChannel2.Play();
                currentMusicSource = musicChannel2;
                musicPaused2.startVolume = volume;
                musicFadeRoutine = CoroutineHelper.Instance.StartCoroutine(FadeMusicRoutine(musicChannel1, musicChannel2, fadeTime, volume, fadeType));
            }
            else
            {
                musicChannel1.clip = music;
                musicChannel1.volume = 0;
                musicChannel1.loop = loop;
                musicChannel1.pitch = 1;
                musicChannel1.Play();
                currentMusicSource = musicChannel1;
                currentMusicSource.FadeTo(volume, fadeTime);
                musicPaused1.startVolume = volume;
                musicFadeRoutine = CoroutineHelper.Instance.StartCoroutine(FadeMusicRoutine(musicChannel2, musicChannel1, fadeTime, volume, fadeType));
            }

            if (clearQueue)
                ClearMusicQueue();
        }

        //Play the given music clip at volume, fading in over fadeTime seconds
        public static void PlayMusic(string musicName, string bankName, float fadeTime, float volume = 1, bool loop = true, FadeTypes fadeType = FadeTypes.FadeOutFadeIn, bool clearQueue = true)
        {
            MKAudioBank.AudioGroupClip clip = GetClip(musicName, bankName);

            if (clip == null) //Sound not found
                return;

            AudioClip musicClip = clip.clip;

            //bad clip name
            if (musicClip == null)
            {
                MKLog.LogError(musicName + " does not exist in " + bankName);
                return;
            }

            PlayMusic(musicClip, fadeTime, clip.volume * volume, loop, fadeType, clearQueue);
        }


        //Fades between the previous and current music channels
        private static IEnumerator FadeMusicRoutine(AudioSource previous, AudioSource current, float fadeTime, float targetVolume, FadeTypes fadeType)
        {
            float previousStartVolume = previous.volume;

            //Fades out the previous music completely and then fade in the new music
            if (fadeType == FadeTypes.FadeOutFadeIn)
            {
                previous.FadeTo(0, fadeTime / 2f, AfterFadeAction.Stop);
                yield return new WaitForSecondsRealtime(fadeTime / 2f);
                current.FadeTo(targetVolume, fadeTime / 2f);
                yield return new WaitForSecondsRealtime(fadeTime / 2f);
            }
            //Fade out the previous music while fading in new music at the same time
            else if (fadeType == FadeTypes.CrossFade)
            {
                previous.FadeTo(0, fadeTime, AfterFadeAction.Stop);
                current.FadeTo(targetVolume, fadeTime);
                yield return new WaitForSecondsRealtime(fadeTime);
            }

            //Clear previous audio source and stop it
            previous.volume = 0;
            previous.Stop();
            previous.clip = null;

            current.volume = targetVolume;
            musicFadeRoutine = null;
        }

        /// <summary>
        /// Plays the given clip
        /// </summary>
        /// <param name="clip">The clip to play</param>
        /// <param name="volume">The volume to play it at</param>
        /// <param name="loop">Whether to loop the sound or not</param>
        /// <param name="minDoubleUpTime">Does not play the sound if another source is playing the same clip and has a time less than this</param>
        /// <returns>The audio source playing the clip</returns>
        public static AudioSource Play(AudioClip clip, bool loop = false, float volume = 1, float minDoubleUpTime = -1)
        {
            if (WillPlayClip(clip, minDoubleUpTime) == false)
                return null;

            AudioSource source = GetFreeAudioSource();
            source.clip = clip;
            source.volume = volume;
            source.loop = loop;
            source.transform.localPosition = Vector3.zero;
            source.spatialBlend = 0;
            source.pitch = 1;
            source.time = 0;
            source.Play();
            return source;
        }

        //Same as Play, but in world space
        public static AudioSource Play3D(AudioClip clip, Vector3 worldPos, bool loop = false, float volume = 1, float minDoubleUpTime = -1, float maxDistance = 500f)
        {
            AudioSource source = Play(clip, loop, volume, minDoubleUpTime);
            if (source != null)
            {
                source.transform.position = worldPos;
                source.spatialBlend = 1; //Sets audio to use world space volume drop off
                source.maxDistance = maxDistance; //The maximum distance the sound can be heard from 
            }
            return source;
        }

        //Same as Play, but in world space
        public static AudioSource Play3D(string clipName, string bankName, Vector3 worldPos, bool loop = false, float volume = 1, float minDoubleUpTime = -1, float maxDistance = 500f)
        {
            AudioSource source = Play(clipName, bankName, loop, volume, minDoubleUpTime);
            if (source != null)
            {
                source.transform.position = worldPos;
                source.spatialBlend = 1; //Sets audio to use world space volume drop off
                source.maxDistance = maxDistance; //The maximum distance the sound can be heard from 
            }
            return source;
        }

        public static AudioSource Play(string clipName, string bankName, bool loop = false, float volume = 1, float minDoubleUpTime = -1)
        {
            MKAudioBank.AudioGroupClip clip = GetClip(clipName, bankName);

            if (clip == null) //Sound not found
                return null;

            return Play(clip.clip, loop, clip.volume * volume, minDoubleUpTime);
        }

        //Same as play, but delayed by the given delay in seconds
        public static Coroutine PlayDelayed(AudioClip clip, float delay, bool loop = false, float volume = 1, float minDoubleUpTime = -1)
        {
            return CoroutineHelper.Instance.StartCoroutine(PlayDelayedRoutine(clip, delay, loop, volume, minDoubleUpTime));
        }

        //Same as play, but delayed by the given delay in seconds
        public static Coroutine PlayDelayed(string clipName, string bankName, float delay, bool loop = false, float volume = 1, float minDoubleUpTime = -1)
        {
            MKAudioBank.AudioGroupClip clip = GetClip(clipName, bankName);

            if (clip == null) //Sound not found
                return null;

            return CoroutineHelper.Instance.StartCoroutine(PlayDelayedRoutine(clip.clip, delay, loop, clip.volume * volume, minDoubleUpTime));
        }

        public static IEnumerator PlayDelayedRoutine(AudioClip clip, float delay, bool loop = false, float volume = 1, float minDoubleUpTime = -1)
        {
            yield return new WaitForSeconds(delay);
            Play(clip, loop, volume, minDoubleUpTime);
        }

        //Same as PlayDelayed, but in world space
        public static Coroutine PlayDelayed3D(AudioClip clip, float delay, Vector3 worldPos, bool loop = false, float volume = 1, float minDoubleUpTime = -1, float maxDistance = 500f)
        {
            return CoroutineHelper.Instance.StartCoroutine(PlayDelayedRoutine3D(clip, delay, worldPos, loop, volume, minDoubleUpTime, maxDistance));
        }

        //Same as PlayDelayed, but in world space
        public static Coroutine PlayDelayed3D(string clipName, string bankName, float delay, Vector3 worldPos, bool loop = false, float volume = 1, float minDoubleUpTime = -1, float maxDistance = 500f)
        {
            MKAudioBank.AudioGroupClip clip = GetClip(clipName, bankName);

            if (clip == null)
                return null;

            return CoroutineHelper.Instance.StartCoroutine(PlayDelayedRoutine3D(clip.clip, delay, worldPos, loop, clip.volume * volume, minDoubleUpTime, maxDistance));
        }

        private static IEnumerator PlayDelayedRoutine3D(AudioClip clip, float delay, Vector3 worldPos, bool loop, float volume, float minDoubleUpTime, float maxDistance)
        {
            yield return new WaitForSeconds(delay);
            AudioSource source = Play(clip, loop, volume, minDoubleUpTime);
            if (source != null)
            {
                source.transform.position = worldPos;
                source.spatialBlend = 1; //Sets audio to use world space volume drop off
                source.maxDistance = maxDistance; //The maximum distance the sound can be heard from 
            }
        }

        /// <summary>
        /// Returns false if the clip has been played in the last <paramref name="minOverlapTime"/> seconds
        /// </summary>
        /// <param name="clip">The clip to check</param>
        /// <param name="minOverlapTime"></param>
        /// <returns></returns>
        public static bool CanPlayClip(AudioClip clip, float minOverlapTime)
        {
            if (clip == null)
                return false;

            float overlapTime = Time.realtimeSinceStartup - minOverlapTime;
            float lastPlayTime;
            if (clipLastPlayTimes.TryGetValue(clip, out lastPlayTime) && lastPlayTime > overlapTime)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Same as CanPlayClip, but with intention to actually play the clip
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="minOverlapTime"></param>
        /// <returns></returns>
        private static bool WillPlayClip(AudioClip clip, float minOverlapTime)
        {
            if (clip == null)
                return false;

            bool result = CanPlayClip(clip, minOverlapTime);
            if (result)
            {
                clipLastPlayTimes[clip] = Time.realtimeSinceStartup;
            }
            return result;
        }

        /// <summary>
        /// Finds an audio source that is not currently in use and returns it
        /// </summary>
        /// <returns></returns>
        private static AudioSource GetFreeAudioSource()
        {
            foreach (AudioSource source in audioSources)
            {
                if (source.clip == null || source.isPlaying == false)
                    return source;
            }

            GameObject go = new GameObject("AudioSource");
            go.transform.SetParent(_instance.transform);
            AudioSource newSource = go.AddComponent<AudioSource>();
            newSource.outputAudioMixerGroup = soundEffectsMixerGroup;
            audioSources.Add(newSource);
            return newSource;
        }

        public static void PauseAll(float fadeTime = 0)
        {
            PauseMusic(fadeTime);
            PauseSounds(fadeTime);
        }

        public static void ResumeAll(float fadeTime = 0)
        {
            ResumeMusic(fadeTime);
            ResumeSounds(fadeTime);
        }

        public static void StopAll(float fadeTime = 0)
        {
            StopMusic(fadeTime);
            StopSounds(fadeTime);
        }

        public static void PauseMusic(float fadeTime = 0)
        {
            musicChannel1.FadeTo(0, fadeTime, AfterFadeAction.Pause);
            musicChannel2.FadeTo(0, fadeTime, AfterFadeAction.Pause);
        }

        public static void ResumeMusic(float fadeTime = 0)
        {
            musicChannel1.UnPause();
            musicChannel1.FadeTo(musicPaused1.startVolume, fadeTime);
            musicChannel2.UnPause();
            musicChannel2.FadeTo(musicPaused2.startVolume, fadeTime);
        }

        public static void StopMusic(float fadeTime = 0)
        {
            musicChannel1.FadeTo(0, fadeTime, AfterFadeAction.Stop);
            musicChannel2.FadeTo(0, fadeTime, AfterFadeAction.Stop);
        }

        public static void PauseSounds(float fadeTime = 0)
        {
            //Pause all currently playing sounds
            foreach (AudioSource source in audioSources)
            {
                if (source.clip == null || source.isPlaying == false)
                    continue;
                PausedAudio audio = new PausedAudio()
                {
                    audioSource = source,
                    startVolume = source.volume,
                };
                pausedSources.Add(audio);
                source.Pause();
            }

            //Removed paused sources from the pool
            foreach (PausedAudio audio in pausedSources)
            {
                audioSources.Remove(audio.audioSource);
            }
        }

        public static void ResumeSounds(float fadeTime = 0)
        {
            foreach (PausedAudio audio in pausedSources)
            {
                audio.audioSource.UnPause();
                audio.audioSource.FadeTo(audio.startVolume, fadeTime);
                audioSources.Add(audio.audioSource);
            }

            pausedSources.Clear();
        }

        public static void StopSounds(float fadeTime = 0)
        {
            foreach (PausedAudio audio in pausedSources)
            {
                audioSources.Add(audio.audioSource);
            }

            pausedSources.Clear();

            foreach (AudioSource source in audioSources)
            {
                source.FadeTo(0, fadeTime, AfterFadeAction.Stop);
            }
        }

        //Unloads all clips that are not currently playing
        public static void UnloadUnusedAudio()
        {
            foreach (AudioSource source in audioSources)
            {
                if (!source.isPlaying)
                    Resources.UnloadAsset(source.clip);
            }
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused)
            {
                PauseAll();
            }
            else
            {
                ResumeAll();
            }
        }

        private static MKAudioBank.AudioGroupClip GetClip(string clipName, string bankName)
        {
            if (!audioBanks.ContainsKey(bankName))
                audioBanks[bankName] = MKAudioBank.LoadBank(bankName);

            //Bad audio bank name
            if (audioBanks[bankName] == null)
            {
                MKLog.LogError("AudioBank " + bankName + " does not exists");
                return null;
            }

            MKAudioBank.AudioGroupClip agc = audioBanks[bankName].GetRandomClip(clipName);

            //bad clip name
            if (agc == null)
            {
                MKLog.LogError(clipName + " does not exist in " + bankName);
                return null;
            }

            return agc;
        }
    }

    struct PausedAudio
    {
        public AudioSource audioSource;
        public float startVolume;
    }

    struct QueuedMusic
    {
        public AudioClip music;
        public float fadeTime;
        public float volume;
        public bool loop;
        public MKAudioManager.FadeTypes fadeType;
    }
}