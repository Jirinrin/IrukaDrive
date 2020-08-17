using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongManager : MonoBehaviour
{
    [NonSerialized] public Beatmap Beatmap;
    
    private AudioSource _audioSource;

    private float _secPerBeat;

    [NonSerialized] public float SongPosSec;
    public float SongPosBeats => SongPosSec - Beatmap.beatOffset / _secPerBeat;
    public float SongPosBeatsMod => Mathf.FloorToInt(SongPosBeats % Beatmap.beatsPerBar);
    public float SongPosBars => Mathf.FloorToInt((SongPosBeats - Beatmap.barOffset) / Beatmap.beatsPerBar);
    public float Timing => SongPosBeats % 1;

    private float _songDspTimeStart;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void LoadSong(Beatmap beatmap)
    {
        Beatmap = beatmap;
        _secPerBeat = 60f / Beatmap.bpm;
        _audioSource.clip = Beatmap.song;
        
        _songDspTimeStart = (float)AudioSettings.dspTime;
        _audioSource.Play();
    }

    private void Update()
    {
        if (_audioSource.isPlaying)
            SongPosSec = (float)(AudioSettings.dspTime - _songDspTimeStart);
    }
}
