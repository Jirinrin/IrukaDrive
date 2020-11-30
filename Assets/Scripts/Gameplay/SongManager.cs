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
            
            if (_tickOnBeat)
                TickOnBeat();
        }

        private int SongPosBeatsRounded => Mathf.RoundToInt(songPosBeats);
        private int _prevSongPosBeatsRounded;

        private void TickOnBeat()
        {
            var beatNew = SongPosBeatsRounded;
            if (beatNew == _prevSongPosBeatsRounded)
                return;

            _prevSongPosBeatsRounded = beatNew;
            SfxManager.Instance.MakeTickSound();
        }
    
        public static event Action OnSongFinished;
    }
}
