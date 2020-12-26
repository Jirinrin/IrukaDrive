using System;
using Shared.Domain;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace Shared
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

        public bool IsPlaying => _audioSource.isPlaying;

        private bool _songFinished;

        private bool _tickOnBeat;
    
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void LoadSong(Beatmap beatmap, float startTime = 0f, bool tickOnBeat = false)
        {
            _tickOnBeat = tickOnBeat;
            
            _currentBeatmap = beatmap;
            _songFinishTimestamp = beatmap.finishTimestamp ?? beatmap.song.length;
            _audioSource.clip = _currentBeatmap.song;

            _songFinished = false;
            _audioSource.time = startTime;
            _audioSource.Play();
            UpdateSongState();
        }

        public void Stop()
        {
            _audioSource.Stop();
        }

        private void UpdateSongState()
        {
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
        
        private void Update()
        {
            if (_currentBeatmap == null)
                return;

            UpdateSongState();
            
            if (_tickOnBeat)
                TickOnBeat();
        }

        private int SongPosBeatsFloored => Mathf.FloorToInt(songPosBeats);
        private int _prevSongPosBeatsRounded;

        private void TickOnBeat()
        {
            var beatNew = SongPosBeatsFloored;
            if (beatNew == _prevSongPosBeatsRounded)
                return;

            _prevSongPosBeatsRounded = beatNew;
            SfxManager.Instance.MakeTickSound();
        }
    
        public static event Action OnSongFinished;
    }
}
