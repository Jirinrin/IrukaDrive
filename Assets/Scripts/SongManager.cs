using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongManager : MonoBehaviour
{
    public float songBpm;
    
    private AudioSource _audioSource;

    private float _secPerBeat;

    [NonSerialized] public float SongPosSec;
    public float SongPosBeats => SongPosSec / _secPerBeat;
    public float Timing => SongPosBeats % 1;

    private float _songDspTimeStart;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        _secPerBeat = 60f / songBpm;
        _songDspTimeStart = (float)AudioSettings.dspTime;
        
        _audioSource.Play();
    }

    private void Update()
    {
        SongPosSec = (float)(AudioSettings.dspTime - _songDspTimeStart);
    }
}
