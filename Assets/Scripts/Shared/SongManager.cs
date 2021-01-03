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
        [NonSerialized] public float songLength;
    
        private AudioSource _audioSource;

        [NonSerialized] public float songPosSec;
        [NonSerialized] public float songPosBeats;
    
        public float SongPosBeatsMod => Mathf.FloorToInt(songPosBeats % _currentBeatmap.beatsPerBar);
        public float SongPosBars => Mathf.FloorToInt((songPosBeats - _currentBeatmap.barOffset) / _currentBeatmap.beatsPerBar);
        public float BeatTiming => songPosBeats % 1;

        public bool IsPlaying => _audioSource.isPlaying;

        private bool _songFinished;

        private bool _tickOnBeat;
    
        private void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void LoadSong(Beatmap beatmap, float startTime = 0f, bool tickOnBeat = false)
        {
            _tickOnBeat = tickOnBeat;
            
            _currentBeatmap = beatmap;
            songLength = beatmap.finishTimestamp ?? beatmap.song.length;
            _audioSource.clip = _currentBeatmap.song;

            _songFinished = false;
            _audioSource.time = startTime;
            _audioSource.Play();
            UpdateSongState();
            OnSongStarted?.Invoke();
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
        
            if (songPosSec > songLength & !_songFinished)
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

        public static event Action OnSongStarted;
        public static event Action OnSongFinished;
    }
}
