using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public int sourceCount = 3;
    private AudioSource mainSource;
    private List<AudioSource> audioSources = new List<AudioSource>();

    private void Awake()
    {
        mainSource = GetComponent<AudioSource>();
        for (int i = 0; i < sourceCount; i++)
        {
            CreateAudioSource();
        }
        mainSource.enabled = false;
    }

    private AudioSource CreateAudioSource()
    {
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.volume = mainSource.volume;
        newSource.pitch = mainSource.pitch;
        newSource.mute = mainSource.mute;
        newSource.bypassEffects = mainSource.bypassEffects;
        newSource.bypassListenerEffects = mainSource.bypassListenerEffects;
        newSource.bypassReverbZones = mainSource.bypassReverbZones;
        newSource.priority = mainSource.priority;
        newSource.loop = mainSource.loop;
        newSource.playOnAwake = mainSource.playOnAwake;
        newSource.spatialBlend = mainSource.spatialBlend; // 0 = 2D, 1 = 3D
        newSource.dopplerLevel = mainSource.dopplerLevel;
        newSource.spread = mainSource.spread;
        newSource.minDistance = mainSource.minDistance;
        newSource.maxDistance = mainSource.maxDistance;
        newSource.rolloffMode = mainSource.rolloffMode;
        audioSources.Add(newSource);

        return newSource;
    }

    public void playClip(AudioClip clip)
    {
        for (int i = 0; i < audioSources.Count; i++)
        {
            if (audioSources[i].isPlaying)
                continue;
            else
            {
                audioSources[i].clip = clip;
                audioSources[i].Play();
                return;
            }
        }

        var newSource = CreateAudioSource();
        newSource.clip = clip;
        newSource.Play();
    }
}
