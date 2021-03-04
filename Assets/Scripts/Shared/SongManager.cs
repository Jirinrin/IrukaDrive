using System;
using Shared.Domain;
using Tools;
using Tools.Commons;
using UnityEngine;

namespace Shared
{
    // todo: expect only some beatmap metadata, without all the notes and stuff
    [RequireComponent(typeof(AudioSource))]
    public class SongManager : Singleton<SongManager>
    {
        [NonSerialized] private Song _currentSong;
        [NonSerialized] public float songLength;
    
        private AudioSource _audioSource;

        [NonSerialized] public float songPosSec;
        [NonSerialized] public float songPosBeats;
    
        public float SongPosBeatsMod => Mathf.FloorToInt(songPosBeats % _currentSong.beatsPerBar);
        public float SongPosBars => Mathf.FloorToInt((songPosBeats - _currentSong.barOffset) / _currentSong.beatsPerBar);
        public float BeatTiming => songPosBeats % 1;

        public bool IsPlaying => _audioSource.isPlaying;

        private bool _songFinished;

        private bool _tickOnBeat;
    
        private void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlaySong(AudioClip song, float startTime = 0f)
        {
            _audioSource.clip = song;
            _audioSource.time = startTime;
            _audioSource.Play();
        }
        public void LoadSong(Song song, float startTime = 0f, bool tickOnBeat = false, float? finishTimestamp = null)
        {
            _tickOnBeat = tickOnBeat;
            
            _currentSong = song;
            songLength = finishTimestamp ?? song.audio.length;
            _audioSource.clip = _currentSong.audio;

            _songFinished = false;
            _audioSource.time = startTime;
            _audioSource.Play();
            UpdateSongState();
            OnSongStarted?.Invoke();
        }

        public void Stop()
        {
            if (_audioSource != null)
                _audioSource.Stop();
        }

        private void UpdateSongState()
        {
            if (_audioSource.isPlaying)
            {
                songPosSec = _audioSource.time;
                songPosBeats = _currentSong.SecToBeats(songPosSec);
            }
        
            if (!_songFinished && (songPosSec >= songLength || !_audioSource.isPlaying))
            {
                _songFinished = true;
                OnSongFinished?.Invoke();
            }
        }
        
        private void Update()
        {
            if (_currentSong == null)
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
