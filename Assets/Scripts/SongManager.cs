using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongManager : MonoBehaviour
{
    [NonSerialized] private Beatmap CurrentBeatmap;
    
    private AudioSource _audioSource;

    private float _secPerBeat;

    [NonSerialized] public float SongPosSec;
    public float SongPosBeats => (SongPosSec - CurrentBeatmap.beatOffset) / _secPerBeat;
    public float SongPosBeatsMod => Mathf.FloorToInt(SongPosBeats % CurrentBeatmap.beatsPerBar);
    public float SongPosBars => Mathf.FloorToInt((SongPosBeats - CurrentBeatmap.barOffset) / CurrentBeatmap.beatsPerBar);
    public float Timing => SongPosBeats % 1;

    private float _songDspTimeStart;

    private bool _songFinished;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void LoadSong(Beatmap beatmap)
    {
        CurrentBeatmap = beatmap;
        _secPerBeat = 60f / CurrentBeatmap.bpm;
        _audioSource.clip = CurrentBeatmap.song;
        
        _songDspTimeStart = (float)AudioSettings.dspTime;
        _audioSource.Play();
    }

    private void Update()
    {
        if (_audioSource.isPlaying)
            SongPosSec = (float)(AudioSettings.dspTime - _songDspTimeStart);
        
        if (SongPosSec > CurrentBeatmap.song.length & !_songFinished)
        {
            _songFinished = true;
            OnSongFinished?.Invoke();
        }
    }
    
    public static event Action OnSongFinished;
}
