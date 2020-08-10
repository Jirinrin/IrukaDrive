using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongManager : MonoBehaviour
{
    [NonSerialized] public float SongBpm;
    
    private AudioSource _audioSource;

    private float _secPerBeat;

    [NonSerialized] public float SongPosSec;
    public float SongPosBeats => SongPosSec / _secPerBeat;
    public float Timing => SongPosBeats % 1;

    private float _songDspTimeStart;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void LoadSong(AudioClip clip, float bpm)
    {
        SongBpm = bpm;
        _secPerBeat = 60f / SongBpm;
        _audioSource.clip = clip;
        
        _songDspTimeStart = (float)AudioSettings.dspTime;
        _audioSource.Play();
    }

    private void Update()
    {
        if (_audioSource.isPlaying)
            SongPosSec = (float)(AudioSettings.dspTime - _songDspTimeStart);
    }
}
