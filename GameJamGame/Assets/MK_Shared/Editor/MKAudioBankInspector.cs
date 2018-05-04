using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MightyKingdom
{
    [CustomEditor(typeof(MKAudioBank))]
    public class MKAudioBankInspector : Editor
    {
        string newClipGroup = "";
        AudioClip newClip = null; 

        public override void OnInspectorGUI()
        {
            MKAudioBank bank = (MKAudioBank)target;
            
            EditorGUIUtility.labelWidth = 60f;

            foreach (var audioGroup in bank.groups)
            {
                EditorGUI.indentLevel = 0;

                string groupName = EditorGUILayout.TextField(audioGroup.name);
                if(groupName != audioGroup.name)
                {
                    audioGroup.name = groupName;
                    EditorUtility.SetDirty(bank);
                }

                List<MKAudioBank.AudioGroupClip> clips = audioGroup.clips;

                for (int i = 0; i < clips.Count; i++)
                {
                    EditorGUI.indentLevel = 2;
                    GUILayout.BeginHorizontal();
                    AudioClip c = (AudioClip)EditorGUILayout.ObjectField(new GUIContent(""), clips[i].clip, typeof(AudioClip), false);

                    //Clip updated
                    if (c != clips[i].clip)
                    {
                        clips[i].clip = c;
                        EditorUtility.SetDirty(bank);
                    }

                    EditorGUIUtility.labelWidth = 80f;
                    float volume = EditorGUILayout.FloatField(new GUIContent("Volume"), clips[i].volume, GUILayout.MaxWidth(115));
                    EditorGUIUtility.labelWidth = 60f;

                    //Volume updated
                    if (volume != clips[i].volume)
                    {
                        clips[i].volume = volume;
                        EditorUtility.SetDirty(bank);
                    }

                    bool approved = EditorGUILayout.Toggle("Final", clips[i].approved, GUILayout.MaxWidth(100));

                    //Approved updated
                    if(approved != clips[i].approved)
                    {
                        clips[i].approved = approved;
                        EditorUtility.SetDirty(bank);
                    }

                    //Remove this clip
                    if (GUILayout.Button("X", GUILayout.MaxWidth(50)))
                    {
                        bank.RemoveClip(groupName, clips[i]);
                        EditorUtility.SetDirty(bank);
                        break;
                    }

                    GUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.Space();

            EditorGUI.indentLevel = 1;
            GUILayout.BeginHorizontal();

            newClipGroup = EditorGUILayout.TextField(new GUIContent("Group: "), newClipGroup);
            newClip = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Clip "), newClip, typeof(AudioClip), false);
            if(GUILayout.Button("Add", GUILayout.MaxWidth(50)) && !string.IsNullOrEmpty(newClipGroup))
            {
                bank.AddClip(newClipGroup, newClip);
                newClip = null;
                EditorUtility.SetDirty(bank);
            }

            GUILayout.EndHorizontal();
        }
    }
}