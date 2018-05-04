using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MightyKingdom.Audio
{
    public enum AfterFadeAction { Stop, Pause, Nothing }

    public static class MKAudioExtensions
    {
        /// <summary>
        /// Fades an audio sources volume over time
        /// </summary>
        /// <param name="targetVolume">The volume to fade to</param>
        /// <param name="time">The time time fade over</param>
        /// <param name="action">What to do after the fade is complete</param>
        public static void FadeTo(this AudioSource audioSource, float targetVolume, float time, AfterFadeAction action = AfterFadeAction.Nothing)
        {
            //Stop any current fade routines on thiss
            if (coroutines.ContainsKey(audioSource))
            {
                if (coroutines[audioSource] != null)
                {
                    CoroutineHelper.Instance.StopCoroutine(coroutines[audioSource]);
                }
            }
            else
            {
                coroutines.Add(audioSource, null);
            }

            coroutines[audioSource] = CoroutineHelper.Instance.StartCoroutine(FadeToRoutine(audioSource, targetVolume, time, action));
        }

        //Fades AudioSource from it's current volume to targetVolume after fadeTime seconds
        static IEnumerator FadeToRoutine(AudioSource audioSource, float targetVolume, float fadeTime, AfterFadeAction action)
        {
            float startVolume = audioSource.volume;
            float time = 0;

            //Fade out previous music
            while (time < fadeTime)
            {
                time += Time.unscaledDeltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, targetVolume, time / fadeTime);
                yield return 0;
            }

            audioSource.volume = targetVolume;

            switch (action)
            {
                case AfterFadeAction.Stop:
                    audioSource.Stop();
                    break;
                case AfterFadeAction.Pause:
                    audioSource.Pause();
                    break;
                case AfterFadeAction.Nothing:
                default:
                    break;
            }

            //Complete this coroutine
            if (coroutines.ContainsKey(audioSource))
                coroutines[audioSource] = null;
        }

        static Dictionary<AudioSource, Coroutine> coroutines = new Dictionary<AudioSource, Coroutine>();
    }
}