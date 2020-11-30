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

        [NonSerialized] public float songPosSec;
        [NonSerialized] public float songPosBeats;
    
        public float SongPosBeatsMod => Mathf.FloorToInt(songPosBeats % _currentBeatmap.beatsPerBar);
        public float SongPosBars => Mathf.FloorToInt((songPosBeats - _currentBeatmap.barOffset) / _currentBeatmap.beatsPerBar);
        public float BeatTiming => songPosBeats % 1;

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
            BeatsPerSec = _currentBeatmap.bpm / 60f;
            _audioSource.clip = _currentBeatmap.song;

            _songFinished = false;
            _audioSource.Play();
        }

        private void Update()
        {
            if (_currentBeatmap == null)
                return;

            if (_audioSource.isPlaying)
            {
                songPosSec = _audioSource.time;
                songPosBeats = _currentBeatmap.SecToBeats(songPosSec);
            }
        
            if (songPosSec > _songFinishTimestamp & !_songFinished)
            {
                _songFinished = true;
                OnSongFinished?.Invoke();
            }
        }
    
        public static event Action OnSongFinished;
    }
}
