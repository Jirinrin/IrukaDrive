using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SongManager : Singleton<SongManager>
{
    [NonSerialized] private Beatmap _currentBeatmap;
    
    private AudioSource _audioSource;

    private float _secPerBeat;

    [NonSerialized] public float SongPosSec;
    public float SongPosBeats => (SongPosSec - _currentBeatmap.beatOffset) / _secPerBeat;
    public float SongPosBeatsMod => Mathf.FloorToInt(SongPosBeats % _currentBeatmap.beatsPerBar);
    public float SongPosBars => Mathf.FloorToInt((SongPosBeats - _currentBeatmap.barOffset) / _currentBeatmap.beatsPerBar);
    public float Timing => SongPosBeats % 1;

    private float _songDspTimeStart;

    private bool _songFinished;
    
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void LoadSong(Beatmap beatmap)
    {
        _currentBeatmap = beatmap;
        _secPerBeat = 60f / _currentBeatmap.bpm;
        _audioSource.clip = _currentBeatmap.song;

        _songDspTimeStart = (float)AudioSettings.dspTime;
        _songFinished = false;
        _audioSource.Play();
    }

    private void Update()
    {
        if (_audioSource.isPlaying)
            SongPosSec = (float)(AudioSettings.dspTime - _songDspTimeStart);
        
        if (SongPosSec > _currentBeatmap.song.length & !_songFinished)
        {
            _songFinished = true;
            OnSongFinished?.Invoke();
        }
    }
    
    public static event Action OnSongFinished;
}
