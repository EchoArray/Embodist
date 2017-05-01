using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceUtility : MonoBehaviour
{
    #region Values
    public bool PlayOnAwake = true;
    public bool UseSplitScreenVolumeScaling = true;
    public bool DestroyOnPlayComplete;

    [Space(15)]
    [Range(0, 1)]
    public float Volume = 1f;

    [Space(15)]
    public bool UseRandomPitchRange;
    public Vector2 PitchRange = new Vector2(0.9f, 1.2f);


    [Space(15)]
    public bool UseRandomVariations;
    public AudioClip[] AudioClipVariations;


    [Serializable]
    public class LoopFade
    {
        public bool UseLoopFadeIn;
        public float FadeInDuration;
        public AnimationCurve FadeInCurve;
        [HideInInspector]
        public float FadeInTime;

        [Space(15)]
        public bool UseLoopFadeOut;
        [HideInInspector]
        public bool FadingOut;
        public float FadeOutDuration;
        public AnimationCurve FadeOutCurve;
        [HideInInspector]
        public float FadeOutTime;
    }
    [Space(15)]
    public LoopFade LoopFading;


    private AudioSource _audioSource;
    private bool _hasPlayed;
    private float _playedTime;
    private bool _initialized;
    #endregion

    #region Unity Functions
    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if (_hasPlayed && DestroyOnPlayComplete && !_audioSource.isPlaying)
            Destroy(this.gameObject);

        ScaleVolume();
        UpdateFadeIn();
        UpdateFadeOut();
    }
    #endregion

    #region Functions
    private void Initialize()
    {
        if (_initialized)
            return;
        _initialized = true;


        _audioSource = this.gameObject.GetComponent<AudioSource>();
        _audioSource.outputAudioMixerGroup = AudioListenerUtility.Instance.AudioMixerGroup;

        // Randomly defines the audio clip for the audio source
        if (UseRandomVariations)
        {
            int audioSourceVariationIndex = UnityEngine.Random.Range(0, AudioClipVariations.Length);
            if (AudioClipVariations.Length > 0)
                _audioSource.clip = AudioClipVariations[audioSourceVariationIndex];
        }

        if (UseRandomPitchRange)
            _audioSource.pitch = UnityEngine.Random.Range(PitchRange.x, PitchRange.y);

        ScaleVolume();

        UpdateFadeIn();

        if (PlayOnAwake)
            Play();
    }

    private void ScaleVolume()
    {
        // Scales the volume of the audio source based on the closest camera
        if (!UseSplitScreenVolumeScaling)
            return;

        float shortestDistance = Mathf.Infinity;
        // Find the closest camera
        for (int i = 0; i < Globals.Instance.Containers.Cameras.childCount; i++)
        {
            Transform camera = Globals.Instance.Containers.Cameras.GetChild(i);
            float distance = Vector3.Distance(camera.position, this.transform.position);

            if (distance < shortestDistance)
                shortestDistance = distance;
        }

        // Scale volume
        AnimationCurve volumeCurve = _audioSource.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        float distanceScale = shortestDistance == _audioSource.maxDistance ? 0 : shortestDistance / _audioSource.maxDistance;
        _audioSource.volume = volumeCurve.Evaluate(distanceScale) * Volume;
    }


    private void UpdateFadeIn()
    {
        if (LoopFading.FadingOut || !LoopFading.UseLoopFadeIn || !_audioSource.isPlaying)
            return;
        if (_playedTime + LoopFading.FadeInDuration < Time.time)
            return;

        LoopFading.FadeInTime += Time.deltaTime;
        float fadeScale = LoopFading.FadeInTime / LoopFading.FadeInDuration;
        _audioSource.volume = LoopFading.FadeInCurve.Evaluate(fadeScale) * Volume;
    }
    private void UpdateFadeOut()
    {
        if (!LoopFading.UseLoopFadeOut || !LoopFading.FadingOut || !_audioSource.isPlaying)
            return;

        LoopFading.FadeOutTime += Time.deltaTime;
        float fadeScale = LoopFading.FadeOutTime / LoopFading.FadeOutDuration;
        _audioSource.volume = LoopFading.FadeOutCurve.Evaluate(fadeScale) * Volume;

        if (LoopFading.FadeOutCurve.Evaluate(fadeScale) == 0)
            _audioSource.Stop();
    }

    private void FadeOutLoop()
    {
        if (LoopFading.FadingOut)
            return;
        LoopFading.FadingOut = true;
    }


    public void Play()
    {
        Initialize();

        _audioSource.Play();
        _hasPlayed = true;
        _playedTime = Time.time;
    }

    public void Stop()
    {
        if (LoopFading.UseLoopFadeOut)
            FadeOutLoop();
        else
            _audioSource.Stop();
    }
    #endregion
}
