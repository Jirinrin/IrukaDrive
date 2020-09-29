using System;
using Shared.Domain;
using Tools.Commons;
using UnityEngine;

namespace Gameplay
{
    [RequireComponent(typeof(AudioSource))]
    public class SongManager : Singleton<SongManager>
    {
        [NonSerialized] private Beatmap _currentBeatmap;
        private float _songFinishTimestamp;
    
        private AudioSource _audioSource;

        private float _secPerBeat;

        [NonSerialized] public float SongPosSec;
        [NonSerialized] public float SongPosBeats;
    
        public float SongPosBeatsMod => Mathf.FloorToInt(SongPosBeats % _currentBeatmap.beatsPerBar);
        public float SongPosBars => Mathf.FloorToInt((SongPosBeats - _currentBeatmap.barOffset) / _currentBeatmap.beatsPerBar);
        public float BeatTiming => SongPosBeats % 1;

        private float _songDspTimeStart;

        private bool _songFinished;
    
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void LoadSong(Beatmap beatmap)
        {
            _currentBeatmap = beatmap;
            _songFinishTimestamp = beatmap.finishTimestamp ?? beatmap.song.length;
            _secPerBeat = 60f / _currentBeatmap.bpm;
            _audioSource.clip = _currentBeatmap.song;

            _songDspTimeStart = (float)AudioSettings.dspTime;
            _songFinished = false;
            _audioSource.Play();
        }

        private void Update()
        {
            if (_currentBeatmap == null)
                return;

            if (_audioSource.isPlaying)
            {
                SongPosSec = (float) (AudioSettings.dspTime - _songDspTimeStart);
                SongPosBeats = (SongPosSec - _currentBeatmap.beatOffset) / _secPerBeat; 
            }
        
            if (SongPosSec > _songFinishTimestamp & !_songFinished)
            {
                _songFinished = true;
                OnSongFinished?.Invoke();
            }
        }
    
        public static event Action OnSongFinished;
    }
}
