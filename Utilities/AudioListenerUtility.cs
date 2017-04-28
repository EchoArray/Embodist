using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioListenerUtility : MonoBehaviour
{
    public static AudioListenerUtility Instance;

    public AudioMixerGroup AudioMixerGroup;

    public enum AudioMixerSnapshotType
    {
        Default,
        Space
    }
    public AudioMixerSnapshot DefaultAudioMixerSnapshot;
    public AudioMixerSnapshot SpaceAudioMixerSnapshot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            DestroyImmediate(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SetAudioMixerSnapShot(AudioMixerSnapshotType audioMixerSnapshotType)
    {
        switch (audioMixerSnapshotType)
        {
            case AudioMixerSnapshotType.Default:
                DefaultAudioMixerSnapshot.TransitionTo(0);
                break;
            case AudioMixerSnapshotType.Space:
                SpaceAudioMixerSnapshot.TransitionTo(0);
                break;
        }
    }
}
