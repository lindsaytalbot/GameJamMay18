using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MightyKingdom
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewAudioBank", menuName = "MightyKingdom/AudioBank")]
    public class MKAudioBank : ScriptableObject
    {
        public static MKAudioBank LoadBank(string name)
        {
            return Resources.Load<MKAudioBank>(name);
        }

        [SerializeField]
        public List<AudioGroup> groups = new List<AudioGroup>();

        //Used in game
        public AudioGroupClip GetRandomClip(string groupName)
        {
            AudioGroup group = groups.FirstOrDefault(g => g.name == groupName);

            if (group == null || group.clips.Count == 0)
                return null;

            return group.clips.RandomItem();
        }

        public void UnloadBank()
        {
            foreach(AudioGroup group in groups)
            {
                group.UnloadClips();
            }
        }

#if UNITY_EDITOR
        //Adds a clip to a collection
        public void AddClip(string groupName, AudioClip clip)
        {
            if (clip == null || string.IsNullOrEmpty(groupName))
                return;

            AudioGroup group = groups.FirstOrDefault(g => g.name == groupName);

            if(group == null)
            {
                group = new AudioGroup(groupName);
                groups.Add(group);
            }

            group.AddClip(clip);
        }

        //Remove a clip from a collection
        public void RemoveClip(string groupName, AudioGroupClip clip)
        {
            AudioGroup group = groups.FirstOrDefault(g => g.name == groupName);

            if (group == null)
                return;

            group.RemoveClip(clip);

            if (group.clips.Count == 0)
                groups.Remove(group);
        }

#endif

        [System.Serializable]
        public class AudioGroup
        {
            public string name;
            public List<AudioGroupClip> clips;

            public AudioGroup(string name)
            {
                this.name = name;
                clips = new List<AudioGroupClip>();
            }

            public void AddClip(AudioClip clip, float volume = 1, bool isFinal = false)
            {
                AudioGroupClip c = new AudioGroupClip() { clip = clip, volume = volume, approved = isFinal };
                clips.Add(c);
            }

            public void RemoveClip(AudioGroupClip clip)
            {
                for (int i = clips.Count - 1; i >= 0; i--)
                {
                    if (clips[i] == clip)
                    {
                        clips.RemoveAt(i);
                        break;
                    }
                }
            }

            public void UnloadClips()
            {
                foreach (AudioGroupClip clip in clips)
                {
                    Resources.UnloadAsset(clip.clip);
                }
            }
        }

        [System.Serializable]
        public class AudioGroupClip
        {
            public AudioClip clip;
            public float volume;
            public bool approved;
        }
    }
}